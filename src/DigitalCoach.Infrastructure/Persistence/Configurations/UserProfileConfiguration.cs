using DigitalCoach.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalCoach.Infrastructure.Persistence.Configurations;

public sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfile", table =>
        {
            table.HasCheckConstraint("CK_UserProfile_height", "[height] > 0");
            table.HasCheckConstraint("CK_UserProfile_weight", "[weight] > 0");
        });

        builder.HasKey(x => x.Id).HasName("PK_UserProfile");
        builder.HasAlternateKey(x => x.Email).HasName("UQ_UserProfile_email");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Gender)
            .HasColumnName("gender")
            .HasMaxLength(20);

        builder.Property(x => x.Height)
            .HasColumnName("height")
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.Property(x => x.Weight)
            .HasColumnName("weight")
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.Property(x => x.BirthDate)
            .HasColumnName("birth_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSDATETIME()")
            .IsRequired();
    }
}
