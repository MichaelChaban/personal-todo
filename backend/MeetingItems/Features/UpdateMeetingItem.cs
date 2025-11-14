#nullable enable
using FluentValidation;
using MediatR;
using MeetingItemsApp.Common;
using MeetingItemsApp.Common.Abstractions;
using MeetingItemsApp.MeetingItems.DTOs;
using MeetingItemsApp.MeetingItems.Extensions;
using MeetingItemsApp.MeetingItems.Models;
using MeetingItemsApp.MeetingItems.Repositories;
using MeetingItemsApp.MeetingItems.Services;

namespace MeetingItemsApp.MeetingItems.Features;

public class UpdateMeetingItem
{
    public class Validator : AbstractValidator<Command>
    {
        private readonly IMeetingItemRepository _meetingItemRepository;

        public Validator(IMeetingItemRepository meetingItemRepository)
        {
            _meetingItemRepository = meetingItemRepository ?? throw new ArgumentNullException(nameof(meetingItemRepository));

            // Meeting Item ID validation
            RuleFor(c => c.Id)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required)
                .MustAsync(_meetingItemRepository.ExistsAsync)
                .WithMessage(BusinessErrorMessage.NotFound);

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

            RuleFor(c => c.OwnerPresenter)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required)
                .MaximumLength(50)
                .WithMessage("Owner/Presenter ID must not exceed 50 characters");

            RuleFor(c => c.Sponsor)
                .MaximumLength(50)
                .WithMessage("Sponsor ID must not exceed 50 characters")
                .When(c => !string.IsNullOrWhiteSpace(c.Sponsor));

