#nullable enable
using Microsoft.EntityFrameworkCore;
using MeetingItemsApp.MeetingItems.Infrastructure.Configurations;
using MeetingItemsApp.MeetingItems.Models;

namespace MeetingItemsApp.MeetingItems.Infrastructure;

public class MeetingItemsDbContext : DbContext
{
    public MeetingItemsDbContext(DbContextOptions<MeetingItemsDbContext> options)
        : base(options)
    {
    }

    public DbSet<MeetingItem> MeetingItems => Set<MeetingItem>();
    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfiguration(new MeetingItemConfiguration());
        modelBuilder.ApplyConfiguration(new DocumentConfiguration());
    }
}
