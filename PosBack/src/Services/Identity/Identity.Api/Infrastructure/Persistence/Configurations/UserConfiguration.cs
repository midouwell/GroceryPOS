using Identity.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Api.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).HasMaxLength(36).IsRequired();
        builder.Property(user => user.UserName).HasMaxLength(100).IsRequired();
        builder.Property(user => user.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(user => user.PasswordHash).HasMaxLength(512).IsRequired();
        builder.Property(user => user.Role).HasMaxLength(50).IsRequired();
        builder.Property(user => user.CreatedAt).HasColumnType("datetimeoffset").IsRequired();
        builder.HasIndex(user => user.UserName).IsUnique();
        builder.HasIndex(user => user.Role);
    }
}