            RuleFor(c => c.Status)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required)
                .Must(BeValidStatus)
                .WithMessage("Status must be 'Draft', 'Submitted', 'UnderReview', 'Approved', 'Rejected', or 'Deferred'");

            // Document validation - new documents to upload
            RuleForEach(c => c.NewDocuments)
                .SetValidator(new CreateMeetingItem.DocumentUploadValidator()!)
                .When(c => c.NewDocuments?.Any() == true);

            // Document version updates
            RuleForEach(c => c.DocumentVersions)
                .SetValidator(new DocumentVersionValidator()!)
                .When(c => c.DocumentVersions?.Any() == true);

            // Document deletion validation
            RuleForEach(c => c.DocumentsToDelete)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required)
                .When(c => c.DocumentsToDelete?.Any() == true);
        }

        private bool BeValidOutcome(string outcome)
        {
            var validOutcomes = new[] { "Decision", "Discussion", "Information" };
            return validOutcomes.Contains(outcome);
        }

        private bool BeValidStatus(string status)
        {
            var validStatuses = new[] { "Draft", "Submitted", "UnderReview", "Approved", "Rejected", "Deferred" };
            return validStatuses.Contains(status);
        }
    }

    public record DocumentVersionUpdate(
        string BaseDocumentId,
        DocumentUploadDto Document);

    public record Command(
        string Id,
        string Topic,
        string Purpose,
        string Outcome,
        string DigitalProduct,
        int Duration,
        string OwnerPresenter,
        string? Sponsor,
        string Status,
        List<DocumentUploadDto>? NewDocuments = null,
        List<DocumentVersionUpdate>? DocumentVersions = null,
        List<string>? DocumentsToDelete = null) : ICommand<Response>;

    public record Response(
        string MeetingItemId,
        List<DocumentUploadResponse> NewlyUploadedDocuments,
        List<DocumentUploadResponse> VersionedDocuments,
        List<string> DeletedDocumentIds);

    internal class Handler(
        IMeetingItemRepository meetingItemRepository,
        IDocumentService documentService,
        IUserSessionProvider userSessionProvider,
        IUnitOfWork unitOfWork) : ICommandHandler<Command, Response>
    {
        public async Task<BusinessResult<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var currentUserId = userSessionProvider.GetUserId() ?? throw new UnauthorizedAccessException();

            // Get existing meeting item
            var meetingItem = await meetingItemRepository.GetByIdWithDocumentsAsync(command.Id, cancellationToken);

            if (meetingItem == null)
            {
                return BusinessResult.NotFound<Response>(BusinessErrorMessage.NotFound);
            }

            // Update meeting item using domain method
            meetingItem.Update(
                command.Topic,
                command.Purpose,
                command.Outcome,
                command.DigitalProduct,
                command.Duration,
                command.OwnerPresenter,
                command.Sponsor,
                command.Status,
                currentUserId);

            // Handle document operations
            var deletedDocumentIds = await HandleDocumentDeletionsAsync(
                meetingItem,
                command.DocumentsToDelete,
                currentUserId,
                cancellationToken);

            var newUploadedDocuments = await HandleNewDocumentUploadsAsync(
                meetingItem,
                command.NewDocuments,
                currentUserId,
                cancellationToken);

            var versionedDocuments = await HandleDocumentVersionsAsync(
                meetingItem,
                command.DocumentVersions,
                currentUserId,
                cancellationToken);

            // Save changes
            await meetingItemRepository.UpdateAsync(meetingItem, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return BusinessResult.Success(new Response(
                meetingItem.Id,
                newUploadedDocuments,
                versionedDocuments,
                deletedDocumentIds));
        }

        /// <summary>
        /// Handles deletion of documents (soft delete with blob removal)
        /// </summary>
        private async Task<List<string>> HandleDocumentDeletionsAsync(
            MeetingItem meetingItem,
            List<string>? documentsToDelete,
            string currentUserId,
            CancellationToken cancellationToken)
        {
            var deletedDocumentIds = new List<string>();

            if (documentsToDelete?.Any() != true)
            {
                return deletedDocumentIds;
            }

            foreach (var documentId in documentsToDelete)
            {
                var document = meetingItem.Documents.FindActiveDocument(documentId);
                if (document != null)
                {
                    await documentService.DeleteDocumentAsync(document, currentUserId, cancellationToken);
                    deletedDocumentIds.Add(documentId);
                }
            }

            return deletedDocumentIds;
        }

        /// <summary>
        /// Handles upload of new documents (brand new documents, not versions)
        /// </summary>
        private async Task<List<DocumentUploadResponse>> HandleNewDocumentUploadsAsync(
            MeetingItem meetingItem,
            List<DocumentUploadDto>? newDocuments,
            string currentUserId,
            CancellationToken cancellationToken)
        {
            if (newDocuments?.Any() != true)
            {
                return new List<DocumentUploadResponse>();
            }

            var uploadedDocuments = await documentService.UploadDocumentsAsync(
                meetingItem.Id,
                newDocuments,
                currentUserId,
                cancellationToken);

            meetingItem.Documents.AddRange(uploadedDocuments);

            return uploadedDocuments.ToUploadResponses();
        }

        /// <summary>
        /// Handles upload of new versions of existing documents
        /// </summary>
        private async Task<List<DocumentUploadResponse>> HandleDocumentVersionsAsync(
            MeetingItem meetingItem,
            List<DocumentVersionUpdate>? documentVersions,
            string currentUserId,
            CancellationToken cancellationToken)
        {
            var versionedDocuments = new List<DocumentUploadResponse>();

            if (documentVersions?.Any() != true)
            {
                return versionedDocuments;
            }

            foreach (var versionUpdate in documentVersions)
            {
                var newVersion = await CreateDocumentVersionAsync(
                    meetingItem,
                    versionUpdate,
                    currentUserId,
                    cancellationToken);

                if (newVersion != null)
                {
                    versionedDocuments.Add(newVersion);
                }
            }

            return versionedDocuments;
        }

        /// <summary>
        /// Creates a new version of an existing document
        /// </summary>
        private async Task<DocumentUploadResponse?> CreateDocumentVersionAsync(
            MeetingItem meetingItem,
            DocumentVersionUpdate versionUpdate,
            string currentUserId,
            CancellationToken cancellationToken)
        {
            var baseDocument = meetingItem.Documents.FindActiveDocument(versionUpdate.BaseDocumentId);
            if (baseDocument == null)
            {
                return null;
            }

            // Mark old version as not latest using domain method
            baseDocument.MarkAsOldVersion();

            // Get next version number using extension method
            var nextVersion = meetingItem.Documents.GetNextVersionNumber(versionUpdate.BaseDocumentId);

            // Upload new version
            var newVersion = await documentService.UploadNewVersionAsync(
                meetingItem.Id,
                versionUpdate.BaseDocumentId,
                nextVersion,
                versionUpdate.Document,
                currentUserId,
                cancellationToken);

            meetingItem.Documents.Add(newVersion);

            return new DocumentUploadResponse(
                newVersion.Id,
                newVersion.FileName,
                newVersion.OriginalFileName,
                newVersion.FileSize,
                newVersion.ContentType,
                newVersion.Version);
        }
    }

    internal class DocumentVersionValidator : AbstractValidator<DocumentVersionUpdate>
    {
        public DocumentVersionValidator()
        {
            RuleFor(v => v.BaseDocumentId)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required);

            RuleFor(v => v.Document)
                .NotNull()
                .WithMessage(BusinessErrorMessage.Required)
                .SetValidator(new CreateMeetingItem.DocumentUploadValidator());
        }
    }
}
