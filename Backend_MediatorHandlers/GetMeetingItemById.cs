#nullable enable
using FluentValidation;
using MediatR;
using MeetingItems.Core.AppServices.Abstractions;
using MeetingItems.Core.AppServices.Common;
using MeetingItems.Core.Domain.Repositories;
using MeetingItems.Infra.Common.Validations;

namespace MeetingItems.Core.AppServices.Features.MeetingItems;

public class GetMeetingItemById
{
    public class Validator : AbstractValidator<Query>
    {
        private readonly IMeetingItemRepository _meetingItemRepository;

        public Validator(IMeetingItemRepository meetingItemRepository)
        {
            _meetingItemRepository = meetingItemRepository ?? throw new ArgumentNullException(nameof(meetingItemRepository));

            RuleFor(q => q.MeetingItemId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required)
                .MustAsync(_meetingItemRepository.ExistsAsync)
                .WithMessage(BusinessErrorMessage.NotFound);
        }
    }

    public record Query(string MeetingItemId) : IQuery<Response>;

    public record Response(
        string Id,
        string DecisionBoardId,
        string DecisionBoardName,
        string TemplateId,
        string TemplateName,
        string Topic,
        string Purpose,
        string Outcome,
        string DigitalProduct,
        int DurationMinutes,
        string RequestorUserId,
        string OwnerPresenterUserId,
        string? SponsorUserId,
        string Status,
        DateTime? SubmissionDate,
        DateTime CreatedDate,
        List<ActiveFieldDto> ActiveFields,
        List<HistoricalFieldDto> HistoricalFields,
        List<DocumentDto> Documents);

    public record ActiveFieldDto(
        string FieldDefinitionId,
        string FieldName,
        string Label,
        string FieldType,
        bool IsRequired,
        string Category,
        string? TextValue,
        decimal? NumberValue,
        DateTime? DateValue,
        bool? BooleanValue,
        string? JsonValue);

    public record HistoricalFieldDto(
        string FieldName,
        string Label,
        string Value,
        DateTime? DeactivatedDate,
        string? DeactivationReason);

    public record DocumentDto(
        string Id,
        string OriginalFileName,
        string StoredFileName,
        string VersionNumber,
        long FileSizeBytes,
        DateTime UploadDate,
        string UploadedBy);

    internal class Handler(
        IMeetingItemRepository meetingItemRepository) : IQueryHandler<Query, Response>
    {
        public async Task<BusinessResult<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var meetingItem = await meetingItemRepository.GetByIdWithAllDetailsAsync(query.MeetingItemId, cancellationToken);

            if (meetingItem == null)
                return BusinessResult.Failure<Response>(BusinessErrorMessage.NotFound);

            // Separate active and historical fields
            var activeFields = meetingItem.FieldValues
                .Where(fv => fv.FieldDefinition.IsActive)
                .Select(fv => new ActiveFieldDto(
                    fv.FieldDefinitionId,
                    fv.FieldDefinition.FieldName,
                    fv.FieldDefinition.Label,
                    fv.FieldDefinition.FieldType,
                    fv.FieldDefinition.IsRequired,
                    fv.FieldDefinition.Category,
                    fv.TextValue,
                    fv.NumberValue,
                    fv.DateValue,
                    fv.BooleanValue,
                    fv.JsonValue))
                .ToList();

            var historicalFields = meetingItem.FieldValues
                .Where(fv => !fv.FieldDefinition.IsActive)
                .Select(fv => new HistoricalFieldDto(
                    fv.FieldDefinition.FieldName,
                    fv.FieldDefinition.Label,
                    GetFieldValueAsString(fv),
                    fv.FieldDefinition.DeactivatedDate,
                    fv.FieldDefinition.DeactivationReason))
                .ToList();

            var documents = meetingItem.Documents
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.UploadDate)
                .Select(d => new DocumentDto(
                    d.Id,
                    d.OriginalFileName,
                    d.StoredFileName,
                    d.VersionNumber,
                    d.FileSizeBytes,
                    d.UploadDate,
                    d.UploadedBy))
                .ToList();

            var response = new Response(
                meetingItem.Id,
                meetingItem.DecisionBoardId,
                meetingItem.DecisionBoard?.Name ?? string.Empty,
                meetingItem.TemplateId,
                meetingItem.Template?.Name ?? string.Empty,
                meetingItem.Topic,
                meetingItem.Purpose,
                meetingItem.Outcome,
                meetingItem.DigitalProduct,
                meetingItem.DurationMinutes,
                meetingItem.RequestorUserId,
                meetingItem.OwnerPresenterUserId,
                meetingItem.SponsorUserId,
                meetingItem.Status,
                meetingItem.SubmissionDate,
                meetingItem.CreatedDate,
                activeFields,
                historicalFields,
                documents);

            return BusinessResult.Success(response);
        }

        private static string GetFieldValueAsString(MeetingItemFieldValue fieldValue)
        {
            return fieldValue.FieldDefinition.FieldType switch
            {
                "Text" or "TextArea" or "Email" => fieldValue.TextValue ?? string.Empty,
                "Number" => fieldValue.NumberValue?.ToString() ?? string.Empty,
                "Date" => fieldValue.DateValue?.ToString("yyyy-MM-dd") ?? string.Empty,
                "YesNo" => fieldValue.BooleanValue?.ToString() ?? string.Empty,
                "Dropdown" or "Radio" => fieldValue.TextValue ?? string.Empty,
                "MultiSelect" => fieldValue.JsonValue ?? string.Empty,
                _ => string.Empty
            };
        }
    }
}
