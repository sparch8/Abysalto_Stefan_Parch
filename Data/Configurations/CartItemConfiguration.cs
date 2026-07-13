using Abysalto.StefanParch.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Abysalto.StefanParch.Api.Data.Configurations;

public sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("cart_items");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(item => item.CartId)
            .HasColumnName("cart_id")
            .IsRequired();

        builder.Property(item => item.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(item => item.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(item => item.UnitPrice)
            .HasColumnName("unit_price")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.ToTable(tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("ck_cart_items_quantity_positive", "quantity > 0");
            tableBuilder.HasCheckConstraint("ck_cart_items_unit_price_non_negative", "unit_price >= 0");
        });

        builder.HasIndex(item => item.CartId)
            .HasDatabaseName("ix_cart_items_cart_id");

        builder.HasIndex(item => new { item.CartId, item.ProductId })
            .IsUnique()
            .HasDatabaseName("ux_cart_items_cart_id_product_id");

        builder.HasIndex(item => item.ProductId)
            .HasDatabaseName("ix_cart_items_product_id");
    }
}
