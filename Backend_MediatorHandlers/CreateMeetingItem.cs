#nullable enable
using FluentValidation;
using MediatR;
using MeetingItems.Core.AppServices.Abstractions;
using MeetingItems.Core.AppServices.Common;
using MeetingItems.Core.AppServices.Shared.DTOs.Files;
using MeetingItems.Core.Blobs.Abstractions;
using MeetingItems.Core.Domain.MeetingItems;
using MeetingItems.Core.Domain.Repositories;
using MeetingItems.Infra.Common.Validations;

namespace MeetingItems.Core.AppServices.Features.MeetingItems;

public class CreateMeetingItem
{
    public class Validator : AbstractValidator<Command>
    {
        private readonly IDecisionBoardRepository _decisionBoardRepository;
        private readonly ITemplateRepository _templateRepository;
        private readonly IUserSessionProvider _userSessionProvider;

        public Validator(
            IDecisionBoardRepository decisionBoardRepository,
            ITemplateRepository templateRepository,
            IUserSessionProvider userSessionProvider)
        {
            _decisionBoardRepository = decisionBoardRepository ?? throw new ArgumentNullException(nameof(decisionBoardRepository));
            _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
            _userSessionProvider = userSessionProvider ?? throw new ArgumentNullException(nameof(userSessionProvider));

            // Decision Board validation
            RuleFor(c => c.DecisionBoardId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required)
                .MustAsync(_decisionBoardRepository.ExistsAsync)
                .WithMessage(BusinessErrorMessage.NotFound);

            // Template validation
            RuleFor(c => c.TemplateId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required)
                .MustAsync(_templateRepository.ExistsAsync)
                .WithMessage(BusinessErrorMessage.NotFound)
                .MustAsync(BeActiveTemplate)
                .WithMessage("Template is not active");

            // Static field validation
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
                .WithMessage("Duration must not exceed 480 minutes (8 hours)");

            RuleFor(c => c.OwnerPresenterUserId)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required);

            RuleFor(c => c.SponsorUserId)
                .MaximumLength(50)
                .WithMessage("Sponsor ID must not exceed 50 characters")
                .When(c => !string.IsNullOrWhiteSpace(c.SponsorUserId));

            // Dynamic field validation
            RuleFor(c => c)
                .MustAsync(ValidateDynamicFields)
                .WithMessage("Dynamic field validation failed");

            // Document validation
            RuleForEach(c => c.Documents)
                .SetValidator(new FileValidator()!)
                .When(command => command.Documents?.Any() == true);
        }

        private async Task<bool> BeActiveTemplate(string templateId, CancellationToken cancellationToken)
        {
            var template = await _templateRepository.GetByIdAsync(templateId, cancellationToken);
            return template?.IsActive == true;
        }

        private bool BeValidOutcome(string outcome)
        {
            var validOutcomes = new[] { "Decision", "Discussion", "Information" };
            return validOutcomes.Contains(outcome);
        }

