using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MeetingItemsApp.MeetingItems.DTOs
{
    public class CreateMeetingItemDto
    {
        [Required]
        [StringLength(200)]
        public string Topic { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Purpose { get; set; } = string.Empty;

        [Required]
        public string Outcome { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int Duration { get; set; }

        [Required]
        [StringLength(200)]
        public string DigitalProduct { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Requestor { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string OwnerPresenter { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Sponsor { get; set; }

        [Required]
        public Guid DecisionBoardId { get; set; }

        public Guid? TemplateId { get; set; }
    }

    public class UpdateMeetingItemDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Topic { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Purpose { get; set; } = string.Empty;

        [Required]
        public string Outcome { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int Duration { get; set; }

        [Required]
        [StringLength(200)]
        public string DigitalProduct { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string OwnerPresenter { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Sponsor { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;
    }

    public class MeetingItemDto
    {
        public Guid Id { get; set; }
        public string Topic { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public string Outcome { get; set; } = string.Empty;
        public int Duration { get; set; }
        public string DigitalProduct { get; set; } = string.Empty;
        public string Requestor { get; set; } = string.Empty;
        public string OwnerPresenter { get; set; } = string.Empty;
        public string? Sponsor { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid DecisionBoardId { get; set; }
        public Guid? TemplateId { get; set; }
        public List<DocumentDto> Documents { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class DocumentDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public DateTime UploadDate { get; set; }
        public string UploadedBy { get; set; } = string.Empty;
    }

    public class UploadDocumentDto
    {
        [Required]
        public Guid MeetingItemId { get; set; }

        [Required]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string ContentType { get; set; } = string.Empty;

        [Required]
        public long FileSize { get; set; }

        [Required]
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
    }
}
