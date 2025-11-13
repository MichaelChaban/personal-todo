#nullable enable
using FluentValidation;
using MediatR;
using MeetingItems.Core.AppServices.Abstractions;
using MeetingItems.Core.AppServices.Common;
using MeetingItems.Core.Domain.MeetingItems;
using MeetingItems.Core.Domain.Repositories;
using MeetingItems.Infra.Common.Validations;

namespace MeetingItems.Core.AppServices.Features.MeetingItems;

public class UpdateMeetingItem
{
    public class Validator : AbstractValidator<Command>
    {
        private readonly IMeetingItemRepository _meetingItemRepository;
        private readonly ITemplateRepository _templateRepository;
        private readonly IUserSessionProvider _userSessionProvider;

        public Validator(
            IMeetingItemRepository meetingItemRepository,
            ITemplateRepository templateRepository,
            IUserSessionProvider userSessionProvider)
        {
            _meetingItemRepository = meetingItemRepository ?? throw new ArgumentNullException(nameof(meetingItemRepository));
            _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
            _userSessionProvider = userSessionProvider ?? throw new ArgumentNullException(nameof(userSessionProvider));

            RuleFor(c => c)
                .MustAsync(AllowedToUpdateMeetingItem)
                .WithMessage(BusinessErrorMessage.NotAllowedToUpdateMeetingItem);

            RuleFor(c => c.MeetingItemId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required)
                .MustAsync(_meetingItemRepository.ExistsAsync)
                .WithMessage(BusinessErrorMessage.NotFound);

            RuleFor(c => c.Topic)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required)
                .MaximumLength(300)
                .WithMessage("Topic must not exceed 300 characters");

            RuleFor(c => c.Purpose)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required)
                .MinimumLength(10)
                .WithMessage("Purpose must be at least 10 characters");

            RuleFor(c => c.Outcome)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required)
                .Must(BeValidOutcome)
                .WithMessage("Outcome must be 'Decision', 'Discussion', or 'Information'");

            RuleFor(c => c.DigitalProduct)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required)
                .MaximumLength(200)
                .WithMessage("Digital Product must not exceed 200 characters");

            RuleFor(c => c.DurationMinutes)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required)
                .GreaterThan(0)
                .WithMessage("Duration must be greater than 0")
                .LessThanOrEqualTo(480)
                .WithMessage("Duration must not exceed 480 minutes");

            RuleFor(c => c.OwnerPresenterUserId)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required);

            RuleFor(c => c)
                .MustAsync(ValidateDynamicFields)
                .WithMessage("Dynamic field validation failed");
        }

        private async Task<bool> AllowedToUpdateMeetingItem(Command command, CancellationToken cancellationToken)
        {
            var currentUserId = _userSessionProvider.GetUserId();
            var meetingItem = await _meetingItemRepository.GetByIdAsync(command.MeetingItemId, cancellationToken);

            // Only requestor or owner can update, and only if not yet discussed
            return meetingItem != null &&
                   (meetingItem.RequestorUserId == currentUserId || meetingItem.OwnerPresenterUserId == currentUserId) &&
                   meetingItem.Status != "Discussed" &&
                   meetingItem.Status != "Denied";
        }

        private bool BeValidOutcome(string outcome)
        {
            var validOutcomes = new[] { "Decision", "Discussion", "Information" };
            return validOutcomes.Contains(outcome);
        }

        private async Task<bool> ValidateDynamicFields(Command command, CancellationToken cancellationToken)
        {
            var meetingItem = await _meetingItemRepository.GetByIdWithTemplateAsync(command.MeetingItemId, cancellationToken);

            if (meetingItem?.Template == null)
                return false;

            var validator = new CreateMeetingItem.DynamicFieldValidator(meetingItem.Template);
            var result = await validator.ValidateAsync(command.FieldValues ?? new List<CreateMeetingItem.FieldValueDto>(), cancellationToken);

            return result.IsValid;
        }
    }

    public record Command(
        string MeetingItemId,
        string Topic,
        string Purpose,
        string Outcome,
        string DigitalProduct,
        int DurationMinutes,
        string OwnerPresenterUserId,
        string? SponsorUserId,
        List<CreateMeetingItem.FieldValueDto>? FieldValues = null) : ICommand<Response>;

    public record Response(string MeetingItemId);

    internal class Handler(
        IMeetingItemRepository meetingItemRepository,
        IUnitOfWork unitOfWork) : ICommandHandler<Command, Response>
    {
        public async Task<BusinessResult<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var meetingItem = await meetingItemRepository.GetByIdWithFieldValuesAsync(command.MeetingItemId, cancellationToken);

            if (meetingItem == null)
                return BusinessResult.Failure<Response>(BusinessErrorMessage.NotFound);

            // Update static fields
            meetingItem.UpdateDetails(
                command.Topic,
                command.Purpose,
                command.Outcome,
                command.DigitalProduct,
                command.DurationMinutes,
                command.OwnerPresenterUserId,
                command.SponsorUserId);

            // Update dynamic field values
            if (command.FieldValues?.Any() == true)
            {
                UpdateFieldValues(meetingItem, command.FieldValues);
            }

            // Increment version
            meetingItem.IncrementVersion();

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return BusinessResult.Success(new Response(meetingItem.Id));
        }

        private static void UpdateFieldValues(MeetingItem meetingItem, List<CreateMeetingItem.FieldValueDto> fieldValuesDto)
        {
            foreach (var dto in fieldValuesDto)
            {
                var existingFieldValue = meetingItem.FieldValues
                    .FirstOrDefault(fv => fv.FieldDefinition.FieldName == dto.FieldName);

                if (existingFieldValue != null)
                {
                    // Update existing field value
                    existingFieldValue.UpdateValue(
                        dto.TextValue,
                        dto.NumberValue,
                        dto.DateValue,
                        dto.BooleanValue,
                        dto.JsonValue);
                }
                else
                {
                    // Add new field value (for fields added to template after item was created)
                    var fieldDefinition = meetingItem.Template.FieldDefinitions
                        .FirstOrDefault(f => f.FieldName == dto.FieldName && f.IsActive);

                    if (fieldDefinition != null)
                    {
                        var newFieldValue = MeetingItemFieldValue.Create(
                            meetingItem.Id,
                            fieldDefinition.Id,
                            dto.TextValue,
                            dto.NumberValue,
                            dto.DateValue,
                            dto.BooleanValue,
                            dto.JsonValue);

                        meetingItem.AddFieldValue(newFieldValue);
                    }
                }
            }
        }
    }
}
