using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Abysalto.StefanParch.Api.Migrations;

public partial class InitialCartSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "carts",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false,
                    defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_carts", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "cart_items",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                cart_id = table.Column<Guid>(type: "uuid", nullable: false),
                product_id = table.Column<Guid>(type: "uuid", nullable: false),
                quantity = table.Column<int>(type: "integer", nullable: false),
                unit_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_cart_items", x => x.id);
                table.CheckConstraint("ck_cart_items_quantity_positive", "quantity > 0");
                table.CheckConstraint("ck_cart_items_unit_price_positive", "unit_price > 0");
                table.ForeignKey(
                    name: "fk_cart_items_carts_cart_id",
                    column: x => x.cart_id,
                    principalTable: "carts",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_cart_items_cart_id",
            table: "cart_items",
            column: "cart_id");

        migrationBuilder.CreateIndex(
            name: "ix_cart_items_product_id",
            table: "cart_items",
            column: "product_id");

        migrationBuilder.CreateIndex(
            name: "ux_cart_items_cart_id_product_id",
            table: "cart_items",
            columns: new[] { "cart_id", "product_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ux_carts_user_id",
            table: "carts",
            column: "user_id",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "cart_items");

        migrationBuilder.DropTable(
            name: "carts");
    }
}
