#nullable enable
using FluentValidation;
using MediatR;
using MeetingItemsApp.Common;
using MeetingItemsApp.Common.Abstractions;
using MeetingItemsApp.MeetingItems.Models;
using MeetingItemsApp.MeetingItems.Repositories;

namespace MeetingItemsApp.MeetingItems.Features;

public class UpdateMeetingItem
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
                .SetValidator(new CreateMeetingItem.DocumentValidator()!)
                .When(c => c.NewDocuments?.Any() == true);

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
        List<CreateMeetingItem.DocumentDto>? NewDocuments = null,
        List<string>? DocumentsToDelete = null) : ICommand<Response>;

    public record Response(
        string MeetingItemId,
        List<CreateMeetingItem.DocumentResponseDto> NewlyUploadedDocuments,
        List<string> DeletedDocumentIds);

    internal class Handler(
        IMeetingItemRepository meetingItemRepository,
        IBlobContainerClientFactory containerClientFactory,
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

            // Update meeting item properties
            meetingItem.Topic = command.Topic;
            meetingItem.Purpose = command.Purpose;
            meetingItem.Outcome = command.Outcome;
            meetingItem.DigitalProduct = command.DigitalProduct;
            meetingItem.Duration = command.Duration;
            meetingItem.OwnerPresenter = command.OwnerPresenter;
            meetingItem.Sponsor = command.Sponsor;
            meetingItem.Status = command.Status;
            meetingItem.UpdatedAt = DateTime.UtcNow;
            meetingItem.UpdatedBy = currentUserId;

            var client = containerClientFactory.GetBlobContainerClient("meeting-item-documents");

            // Delete documents if specified
            var deletedDocumentIds = new List<string>();
            if (command.DocumentsToDelete?.Any() == true)
            {
                foreach (var documentId in command.DocumentsToDelete)
                {
                    var document = meetingItem.Documents.FirstOrDefault(d => d.Id == documentId);
                    if (document != null)
                    {
                        // Delete from blob storage
                        await client.DeleteAsync(document.FilePath, cancellationToken);

                        // Remove from collection
                        meetingItem.Documents.Remove(document);
                        deletedDocumentIds.Add(documentId);
                    }
                }
            }

            // Upload new documents if provided
            var uploadedDocuments = new List<CreateMeetingItem.DocumentResponseDto>();
            if (command.NewDocuments?.Any() == true)
            {
                var newDocuments = await UploadDocumentsAsync(client, meetingItem.Id, command.NewDocuments, currentUserId, cancellationToken);

                meetingItem.Documents.AddRange(newDocuments);

                uploadedDocuments.AddRange(newDocuments.Select(d => new CreateMeetingItem.DocumentResponseDto(
                    d.Id,
                    d.FileName,
                    d.FileSize,
                    d.ContentType)));
            }

            // Save changes
            await meetingItemRepository.UpdateAsync(meetingItem, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return BusinessResult.Success(new Response(
                meetingItem.Id,
                uploadedDocuments,
                deletedDocumentIds));
        }

        private static async Task<List<Document>> UploadDocumentsAsync(
            IBlobContainerClient client,
            string meetingItemId,
            IEnumerable<CreateMeetingItem.DocumentDto> documentDtos,
            string uploadedBy,
            CancellationToken cancellationToken)
        {
            var uploadTasks = documentDtos.Select(async docDto =>
            {
                // Extract base64 content (remove data URL prefix if present)
                var base64Content = docDto.Base64Content.Contains(',')
                    ? docDto.Base64Content.Split(',')[1]
                    : docDto.Base64Content;

                var fileContent = Convert.FromBase64String(base64Content);

                // Generate blob name
                var blobName = Guid.NewGuid().ToString();

                // Upload to blob storage
                using var stream = new MemoryStream(fileContent);
                await client.UploadAsync(blobName, stream, cancellationToken);

                // Create document entity
                return new Document
                {
                    Id = Guid.NewGuid().ToString(),
                    MeetingItemId = meetingItemId,
                    FileName = docDto.FileName,
                    FilePath = blobName,
                    FileSize = fileContent.Length,
                    ContentType = docDto.ContentType,
                    UploadDate = DateTime.UtcNow,
                    UploadedBy = uploadedBy
                };
            });

            return (await Task.WhenAll(uploadTasks)).ToList();
        }
    }
}
