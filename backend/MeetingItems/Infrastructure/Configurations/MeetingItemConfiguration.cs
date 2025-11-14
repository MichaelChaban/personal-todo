#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MeetingItemsApp.MeetingItems.Models;

namespace MeetingItemsApp.MeetingItems.Infrastructure.Configurations;

public class MeetingItemConfiguration : IEntityTypeConfiguration<MeetingItem>
{
    public void Configure(EntityTypeBuilder<MeetingItem> builder)
    {
        builder.ToTable("MeetingItems");

        // Primary Key
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasMaxLength(50)
            .IsRequired();

        // General Information
        builder.Property(m => m.Topic)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.Purpose)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(m => m.Outcome)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(m => m.Duration)
            .IsRequired();

        builder.Property(m => m.DigitalProduct)
            .HasMaxLength(200)
            .IsRequired();

        // Participants
        builder.Property(m => m.Requestor)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(m => m.OwnerPresenter)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(m => m.Sponsor)
            .HasMaxLength(50);

        // System Information
        builder.Property(m => m.SubmissionDate)
            .IsRequired();

        builder.Property(m => m.Status)
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue("Draft");

        // References
        builder.Property(m => m.DecisionBoardId)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(m => m.TemplateId)
            .HasMaxLength(50);

        // Audit
        builder.Property(m => m.CreatedAt)
            .IsRequired();

        builder.Property(m => m.CreatedBy)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(m => m.UpdatedAt);

        builder.Property(m => m.UpdatedBy)
            .HasMaxLength(50);

        // Relationships
        builder.HasMany(m => m.Documents)
            .WithOne()
            .HasForeignKey(d => d.MeetingItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(m => m.DecisionBoardId);
        builder.HasIndex(m => m.Status);
        builder.HasIndex(m => m.CreatedAt);
        builder.HasIndex(m => m.SubmissionDate);
    }
}
