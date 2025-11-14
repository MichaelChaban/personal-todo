#nullable enable
using FluentValidation;
using MediatR;
using MeetingItemsApp.Common;
using MeetingItemsApp.Common.Abstractions;
using MeetingItemsApp.MeetingItems.Models;
using MeetingItemsApp.MeetingItems.Repositories;

namespace MeetingItemsApp.MeetingItems.Features;

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

            // Template validation (optional)
            RuleFor(c => c.TemplateId)
                .Cascade(CascadeMode.Stop)
                .MustAsync(BeActiveTemplateIfProvided!)
                .WithMessage("Template is not active")
                .When(c => !string.IsNullOrWhiteSpace(c.TemplateId));

            // Static field validation
            RuleFor(c => c.Topic)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required)
                .MaximumLength(200)
                .WithMessage("Topic must not exceed 200 characters");

            RuleFor(c => c.Purpose)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required)
                .MinimumLength(10)
                .WithMessage("Purpose must be at least 10 characters")
                .MaximumLength(2000)
                .WithMessage("Purpose must not exceed 2000 characters");

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

            RuleFor(c => c.Duration)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required)
                .GreaterThan(0)
                .WithMessage("Duration must be greater than 0")
                .LessThanOrEqualTo(480)
                .WithMessage("Duration must not exceed 480 minutes (8 hours)");

            RuleFor(c => c.Requestor)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required)
                .MaximumLength(50)
                .WithMessage("Requestor ID must not exceed 50 characters");

            RuleFor(c => c.OwnerPresenter)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required)
                .MaximumLength(50)
                .WithMessage("Owner/Presenter ID must not exceed 50 characters");

            RuleFor(c => c.Sponsor)
                .MaximumLength(50)
                .WithMessage("Sponsor ID must not exceed 50 characters")
                .When(c => !string.IsNullOrWhiteSpace(c.Sponsor));

            // Document validation
            RuleForEach(c => c.Documents)
                .SetValidator(new DocumentValidator()!)
                .When(c => c.Documents?.Any() == true);
        }

        private async Task<bool> BeActiveTemplateIfProvided(string? templateId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(templateId))
                return true;

            var template = await _templateRepository.GetByIdAsync(templateId, cancellationToken);
            return template?.IsActive == true;
        }

        private bool BeValidOutcome(string outcome)
        {
            var validOutcomes = new[] { "Decision", "Discussion", "Information" };
            return validOutcomes.Contains(outcome);
        }
    }

    public record DocumentDto(
        string FileName,
        string ContentType,
        long FileSize,
        string Base64Content);

    public record Command(
        string DecisionBoardId,
        string? TemplateId,
        string Topic,
        string Purpose,
        string Outcome,
        string DigitalProduct,
        int Duration,
        string Requestor,
        string OwnerPresenter,
        string? Sponsor,
        List<DocumentDto>? Documents = null) : ICommand<Response>;

    public record Response(string MeetingItemId, List<DocumentResponseDto> UploadedDocuments);

    public record DocumentResponseDto(
        string Id,
        string FileName,
        long FileSize,
        string ContentType);

    internal class Handler(
        IMeetingItemRepository meetingItemRepository,
        IDocumentStorageService documentStorage,
        IUserSessionProvider userSessionProvider,
        IUnitOfWork unitOfWork) : ICommandHandler<Command, Response>
    {
        public async Task<BusinessResult<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var currentUserId = userSessionProvider.GetUserId() ?? throw new UnauthorizedAccessException();

            // Create meeting item entity
            var meetingItem = MeetingItem.Create(
                command.DecisionBoardId,
                command.TemplateId,
                command.Topic,
                command.Purpose,
                command.Outcome,
                command.DigitalProduct,
                command.Duration,
                command.Requestor,
                command.OwnerPresenter,
                command.Sponsor,
                currentUserId);

            // Upload documents if provided
            var uploadedDocuments = new List<DocumentResponseDto>();
            if (command.Documents?.Any() == true)
            {
                foreach (var docDto in command.Documents)
                {
                    var document = await UploadDocumentAsync(
                        meetingItem.Id,
                        docDto,
                        currentUserId,
                        cancellationToken);

                    meetingItem.AddDocument(document);

                    uploadedDocuments.Add(new DocumentResponseDto(
                        document.Id,
                        document.FileName,
                        document.FileSize,
                        document.ContentType));
                }
            }

            // Save to repository
            await meetingItemRepository.AddAsync(meetingItem, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return BusinessResult.Success(new Response(meetingItem.Id, uploadedDocuments));
        }

        private async Task<Document> UploadDocumentAsync(
            string meetingItemId,
            DocumentDto docDto,
            string uploadedBy,
            CancellationToken cancellationToken)
        {
            // Extract base64 content (remove data URL prefix if present)
            var base64Content = docDto.Base64Content.Contains(',')
                ? docDto.Base64Content.Split(',')[1]
                : docDto.Base64Content;

            var fileContent = Convert.FromBase64String(base64Content);

            // Generate file path
            var fileExtension = Path.GetExtension(docDto.FileName);
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = $"meeting-items/{meetingItemId}/{fileName}";

            // Upload to storage
            await documentStorage.UploadAsync(filePath, fileContent, docDto.ContentType, cancellationToken);

            // Create document entity
            return Document.Create(
                meetingItemId,
                docDto.FileName,
                filePath,
                fileContent.Length,
                docDto.ContentType,
                uploadedBy);
        }
    }

    /// <summary>
    /// Validator for document uploads
    /// </summary>
    internal class DocumentValidator : AbstractValidator<DocumentDto>
    {
        public DocumentValidator()
        {
            RuleFor(d => d.FileName)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required)
                .MaximumLength(255)
                .WithMessage("File name must not exceed 255 characters");

            RuleFor(d => d.ContentType)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required);

            RuleFor(d => d.FileSize)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required)
                .GreaterThan(0)
                .WithMessage("File size must be greater than 0")
                .LessThanOrEqualTo(10 * 1024 * 1024) // 10 MB
                .WithMessage("File size must not exceed 10 MB");

            RuleFor(d => d.Base64Content)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required)
                .Must(BeValidBase64)
                .WithMessage("Invalid file content");
        }

        private bool BeValidBase64(string base64Content)
        {
            if (string.IsNullOrWhiteSpace(base64Content))
                return false;

            try
            {
                // Remove data URL prefix if present
                var content = base64Content.Contains(',')
                    ? base64Content.Split(',')[1]
                    : base64Content;

                Convert.FromBase64String(content);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
