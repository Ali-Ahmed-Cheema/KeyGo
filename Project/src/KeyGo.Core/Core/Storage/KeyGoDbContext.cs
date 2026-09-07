using KeyGo.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace KeyGo.Core.Storage;

public sealed class KeyGoDbContext : DbContext
{
    public KeyGoDbContext(DbContextOptions<KeyGoDbContext> options)
        : base(options)
    {
    }

    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<UsageRecord> UsageRecords => Set<UsageRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Title).HasMaxLength(200);
            entity.Property(c => c.Provider).HasMaxLength(200);
            entity.Property(c => c.Model).HasMaxLength(200);
            entity.HasMany(c => c.Messages)
                .WithOne(m => m.Conversation)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Role).HasMaxLength(50);
            entity.Property(m => m.MessageType).HasMaxLength(50);
            entity.HasIndex(m => m.ConversationId);
        });

        modelBuilder.Entity<UsageRecord>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Provider).HasMaxLength(200);
            entity.Property(u => u.Model).HasMaxLength(200);
            entity.Property(u => u.UsageStatus).HasMaxLength(50);
            entity.Property(u => u.RequestStatus).HasMaxLength(50);
            entity.HasIndex(u => u.ConversationId);
        });
    }
}
