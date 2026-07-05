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
                    is_small_island_developing_state = table.Column<bool>(type: "boolean", nullable: false)
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

            migrationBuilder.Sql(
                """
                CREATE VIEW country_or_area_dossier AS
                WITH ancestry AS (
                    SELECT
                        coa.id AS country_id,
                        coa.m49code, coa.iso_alpha2code, coa.iso_alpha3code, coa.name,
                        coa.is_least_developed_country, coa.is_land_locked_developing_country, coa.is_small_island_developing_state,
                        CASE WHEN r1.level = 3 THEN r1.id END AS intermediate_region_id,
                        CASE WHEN r1.level = 2 THEN r1.id WHEN r2.level = 2 THEN r2.id END AS subregion_id,
                        CASE WHEN r2.level = 1 THEN r2.id WHEN r3.level = 1 THEN r3.id END AS region_id
                    FROM country_or_areas coa
                    LEFT JOIN regions r1 ON r1.id = coa.parent_region_id
                    LEFT JOIN regions r2 ON r2.id = r1.parent_region_id
                    LEFT JOIN regions r3 ON r3.id = r2.parent_region_id
                )
                SELECT
                    a.m49code AS code,
                    a.iso_alpha2code AS alpha2,
                    a.iso_alpha3code AS alpha3,
                    jsonb_build_object(
                        'code', a.m49code,
                        'alpha2', a.iso_alpha2code,
                        'alpha3', a.iso_alpha3code,
                        'name', a.name,
                        'isLeastDevelopedCountry', a.is_least_developed_country,
                        'isLandLockedDevelopingCountry', a.is_land_locked_developing_country,
                        'isSmallIslandDevelopingState', a.is_small_island_developing_state,
                        'region', CASE WHEN region.id IS NULL THEN NULL ELSE jsonb_build_object(
                            'code', region.m49code,
                            'name', region.name,
                            'subregion', CASE WHEN subregion.id IS NULL THEN NULL ELSE jsonb_build_object(
                                'code', subregion.m49code,
                                'name', subregion.name,
                                'intermediateRegion', CASE WHEN intermediate_region.id IS NULL THEN NULL ELSE jsonb_build_object(
                                    'code', intermediate_region.m49code,
                                    'name', intermediate_region.name
                                ) END
                            ) END
                        ) END
                    ) AS dossier
                FROM ancestry a
                LEFT JOIN regions region ON region.id = a.region_id
                LEFT JOIN regions subregion ON subregion.id = a.subregion_id
                LEFT JOIN regions intermediate_region ON intermediate_region.id = a.intermediate_region_id;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW country_or_area_dossier;");

            migrationBuilder.DropTable(
                name: "country_or_areas");

            migrationBuilder.DropTable(
                name: "regions");
        }
    }
}
