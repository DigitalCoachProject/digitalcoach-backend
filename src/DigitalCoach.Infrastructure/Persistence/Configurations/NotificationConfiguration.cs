using DigitalCoach.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalCoach.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notification", table =>
        {
            table.HasCheckConstraint("CK_Notification_priority", "[priority] BETWEEN 1 AND 5");
            table.HasCheckConstraint("CK_Notification_type", "[type] IN ('habit', 'task', 'wellness', 'recommendation', 'burnout', 'reminder', 'system')");
        });

        builder.HasKey(x => x.Id).HasName("PK_Notification");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Message)
            .HasColumnName("message")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.Priority)
            .HasColumnName("priority")
            .IsRequired();

        builder.Property(x => x.IsRead)
            .HasColumnName("is_read")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(x => x.ScheduledFor)
            .HasColumnName("scheduled_for")
            .HasColumnType("datetime2");

        builder.Property(x => x.ReadAt)
            .HasColumnName("read_at")
            .HasColumnType("datetime2");

        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("datetime2");

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_Notification_user_id");

        builder.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAt })
            .HasDatabaseName("IX_Notification_user_is_read_created_at");

        builder.HasIndex(x => new { x.UserId, x.Type, x.CreatedAt })
            .HasDatabaseName("IX_Notification_user_type_created_at");

        builder.HasOne(x => x.UserProfile)
            .WithMany(x => x.Notifications)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Notification_UserProfile");
    }
}
