#nullable enable
using FluentValidation;
using MediatR;
using MeetingItems.Core.AppServices.Abstractions;
using MeetingItems.Core.AppServices.Common;
using MeetingItems.Core.Domain.Repositories;
using MeetingItems.Infra.Common.Validations;

namespace MeetingItems.Core.AppServices.Features.MeetingItems;

public class GetTemplateByDecisionBoard
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

    public record Response(
        string TemplateId,
        string TemplateName,
        string? Description,
        List<FieldDefinitionDto> FieldDefinitions);

    public record FieldDefinitionDto(
        string Id,
        string FieldName,
        string Label,
        string FieldType,
        bool IsRequired,
        string Category,
        int DisplayOrder,
        string? HelpText,
        string? PlaceholderText,
        ValidationRulesDto? ValidationRules,
        List<FieldOptionDto>? Options);

    public record ValidationRulesDto(
        int? MinLength,
        int? MaxLength,
        decimal? Min,
        decimal? Max,
        string? Pattern,
        string? PatternMessage);

    public record FieldOptionDto(
        string Value,
        string Label,
        int DisplayOrder,
        bool IsDefault);

    internal class Handler(
        IDecisionBoardRepository decisionBoardRepository) : IQueryHandler<Query, Response>
    {
        public async Task<BusinessResult<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var decisionBoard = await decisionBoardRepository.GetByIdWithTemplateAsync(query.DecisionBoardId, cancellationToken);

            if (decisionBoard?.DefaultTemplate == null)
                return BusinessResult.Failure<Response>("Decision board does not have a default template");

            var template = decisionBoard.DefaultTemplate;

            // Only return active field definitions, ordered by category and display order
            var fieldDefinitions = template.FieldDefinitions
                .Where(f => f.IsActive)
                .OrderBy(f => f.Category)
                .ThenBy(f => f.DisplayOrder)
                .Select(f => new FieldDefinitionDto(
                    f.Id,
                    f.FieldName,
                    f.Label,
                    f.FieldType,
                    f.IsRequired,
                    f.Category,
                    f.DisplayOrder,
                    f.HelpText,
                    f.PlaceholderText,
                    ParseValidationRules(f.ValidationRulesJson),
                    f.Options?
                        .Where(o => o.IsActive)
                        .OrderBy(o => o.DisplayOrder)
                        .Select(o => new FieldOptionDto(
                            o.Value,
                            o.Label,
                            o.DisplayOrder,
                            o.IsDefault))
                        .ToList()))
                .ToList();

            var response = new Response(
                template.Id,
                template.Name,
                template.Description,
                fieldDefinitions);

            return BusinessResult.Success(response);
        }

        private static ValidationRulesDto? ParseValidationRules(string? rulesJson)
        {
            if (string.IsNullOrWhiteSpace(rulesJson))
                return null;

            try
            {
                var rules = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(rulesJson);

                if (rules == null)
                    return null;

                return new ValidationRulesDto(
                    rules.ContainsKey("minLength") ? Convert.ToInt32(rules["minLength"]) : null,
                    rules.ContainsKey("maxLength") ? Convert.ToInt32(rules["maxLength"]) : null,
                    rules.ContainsKey("min") ? Convert.ToDecimal(rules["min"]) : null,
                    rules.ContainsKey("max") ? Convert.ToDecimal(rules["max"]) : null,
                    rules.ContainsKey("pattern") ? rules["pattern"]?.ToString() : null,
                    rules.ContainsKey("patternMessage") ? rules["patternMessage"]?.ToString() : null);
            }
            catch
            {
                return null;
            }
        }
    }
}
