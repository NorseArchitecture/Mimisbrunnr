using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Norse.ReferenceData.Data.Migrations.Migrations;

/// <inheritdoc />
public partial class AddCountryOrAreaDossierView : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
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
	protected override void Down(MigrationBuilder migrationBuilder) =>
		migrationBuilder.Sql("DROP VIEW country_or_area_dossier;");
}
