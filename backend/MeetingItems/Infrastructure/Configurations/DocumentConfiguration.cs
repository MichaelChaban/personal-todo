#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MeetingItemsApp.MeetingItems.Models;

namespace MeetingItemsApp.MeetingItems.Infrastructure.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        // Primary Key
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasMaxLength(50)
            .IsRequired();

        // Foreign Key
        builder.Property(d => d.MeetingItemId)
            .HasMaxLength(50)
            .IsRequired();

        // Properties
        builder.Property(d => d.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(d => d.FilePath)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(d => d.FileSize)
            .IsRequired();

        builder.Property(d => d.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(d => d.UploadDate)
            .IsRequired();

        builder.Property(d => d.UploadedBy)
            .HasMaxLength(50)
            .IsRequired();

        // Indexes
        builder.HasIndex(d => d.MeetingItemId);
        builder.HasIndex(d => d.UploadDate);
    }
}
