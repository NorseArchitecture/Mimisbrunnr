using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Norse.ReferenceData.Data.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "regions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    m49code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    parent_region_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_regions", x => x.id);
                    table.ForeignKey(
                        name: "fk_regions_regions_parent_region_id",
                        column: x => x.parent_region_id,
                        principalTable: "regions",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "country_or_areas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    m49code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    iso_alpha2code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    iso_alpha3code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    parent_region_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_least_developed_country = table.Column<bool>(type: "boolean", nullable: false),
                    is_land_locked_developing_country = table.Column<bool>(type: "boolean", nullable: false),
                    is_small_island_developing_state = table.Column<bool>(type: "boolean", nullable: false),
                    view = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_country_or_areas", x => x.id);
                    table.ForeignKey(
                        name: "fk_country_or_areas_region_parent_region_id",
                        column: x => x.parent_region_id,
                        principalTable: "regions",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_country_or_areas_parent_region_id",
                table: "country_or_areas",
                column: "parent_region_id");

            migrationBuilder.CreateIndex(
                name: "uq_country_or_areas_iso_alpha2_code",
                table: "country_or_areas",
                column: "iso_alpha2code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_country_or_areas_iso_alpha3_code",
                table: "country_or_areas",
                column: "iso_alpha3code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_country_or_areas_m49_code",
                table: "country_or_areas",
                column: "m49code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_regions_parent_region_id",
                table: "regions",
                column: "parent_region_id");

            migrationBuilder.CreateIndex(
                name: "uq_regions_m49_code",
                table: "regions",
                column: "m49code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "country_or_areas");

            migrationBuilder.DropTable(
                name: "regions");
        }
    }
}
