#nullable enable
using FluentValidation;
using MediatR;
using MeetingItemsApp.Common;
using MeetingItemsApp.Common.Abstractions;
using MeetingItemsApp.MeetingItems.Models;
using MeetingItemsApp.MeetingItems.Repositories;

namespace MeetingItemsApp.MeetingItems.Features;

public class UploadDocument
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

            RuleFor(c => c.FileName)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required)
                .MaximumLength(255)
                .WithMessage("File name must not exceed 255 characters");

            RuleFor(c => c.ContentType)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required);

            RuleFor(c => c.FileSize)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(BusinessErrorMessage.Required)
                .GreaterThan(0)
                .WithMessage("File size must be greater than 0")
                .LessThanOrEqualTo(10 * 1024 * 1024) // 10 MB
                .WithMessage("File size must not exceed 10 MB");

            RuleFor(c => c.Base64Content)
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

    public record Command(
        string MeetingItemId,
        string FileName,
        string ContentType,
        long FileSize,
        string Base64Content) : ICommand<Response>;

    public record Response(
        string DocumentId,
        string FileName,
        long FileSize,
        string ContentType,
        DateTime UploadDate,
        string UploadedBy);

    internal class Handler(
        IMeetingItemRepository meetingItemRepository,
        IDocumentStorageService documentStorage,
        IUserSessionProvider userSessionProvider,
        IUnitOfWork unitOfWork) : ICommandHandler<Command, Response>
    {
        public async Task<BusinessResult<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var currentUserId = userSessionProvider.GetUserId() ?? throw new UnauthorizedAccessException();

            // Get meeting item
            var meetingItem = await meetingItemRepository.GetByIdAsync(command.MeetingItemId, cancellationToken);

            if (meetingItem == null)
            {
                return BusinessResult.NotFound<Response>(BusinessErrorMessage.NotFound);
            }

            // Extract base64 content (remove data URL prefix if present)
            var base64Content = command.Base64Content.Contains(',')
                ? command.Base64Content.Split(',')[1]
                : command.Base64Content;

            var fileContent = Convert.FromBase64String(base64Content);

            // Generate file path
            var filePath = $"meeting-items/{command.MeetingItemId}/{Guid.NewGuid()}{Path.GetExtension(command.FileName)}";

            // Upload to storage
            await documentStorage.UploadAsync(filePath, fileContent, command.ContentType, cancellationToken);

            // Create document entity
            var document = Document.Create(
                command.MeetingItemId,
                command.FileName,
                filePath,
                fileContent.Length,
                command.ContentType,
                currentUserId);

            // Add document to meeting item
            meetingItem.AddDocument(document);

            // Save changes
            await meetingItemRepository.UpdateAsync(meetingItem, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return BusinessResult.Success(new Response(
                document.Id,
                document.FileName,
                document.FileSize,
                document.ContentType,
                document.UploadDate,
                document.UploadedBy));
        }
    }
}