        private async Task<bool> ValidateDynamicFields(Command command, CancellationToken cancellationToken)
        {
            var template = await _templateRepository.GetByIdWithFieldsAsync(command.TemplateId, cancellationToken);

            if (template == null)
                return false;

            var validator = new DynamicFieldValidator(template);
            var result = await validator.ValidateAsync(command.FieldValues ?? new List<FieldValueDto>(), cancellationToken);

            return result.IsValid;
        }
    }

    public record Command(
        string DecisionBoardId,
        string TemplateId,
        string Topic,
        string Purpose,
        string Outcome,
        string DigitalProduct,
        int DurationMinutes,
        string OwnerPresenterUserId,
        string? SponsorUserId,
        List<FieldValueDto>? FieldValues = null,
        List<BlobFileDto>? Documents = null) : ICommand<Response>;

    public record FieldValueDto(
        string FieldName,
        string? TextValue = null,
        decimal? NumberValue = null,
        DateTime? DateValue = null,
        bool? BooleanValue = null,
        string? JsonValue = null);

    public record Response(string MeetingItemId);

    internal class Handler(
        IMeetingItemRepository meetingItemRepository,
        ITemplateRepository templateRepository,
        IUserSessionProvider userSessionProvider,
        IBlobContainerClientFactory blobClientFactory,
        IUnitOfWork unitOfWork) : ICommandHandler<Command, Response>
    {
        public async Task<BusinessResult<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var template = await templateRepository.GetByIdWithFieldsAsync(command.TemplateId, cancellationToken);
            var requestorUserId = userSessionProvider.GetUserId() ?? throw new UnauthorizedAccessException();

            // Create meeting item entity
            var meetingItem = MeetingItem.Create(
                command.DecisionBoardId,
                command.TemplateId,
                command.Topic,
                command.Purpose,
                command.Outcome,
                command.DigitalProduct,
                command.DurationMinutes,
                requestorUserId,
                command.OwnerPresenterUserId,
                command.SponsorUserId);

            // Add dynamic field values
            if (command.FieldValues?.Any() == true)
            {
                var fieldValues = CreateFieldValues(meetingItem.Id, command.FieldValues, template!);
                foreach (var fieldValue in fieldValues)
                {
                    meetingItem.AddFieldValue(fieldValue);
                }
            }

            // Upload documents
            if (command.Documents?.Any() == true)
            {
                var uploadedDocuments = await UploadDocuments(
                    meetingItem.Id,
                    command.Topic,
                    command.DecisionBoardId,
                    command.Documents,
                    requestorUserId,
                    cancellationToken);

                foreach (var document in uploadedDocuments)
                {
                    meetingItem.AddDocument(document);
                }
            }

            // Save to repository
            await meetingItemRepository.AddAsync(meetingItem, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return BusinessResult.Success(new Response(meetingItem.Id));
        }

        private static List<MeetingItemFieldValue> CreateFieldValues(
            string meetingItemId,
            List<FieldValueDto> fieldValuesDto,
            Template template)
        {
            var fieldValues = new List<MeetingItemFieldValue>();

            foreach (var dto in fieldValuesDto)
            {
                var fieldDefinition = template.FieldDefinitions
                    .FirstOrDefault(f => f.FieldName == dto.FieldName);

                if (fieldDefinition == null)
                    continue;

                var fieldValue = MeetingItemFieldValue.Create(
                    meetingItemId,
                    fieldDefinition.Id,
                    dto.TextValue,
                    dto.NumberValue,
                    dto.DateValue,
                    dto.BooleanValue,
                    dto.JsonValue);

                fieldValues.Add(fieldValue);
            }

            return fieldValues;
        }

        private async Task<List<Document>> UploadDocuments(
            string meetingItemId,
            string topic,
            string decisionBoardId,
            List<BlobFileDto> documents,
            string uploadedBy,
            CancellationToken cancellationToken)
        {
            var uploadedDocuments = new List<Document>();
            var client = blobClientFactory.GetBlobContainerClient(Constants.BlobContainers.MeetingItemDocuments);

            // Get decision board abbreviation for file naming
            var decisionBoard = await GetDecisionBoardAsync(decisionBoardId, cancellationToken);
            var abbreviation = decisionBoard?.Abbreviation ?? "DB";

            foreach (var documentDto in documents)
            {
                if (string.IsNullOrWhiteSpace(documentDto.Base64Content))
                    continue;

                var (storedFileName, version) = GenerateStoredFileName(
                    abbreviation,
                    topic,
                    documentDto.FileName ?? "document",
                    await GetExistingVersionsAsync(meetingItemId, documentDto.FileName ?? "", cancellationToken));

                var blobPath = $"meeting-items/{meetingItemId}/{storedFileName}";

                await using var stream = new MemoryStream(Convert.FromBase64String(documentDto.Base64Content.ExtractBase64Data()));

                var metadata = MetadataExtensions.CreateDocumentMetadata(
                    documentDto.FileName ?? "unknown",
                    documentDto.ContentType ?? "application/octet-stream",
                    uploadedBy);

                await client.UploadAsync(blobPath, stream, metadata, cancellationToken);

                var document = Document.Create(
                    meetingItemId,
                    documentDto.FileName ?? "unknown",
                    storedFileName,
                    blobPath,
                    version,
                    stream.Length,
                    documentDto.ContentType ?? "application/octet-stream",
                    uploadedBy);

                uploadedDocuments.Add(document);
            }

            return uploadedDocuments;
        }

        private static (string storedFileName, string version) GenerateStoredFileName(
            string abbreviation,
            string topic,
            string originalFileName,
            int existingVersionCount)
        {
            var dateStr = DateTime.UtcNow.ToString("yyyyMMdd");
            var topicSlug = SanitizeFileName(topic, maxLength: 20);
            var extension = Path.GetExtension(originalFileName);
            var versionNumber = existingVersionCount + 1;
            var version = versionNumber.ToString("D2");

            var storedFileName = $"{abbreviation}{dateStr}{topicSlug}.v{version}{extension}";

            return (storedFileName, version);
        }

        private static string SanitizeFileName(string fileName, int maxLength = 50)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalid));
            return sanitized.Length > maxLength
                ? sanitized.Substring(0, maxLength)
                : sanitized;
        }

        private async Task<DecisionBoard?> GetDecisionBoardAsync(string decisionBoardId, CancellationToken cancellationToken)
        {
            // This would be injected via repository
            // Simplified for example
            return await Task.FromResult<DecisionBoard?>(null);
        }

        private async Task<int> GetExistingVersionsAsync(string meetingItemId, string originalFileName, CancellationToken cancellationToken)
        {
            // Query existing documents with same original filename
            // Return count for version numbering
            return await Task.FromResult(0);
        }
    }

    /// <summary>
    /// Dynamic field validator that validates field values against template field definitions
    /// </summary>
    internal class DynamicFieldValidator : AbstractValidator<List<FieldValueDto>>
    {
        private readonly Template _template;

        public DynamicFieldValidator(Template template)
        {
            _template = template;

            RuleFor(fieldValues => fieldValues)
                .Must(HaveAllRequiredFields)
                .WithMessage("Missing required fields");

            RuleForEach(fieldValue => fieldValue)
                .Must(BeValidField)
                .WithMessage(fv => $"Invalid field: {fv.FieldName}");
        }

        private bool HaveAllRequiredFields(List<FieldValueDto> fieldValues)
        {
            var requiredFields = _template.FieldDefinitions
                .Where(f => f.IsRequired && f.IsActive)
                .Select(f => f.FieldName)
                .ToList();

            var providedFields = fieldValues.Select(fv => fv.FieldName).ToList();

            return requiredFields.All(rf => providedFields.Contains(rf));
        }

        private bool BeValidField(FieldValueDto fieldValue)
        {
            var fieldDefinition = _template.FieldDefinitions
                .FirstOrDefault(f => f.FieldName == fieldValue.FieldName);

            if (fieldDefinition == null)
                return false;

            // Type validation
            return fieldDefinition.FieldType switch
            {
                "Text" or "TextArea" or "Email" => ValidateTextField(fieldValue, fieldDefinition),
                "Number" => ValidateNumberField(fieldValue, fieldDefinition),
                "Date" => ValidateDateField(fieldValue, fieldDefinition),
                "YesNo" => ValidateBooleanField(fieldValue, fieldDefinition),
                "Dropdown" or "Radio" => ValidateDropdownField(fieldValue, fieldDefinition),
                "MultiSelect" => ValidateMultiSelectField(fieldValue, fieldDefinition),
                _ => true
            };
        }

        private bool ValidateTextField(FieldValueDto fieldValue, FieldDefinition fieldDef)
        {
            if (fieldDef.IsRequired && string.IsNullOrWhiteSpace(fieldValue.TextValue))
                return false;

            if (fieldValue.TextValue == null)
                return true;

            var rules = ParseValidationRules(fieldDef.ValidationRulesJson);

            if (rules.ContainsKey("minLength") && fieldValue.TextValue.Length < (int)rules["minLength"])
                return false;

            if (rules.ContainsKey("maxLength") && fieldValue.TextValue.Length > (int)rules["maxLength"])
                return false;

            if (rules.ContainsKey("pattern") && !System.Text.RegularExpressions.Regex.IsMatch(fieldValue.TextValue, rules["pattern"].ToString()!))
                return false;

            if (fieldDef.FieldType == "Email" && !IsValidEmail(fieldValue.TextValue))
                return false;

            return true;
        }

        private bool ValidateNumberField(FieldValueDto fieldValue, FieldDefinition fieldDef)
        {
            if (fieldDef.IsRequired && fieldValue.NumberValue == null)
                return false;

            if (fieldValue.NumberValue == null)
                return true;

            var rules = ParseValidationRules(fieldDef.ValidationRulesJson);

            if (rules.ContainsKey("min") && fieldValue.NumberValue < (decimal)rules["min"])
                return false;

            if (rules.ContainsKey("max") && fieldValue.NumberValue > (decimal)rules["max"])
                return false;

            return true;
        }

        private bool ValidateDateField(FieldValueDto fieldValue, FieldDefinition fieldDef)
        {
            if (fieldDef.IsRequired && fieldValue.DateValue == null)
                return false;

            return true;
        }

        private bool ValidateBooleanField(FieldValueDto fieldValue, FieldDefinition fieldDef)
        {
            if (fieldDef.IsRequired && fieldValue.BooleanValue == null)
                return false;

            return true;
        }

        private bool ValidateDropdownField(FieldValueDto fieldValue, FieldDefinition fieldDef)
        {
            if (fieldDef.IsRequired && string.IsNullOrWhiteSpace(fieldValue.TextValue))
                return false;

            if (fieldValue.TextValue == null)
                return true;

            var validOptions = fieldDef.Options
                .Where(o => o.IsActive)
                .Select(o => o.Value)
                .ToList();

            return validOptions.Contains(fieldValue.TextValue);
        }

        private bool ValidateMultiSelectField(FieldValueDto fieldValue, FieldDefinition fieldDef)
        {
            if (fieldDef.IsRequired && string.IsNullOrWhiteSpace(fieldValue.JsonValue))
                return false;

            if (fieldValue.JsonValue == null)
                return true;

            try
            {
                var selectedValues = System.Text.Json.JsonSerializer.Deserialize<List<string>>(fieldValue.JsonValue);
                if (selectedValues == null)
                    return false;

                var validOptions = fieldDef.Options
                    .Where(o => o.IsActive)
                    .Select(o => o.Value)
                    .ToList();

                return selectedValues.All(sv => validOptions.Contains(sv));
            }
            catch
            {
                return false;
            }
        }

        private Dictionary<string, object> ParseValidationRules(string? rulesJson)
        {
            if (string.IsNullOrWhiteSpace(rulesJson))
                return new Dictionary<string, object>();

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(rulesJson)
                    ?? new Dictionary<string, object>();
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
