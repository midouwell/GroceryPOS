using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payments.Api.Domain;

namespace Payments.Api.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(payment => payment.Id);
        builder.Property(payment => payment.Id).HasMaxLength(36).IsRequired();
        builder.Property(payment => payment.IdempotencyKey).HasMaxLength(100).IsRequired();
        builder.Property(payment => payment.OperationReference).HasMaxLength(36).IsRequired();
        builder.Property(payment => payment.Method).HasMaxLength(30).IsRequired();
        builder.Property(payment => payment.Status).HasMaxLength(30).IsRequired();
        builder.Property(payment => payment.ProviderReference).HasMaxLength(200);
        builder.Property(payment => payment.CreatedAt).HasColumnType("datetimeoffset").IsRequired();
        builder.HasIndex(payment => payment.IdempotencyKey).IsUnique();
        builder.HasIndex(payment => payment.OperationReference);
    }
}
