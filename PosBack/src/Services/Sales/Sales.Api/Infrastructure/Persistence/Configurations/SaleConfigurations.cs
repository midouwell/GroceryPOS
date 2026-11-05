using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sales.Api.Domain;

namespace Sales.Api.Infrastructure.Persistence.Configurations;

public sealed class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");
        builder.HasKey(sale => sale.Id);
        builder.Property(sale => sale.Id).HasMaxLength(36).IsRequired();
        builder.Property(sale => sale.SaleNumber).HasMaxLength(100).IsRequired();
        builder.Property(sale => sale.IdempotencyKey).HasMaxLength(100).IsRequired();
        builder.Property(sale => sale.CashierId).HasMaxLength(36).IsRequired();
        builder.Property(sale => sale.PaymentStatus).HasMaxLength(30).IsRequired();
        builder.Property(sale => sale.Status).HasMaxLength(30).IsRequired();
        builder.Property(sale => sale.PaymentId).HasMaxLength(36);
        builder.Property(sale => sale.PaymentMethod).HasMaxLength(30);
        builder.Property(sale => sale.SaleDate).HasColumnType("datetimeoffset").IsRequired();
        builder.Property(sale => sale.CompletedAt).HasColumnType("datetimeoffset");
        builder.HasIndex(sale => sale.SaleNumber).IsUnique();
        builder.HasIndex(sale => sale.IdempotencyKey).IsUnique();
        builder.HasMany(sale => sale.Items).WithOne().HasForeignKey("SaleId").OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");
        builder.Property<string>("Id").HasMaxLength(36).ValueGeneratedOnAdd();
        builder.Property<string>("SaleId").HasMaxLength(36);
        builder.HasKey("Id");
        builder.Property(item => item.ProductId).HasMaxLength(36).IsRequired();
        builder.Property(item => item.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(item => item.Sku).HasMaxLength(100).IsRequired();
        builder.Property(item => item.TaxRate).HasPrecision(9, 6);
    }
}
