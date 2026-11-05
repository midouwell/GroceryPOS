using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Api.Domain;

namespace Products.Api.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(product => product.Id);
        builder.Property(product => product.Id).HasMaxLength(36).IsRequired();
        builder.Property(product => product.Sku).HasMaxLength(100).IsRequired();
        builder.Property(product => product.Barcode).HasMaxLength(100).IsRequired();
        builder.Property(product => product.Name).HasMaxLength(200).IsRequired();
        builder.Property(product => product.Description).HasMaxLength(2000);
        builder.Property(product => product.Category).HasMaxLength(100).IsRequired();
        builder.Property(product => product.Unit).HasMaxLength(50).IsRequired();
        builder.Property(product => product.Price).HasPrecision(18, 2);
        builder.Property(product => product.TaxRate).HasPrecision(9, 6);
        builder.Property(product => product.CreatedAt).HasColumnType("datetimeoffset").IsRequired();
        builder.Property(product => product.UpdatedAt).HasColumnType("datetimeoffset");
        builder.HasIndex(product => product.Sku).IsUnique();
        builder.HasIndex(product => product.Barcode).IsUnique();
    }
}
