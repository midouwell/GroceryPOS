using Customers.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Customers.Api.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(customer => customer.Id);
        builder.Property(customer => customer.Id).HasMaxLength(36).IsRequired();
        builder.Property(customer => customer.Name).HasMaxLength(200).IsRequired();
        builder.Property(customer => customer.Email).HasMaxLength(320).IsRequired();
        builder.Property(customer => customer.Phone).HasMaxLength(30);
        builder.Property(customer => customer.CreatedAt).HasColumnType("datetimeoffset").IsRequired();
        builder.Property(customer => customer.UpdatedAt).HasColumnType("datetimeoffset");
        builder.HasIndex(customer => customer.Email);
    }
}
