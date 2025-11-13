#nullable enable
using FluentValidation;
using MediatR;
using MeetingItems.Core.AppServices.Abstractions;
using MeetingItems.Core.AppServices.Common;
using MeetingItems.Core.AppServices.Common.Validators;
using MeetingItems.Core.AppServices.Extensions;
using MeetingItems.Core.AppServices.Shared.DTOs.Files;
using MeetingItems.Core.Blobs.Abstractions;
using MeetingItems.Core.Domain.MeetingItems;
using MeetingItems.Core.Domain.Repositories;
using MeetingItems.Infra.Common.Validations;

namespace MeetingItems.Core.AppServices.Features.MeetingItems;

public class UploadMeetingItemDocument
{
    public class Validator : AbstractValidator<Command>
    {
        private readonly IMeetingItemRepository _meetingItemRepository;

        public Validator(IMeetingItemRepository meetingItemRepository)
        {
            _meetingItemRepository = meetingItemRepository ?? throw new ArgumentNullException(nameof(meetingItemRepository));

            RuleFor(c => c.MeetingItemId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required)
                .MustAsync(_meetingItemRepository.ExistsAsync)
                .WithMessage(BusinessErrorMessage.NotFound);

            RuleFor(c => c.Document)
                .NotNull()
                .WithMessage(BusinessErrorMessage.Required)
                .SetValidator(new FileValidator());
        }
    }

    public record Command(
        string MeetingItemId,
        BlobFileDto Document) : ICommand<Response>;

    public record Response(
        string DocumentId,
        string StoredFileName,
        string VersionNumber);

    internal class Handler(
        IMeetingItemRepository meetingItemRepository,
        IDecisionBoardRepository decisionBoardRepository,
        IDocumentRepository documentRepository,
        IBlobContainerClientFactory blobClientFactory,
        IUserSessionProvider userSessionProvider,
        IUnitOfWork unitOfWork) : ICommandHandler<Command, Response>
    {
        public async Task<BusinessResult<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var meetingItem = await meetingItemRepository.GetByIdAsync(command.MeetingItemId, cancellationToken);

            if (meetingItem == null)
                return BusinessResult.Failure<Response>(BusinessErrorMessage.NotFound);

            var uploadedBy = userSessionProvider.GetUserId() ?? throw new UnauthorizedAccessException();

            // Get decision board for abbreviation
            var decisionBoard = await decisionBoardRepository.GetByIdAsync(meetingItem.DecisionBoardId, cancellationToken);
            var abbreviation = decisionBoard?.Abbreviation ?? "DB";

            // Get existing versions for this filename
            var existingVersions = await documentRepository.GetVersionCountAsync(
                command.MeetingItemId,
                command.Document.FileName ?? "document",
                cancellationToken);

            // Generate stored filename
            var (storedFileName, version) = GenerateStoredFileName(
                abbreviation,
                meetingItem.Topic,
                command.Document.FileName ?? "document",
                existingVersions);

            // Upload to blob storage
            var client = blobClientFactory.GetBlobContainerClient(Constants.BlobContainers.MeetingItemDocuments);
            var blobPath = $"meeting-items/{command.MeetingItemId}/{storedFileName}";

            await using var stream = new MemoryStream(Convert.FromBase64String(command.Document.Base64Content.ExtractBase64Data()));

            var metadata = MetadataExtensions.CreateDocumentMetadata(
                command.Document.FileName ?? "unknown",
                command.Document.ContentType ?? "application/octet-stream",
                uploadedBy);

            await client.UploadAsync(blobPath, stream, metadata, cancellationToken);

            // Create document entity
            var document = Document.Create(
                command.MeetingItemId,
                command.Document.FileName ?? "unknown",
                storedFileName,
                blobPath,
                version,
                stream.Length,
                command.Document.ContentType ?? "application/octet-stream",
                uploadedBy);

            meetingItem.AddDocument(document);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return BusinessResult.Success(new Response(
                document.Id,
                storedFileName,
                version));
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
    }
}
