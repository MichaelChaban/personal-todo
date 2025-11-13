#nullable enable
using FluentValidation;
using MediatR;
using MeetingItems.Core.AppServices.Abstractions;
using MeetingItems.Core.AppServices.Common;
using MeetingItems.Core.Domain.MeetingItems;
using MeetingItems.Core.Domain.Repositories;
using MeetingItems.Infra.Common.Validations;

namespace MeetingItems.Core.AppServices.Features.MeetingItems;

public class UpdateMeetingItemStatus
{
    public class Validator : AbstractValidator<Command>
    {
        private readonly IMeetingItemRepository _meetingItemRepository;
        private readonly IUserSessionProvider _userSessionProvider;

        public Validator(
            IMeetingItemRepository meetingItemRepository,
            IUserSessionProvider userSessionProvider)
        {
            _meetingItemRepository = meetingItemRepository ?? throw new ArgumentNullException(nameof(meetingItemRepository));
            _userSessionProvider = userSessionProvider ?? throw new ArgumentNullException(nameof(userSessionProvider));

            RuleFor(c => c)
                .MustAsync(AllowedToUpdateStatus)
                .WithMessage("Only secretary or chair can update meeting item status");

            RuleFor(c => c.MeetingItemId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required)
                .MustAsync(_meetingItemRepository.ExistsAsync)
                .WithMessage(BusinessErrorMessage.NotFound);

            RuleFor(c => c.NewStatus)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required)
                .Must(BeValidStatus)
                .WithMessage("Invalid status. Must be: Submitted, Proposed, Planned, Discussed, or Denied");

            RuleFor(c => c)
                .MustAsync(BeValidStatusTransition)
                .WithMessage("Invalid status transition");

            RuleFor(c => c.Comment)
                .MaximumLength(1000)
                .WithMessage("Comment must not exceed 1000 characters")
                .When(c => !string.IsNullOrWhiteSpace(c.Comment));

            RuleFor(c => c.DenialReason)
                .NotEmpty()
                .WithMessage("Denial reason is required when status is Denied")
                .MaximumLength(1000)
                .WithMessage("Denial reason must not exceed 1000 characters")
                .When(c => c.NewStatus == "Denied");
        }

        private async Task<bool> AllowedToUpdateStatus(Command command, CancellationToken cancellationToken)
        {
            // Check if user has secretary or chair role
            // This should be implemented based on your role management system
            var userRoles = _userSessionProvider.GetUserRoles();
            return userRoles?.Any(r => r == "Secretary" || r == "Chair") == true;
        }

        private bool BeValidStatus(string status)
        {
            var validStatuses = new[] { "Submitted", "Proposed", "Planned", "Discussed", "Denied" };
            return validStatuses.Contains(status);
        }

        private async Task<bool> BeValidStatusTransition(Command command, CancellationToken cancellationToken)
        {
            var meetingItem = await _meetingItemRepository.GetByIdAsync(command.MeetingItemId, cancellationToken);

            if (meetingItem == null)
                return false;

            // Define valid status transitions
            var validTransitions = new Dictionary<string, List<string>>
            {
                { "Submitted", new List<string> { "Proposed", "Denied" } },
                { "Proposed", new List<string> { "Planned", "Denied" } },
                { "Planned", new List<string> { "Discussed", "Denied" } },
                { "Discussed", new List<string>() }, // Terminal state
                { "Denied", new List<string>() } // Terminal state
            };

            if (!validTransitions.ContainsKey(meetingItem.Status))
                return false;

            return validTransitions[meetingItem.Status].Contains(command.NewStatus);
        }
    }

    public record Command(
        string MeetingItemId,
        string NewStatus,
        string? Comment = null,
        string? DenialReason = null) : ICommand<Response>;

    public record Response(
        string MeetingItemId,
        string PreviousStatus,
        string NewStatus,
        DateTime ChangedDate);

    internal class Handler(
        IMeetingItemRepository meetingItemRepository,
        IUserSessionProvider userSessionProvider,
        IUnitOfWork unitOfWork) : ICommandHandler<Command, Response>
    {
        public async Task<BusinessResult<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var meetingItem = await meetingItemRepository.GetByIdAsync(command.MeetingItemId, cancellationToken);

            if (meetingItem == null)
                return BusinessResult.Failure<Response>(BusinessErrorMessage.NotFound);

            var previousStatus = meetingItem.Status;
            var changedBy = userSessionProvider.GetUserId() ?? throw new UnauthorizedAccessException();

            // Update status and add to history
            meetingItem.UpdateStatus(command.NewStatus, command.DenialReason);

            // Create status history record
            var statusHistory = MeetingItemStatusHistory.Create(
                meetingItem.Id,
                previousStatus,
                command.NewStatus,
                command.Comment,
                changedBy);

            meetingItem.AddStatusHistory(statusHistory);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return BusinessResult.Success(new Response(
                meetingItem.Id,
                previousStatus,
                command.NewStatus,
                DateTime.UtcNow));
        }
    }
}
