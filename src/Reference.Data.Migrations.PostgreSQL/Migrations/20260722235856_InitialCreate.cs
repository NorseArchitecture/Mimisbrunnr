using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Norse.Reference.Data.Migrations.PostgreSQL.Migrations;

/// <inheritdoc />
public partial class _20260722235856_InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "region",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                level = table.Column<int>(type: "integer", nullable: false),
                parent_region_id = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_region", x => x.id);
                table.ForeignKey(
                    name: "fk_region_region_parent_region_id",
                    column: x => x.parent_region_id,
                    principalTable: "region",
                    principalColumn: "id");
            });

        migrationBuilder.CreateTable(
            name: "country_or_area",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                alpha2 = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                alpha3 = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                parent_region_id = table.Column<Guid>(type: "uuid", nullable: true),
                classification = table.Column<int>(type: "integer", nullable: false),
                view = table.Column<string>(type: "jsonb", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_country_or_area", x => x.id);
                table.ForeignKey(
                    name: "fk_country_or_area_region_parent_region_id",
                    column: x => x.parent_region_id,
                    principalTable: "region",
                    principalColumn: "id");
            });

        migrationBuilder.CreateIndex(
            name: "ix_country_or_area_alpha2",
            table: "country_or_area",
            column: "alpha2",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_country_or_area_alpha3",
            table: "country_or_area",
            column: "alpha3",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_country_or_area_code",
            table: "country_or_area",
            column: "code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_country_or_area_parent_region_id",
            table: "country_or_area",
            column: "parent_region_id");

        migrationBuilder.CreateIndex(
            name: "ix_region_code",
            table: "region",
            column: "code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_region_parent_region_id",
            table: "region",
            column: "parent_region_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "country_or_area");

        migrationBuilder.DropTable(
            name: "region");
    }
}
