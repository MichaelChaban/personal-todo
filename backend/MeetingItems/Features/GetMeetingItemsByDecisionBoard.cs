#nullable enable
using FluentValidation;
using MediatR;
using MeetingItemsApp.Common;
using MeetingItemsApp.Common.Abstractions;
using MeetingItemsApp.MeetingItems.Repositories;

namespace MeetingItemsApp.MeetingItems.Features;

public class GetMeetingItemsByDecisionBoard
{
    public class Validator : AbstractValidator<Query>
    {
        private readonly IDecisionBoardRepository _decisionBoardRepository;

        public Validator(IDecisionBoardRepository decisionBoardRepository)
        {
            _decisionBoardRepository = decisionBoardRepository ?? throw new ArgumentNullException(nameof(decisionBoardRepository));

            RuleFor(q => q.DecisionBoardId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required)
                .MustAsync(_decisionBoardRepository.ExistsAsync)
                .WithMessage(BusinessErrorMessage.NotFound);
        }
    }

    public record Query(string DecisionBoardId) : IQuery<Response>;

    public record Response(List<MeetingItemDto> MeetingItems);

    public record MeetingItemDto(
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
        string? TemplateId,
        int DocumentCount,
        DateTime CreatedAt,
        string CreatedBy);

    internal class Handler(
        IMeetingItemRepository meetingItemRepository) : IQueryHandler<Query, Response>
    {
        public async Task<BusinessResult<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var meetingItems = await meetingItemRepository.GetByDecisionBoardIdAsync(query.DecisionBoardId, cancellationToken);

            var dtos = meetingItems.Select(m => new MeetingItemDto(
                m.Id,
                m.Topic,
                m.Purpose,
                m.Outcome,
                m.DigitalProduct,
                m.Duration,
                m.Requestor,
                m.OwnerPresenter,
                m.Sponsor,
                m.SubmissionDate,
                m.Status,
                m.TemplateId,
                m.Documents.Count,
                m.CreatedAt,
                m.CreatedBy)).ToList();

            return BusinessResult.Success(new Response(dtos));
        }
    }
}
