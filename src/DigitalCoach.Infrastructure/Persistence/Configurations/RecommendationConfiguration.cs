using DigitalCoach.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalCoach.Infrastructure.Persistence.Configurations;

public sealed class RecommendationConfiguration : IEntityTypeConfiguration<Recommendation>
{
    public void Configure(EntityTypeBuilder<Recommendation> builder)
    {
        builder.ToTable("Recommendation", table =>
        {
            table.HasCheckConstraint("CK_Recommendation_priority", "[priority] BETWEEN 1 AND 5");
            table.HasCheckConstraint("CK_Recommendation_type", "[type] IN ('productivity', 'wellness', 'habit', 'task', 'burnout', 'sleep', 'motivation')");
        });

        builder.HasKey(x => x.Id).HasName("PK_Recommendation");

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

        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("datetime2");

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_Recommendation_user_id");

        builder.HasIndex(x => new { x.UserId, x.Type, x.CreatedAt })
            .HasDatabaseName("IX_Recommendation_user_type_created_at");

        builder.HasOne(x => x.UserProfile)
            .WithMany(x => x.Recommendations)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Recommendation_UserProfile");
    }
}
