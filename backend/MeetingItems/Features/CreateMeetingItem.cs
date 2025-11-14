#nullable enable
using FluentValidation;
using MediatR;
using MeetingItemsApp.Common;
using MeetingItemsApp.Common.Abstractions;
using MeetingItemsApp.MeetingItems.DTOs;
using MeetingItemsApp.MeetingItems.Models;
using MeetingItemsApp.MeetingItems.Repositories;
using MeetingItemsApp.MeetingItems.Services;

namespace MeetingItemsApp.MeetingItems.Features;

public class CreateMeetingItem
{
    public class Validator : AbstractValidator<Command>
    {
        private readonly IDecisionBoardRepository _decisionBoardRepository;
        private readonly ITemplateRepository _templateRepository;

        public Validator(
            IDecisionBoardRepository decisionBoardRepository,
            ITemplateRepository templateRepository)
        {
            _decisionBoardRepository = decisionBoardRepository ?? throw new ArgumentNullException(nameof(decisionBoardRepository));
            _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));

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
                .SetValidator(new DocumentUploadValidator()!)
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
        List<DocumentUploadDto>? Documents = null) : ICommand<Response>;

    public record Response(
        string MeetingItemId,
        List<DocumentUploadResponse> UploadedDocuments);

    internal class Handler(
        IMeetingItemRepository meetingItemRepository,
        IDocumentService documentService,
        IUserSessionProvider userSessionProvider,
        IUnitOfWork unitOfWork) : ICommandHandler<Command, Response>
    {
        public async Task<BusinessResult<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var currentUserId = userSessionProvider.GetUserId() ?? throw new UnauthorizedAccessException();

            // Create meeting item entity
            var meetingItem = new MeetingItem
            {
                Id = Guid.NewGuid().ToString(),
                DecisionBoardId = command.DecisionBoardId,
                TemplateId = command.TemplateId,
                Topic = command.Topic,
                Purpose = command.Purpose,
                Outcome = command.Outcome,
                DigitalProduct = command.DigitalProduct,
                Duration = command.Duration,
                Requestor = command.Requestor,
                OwnerPresenter = command.OwnerPresenter,
                Sponsor = command.Sponsor,
                SubmissionDate = DateTime.UtcNow,
                Status = "Draft",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId
            };

            // Upload documents if provided (new meeting item - no versioning)
            var uploadedDocuments = new List<DocumentUploadResponse>();
            if (command.Documents?.Any() == true)
            {
                var documents = await documentService.UploadDocumentsAsync(
                    meetingItem.Id,
                    command.Documents,
                    currentUserId,
                    cancellationToken);

                meetingItem.Documents.AddRange(documents);

                uploadedDocuments.AddRange(documents.Select(d => new DocumentUploadResponse(
                    d.Id,
                    d.FileName,
                    d.OriginalFileName,
                    d.FileSize,
                    d.ContentType,
                    d.Version)));
            }

            // Save to repository
            await meetingItemRepository.AddAsync(meetingItem, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return BusinessResult.Success(new Response(meetingItem.Id, uploadedDocuments));
        }
    }

    /// <summary>
    /// Validator for document uploads
    /// </summary>
    internal class DocumentUploadValidator : AbstractValidator<DocumentUploadDto>
    {
        public DocumentUploadValidator()
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
