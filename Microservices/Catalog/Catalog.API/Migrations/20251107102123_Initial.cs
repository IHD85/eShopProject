using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Catalog.API.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "brand",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    brand_name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("brand_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "type",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type_name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("type_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "item",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    description = table.Column<string>(type: "text", nullable: true),
                    price = table.Column<decimal>(type: "numeric", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    picture_uri = table.Column<string>(type: "text", nullable: true),
                    catalog_type_id = table.Column<int>(type: "integer", nullable: true),
                    catalog_brand_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("item_pkey", x => x.id);
                    table.ForeignKey(
                        name: "item_catalog_brand_id_fkey",
                        column: x => x.catalog_brand_id,
                        principalTable: "brand",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "item_catalog_type_id_fkey",
                        column: x => x.catalog_type_id,
                        principalTable: "type",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_item_catalog_brand_id",
                table: "item",
                column: "catalog_brand_id");

            migrationBuilder.CreateIndex(
                name: "IX_item_catalog_type_id",
                table: "item",
                column: "catalog_type_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "item");

            migrationBuilder.DropTable(
                name: "brand");

            migrationBuilder.DropTable(
                name: "type");
        }
    }
}
