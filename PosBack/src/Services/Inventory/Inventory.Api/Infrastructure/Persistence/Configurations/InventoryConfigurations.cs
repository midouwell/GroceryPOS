using Inventory.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Api.Infrastructure.Persistence.Configurations;

public sealed class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable("StockItems");
        builder.HasKey(item => item.ProductId);
        builder.Property(item => item.ProductId).HasMaxLength(36).IsRequired();
        builder.Property(item => item.UpdatedAt).HasColumnType("datetimeoffset").IsRequired();
        builder.HasIndex(item => item.QuantityOnHand);
    }
}

public sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");
        builder.HasKey(movement => movement.Id);
        builder.Property(movement => movement.Id).HasMaxLength(36).IsRequired();
        builder.Property(movement => movement.ProductId).HasMaxLength(36).IsRequired();
        builder.Property(movement => movement.Type).HasMaxLength(30).IsRequired();
        builder.Property(movement => movement.Reason).HasMaxLength(500);
        builder.Property(movement => movement.CreatedAt).HasColumnType("datetimeoffset").IsRequired();
        builder.HasIndex(movement => movement.ProductId);
        builder.HasIndex(movement => new { movement.ProductId, movement.Reason });
    }
}
