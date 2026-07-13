using System;
using Abysalto.StefanParch.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Abysalto.StefanParch.Api.Migrations;

[DbContext(typeof(ApplicationDbContext))]
partial class ApplicationDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.28")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("Abysalto.StefanParch.Api.Models.Cart", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedNever()
                .HasColumnType("uuid")
                .HasColumnName("id");

            b.Property<DateTimeOffset>("CreatedAt")
                .ValueGeneratedOnAdd()
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");

            b.Property<Guid>("UserId")
                .HasColumnType("uuid")
                .HasColumnName("user_id");

            b.HasKey("Id")
                .HasName("pk_carts");

            b.HasIndex("UserId")
                .IsUnique()
                .HasDatabaseName("ux_carts_user_id");

            b.ToTable("carts", (string)null);
        });

        modelBuilder.Entity("Abysalto.StefanParch.Api.Models.CartItem", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedNever()
                .HasColumnType("uuid")
                .HasColumnName("id");

            b.Property<Guid>("CartId")
                .HasColumnType("uuid")
                .HasColumnName("cart_id");

            b.Property<Guid>("ProductId")
                .HasColumnType("uuid")
                .HasColumnName("product_id");

            b.Property<int>("Quantity")
                .HasColumnType("integer")
                .HasColumnName("quantity");

            b.Property<decimal>("UnitPrice")
                .HasPrecision(18, 2)
                .HasColumnType("numeric(18,2)")
                .HasColumnName("unit_price");

            b.HasKey("Id")
                .HasName("pk_cart_items");

            b.HasIndex("CartId")
                .HasDatabaseName("ix_cart_items_cart_id");

            b.HasIndex("CartId", "ProductId")
                .IsUnique()
                .HasDatabaseName("ux_cart_items_cart_id_product_id");

            b.HasIndex("ProductId")
                .HasDatabaseName("ix_cart_items_product_id");

            b.ToTable("cart_items", (string)null, t =>
            {
                t.HasCheckConstraint("ck_cart_items_quantity_positive", "quantity > 0");

                t.HasCheckConstraint("ck_cart_items_unit_price_positive", "unit_price > 0");
            });
        });

        modelBuilder.Entity("Abysalto.StefanParch.Api.Models.CartItem", b =>
        {
            b.HasOne("Abysalto.StefanParch.Api.Models.Cart", "Cart")
                .WithMany("Items")
                .HasForeignKey("CartId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired()
                .HasConstraintName("fk_cart_items_carts_cart_id");

            b.Navigation("Cart");
        });

        modelBuilder.Entity("Abysalto.StefanParch.Api.Models.Cart", b =>
        {
            b.Navigation("Items");
        });
#pragma warning restore 612, 618
    }
}
