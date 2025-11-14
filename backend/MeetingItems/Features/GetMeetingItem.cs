#nullable enable
using FluentValidation;
using MediatR;
using MeetingItemsApp.Common;
using MeetingItemsApp.Common.Abstractions;
using MeetingItemsApp.MeetingItems.Repositories;

namespace MeetingItemsApp.MeetingItems.Features;

public class GetMeetingItem
{
    public class Validator : AbstractValidator<Query>
    {
        private readonly IMeetingItemRepository _meetingItemRepository;

        public Validator(IMeetingItemRepository meetingItemRepository)
        {
            _meetingItemRepository = meetingItemRepository ?? throw new ArgumentNullException(nameof(meetingItemRepository));

            RuleFor(q => q.Id)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required)
                .MustAsync(_meetingItemRepository.ExistsAsync)
                .WithMessage(BusinessErrorMessage.NotFound);
        }
    }

    public record Query(string Id) : IQuery<Response>;

    public record Response(
        string Id,
        string Topic,
        string Purpose,
        string Outcome,
        string DigitalProduct,
        int Duration,
        string Requestor,
        string OwnerPresenter,
        string? Sponsor,
        DateTime SubmissionDate,
        string Status,
        string DecisionBoardId,
        string? TemplateId,
        List<DocumentDto> Documents,
        DateTime CreatedAt,
        string CreatedBy);

    public record DocumentDto(
        string Id,
        string FileName,
        long FileSize,
        string ContentType,
        DateTime UploadDate,
        string UploadedBy);

    internal class Handler(
        IMeetingItemRepository meetingItemRepository) : IQueryHandler<Query, Response>
    {
        public async Task<BusinessResult<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var meetingItem = await meetingItemRepository.GetByIdWithDocumentsAsync(query.Id, cancellationToken);

            if (meetingItem == null)
            {
                return BusinessResult.NotFound<Response>(BusinessErrorMessage.NotFound);
            }

            var response = new Response(
                meetingItem.Id,
                meetingItem.Topic,
                meetingItem.Purpose,
                meetingItem.Outcome,
                meetingItem.DigitalProduct,
                meetingItem.Duration,
                meetingItem.Requestor,
                meetingItem.OwnerPresenter,
                meetingItem.Sponsor,
                meetingItem.SubmissionDate,
                meetingItem.Status,
                meetingItem.DecisionBoardId,
                meetingItem.TemplateId,
                meetingItem.Documents.Select(d => new DocumentDto(
                    d.Id,
                    d.FileName,
                    d.FileSize,
                    d.ContentType,
                    d.UploadDate,
                    d.UploadedBy)).ToList(),
                meetingItem.CreatedAt,
                meetingItem.CreatedBy);

            return BusinessResult.Success(response);
        }
    }
}
