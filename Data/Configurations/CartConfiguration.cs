using Abysalto.StefanParch.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Abysalto.StefanParch.Api.Data.Configurations;

public sealed class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("carts");

        builder.HasKey(cart => cart.Id);

        builder.Property(cart => cart.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(cart => cart.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(cart => cart.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.HasMany(cart => cart.Items)
            .WithOne(item => item.Cart)
            .HasForeignKey(item => item.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(cart => cart.UserId)
            .IsUnique()
            .HasDatabaseName("ux_carts_user_id");
    }
}
