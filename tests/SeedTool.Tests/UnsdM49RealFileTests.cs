using Norse.Primitives.Ingestion;
using Norse.Reference.Seeds;
using Norse.SeedTool.Mappers;

namespace Norse.SeedTool.Tests;

public sealed class UnsdM49RealFileTests
{
	const string RegionTsvPath = "../../../../../seeds/region.tsv";
	const string CountryOrAreaTsvPath = "../../../../../seeds/country-or-area.tsv";

	[Fact]
	void Map_produces_the_expected_counts_and_known_rows_from_the_real_source()
	{
		using var reader = TabularReader.OpenDelimited(RawDatasets.UnsdM49(), ';');
		var (regions, countries) = UnsdM49Mapper.Map(reader);

		// 5 Regions + 17 Sub-regions + 7 Intermediate Regions, per the approved M49 spec's
		// verified data facts (Glitnir/docs/Mimisbrunnr/specs/2026-07-04-unsd-m49-reference-data-design.md §1).
		regions.Count.ShouldBe(29);
		countries.Count.ShouldBe(248);

		countries.Any(c => c is { M49Code: "566", Name: "Nigeria", IsoAlpha2Code: "NG", IsoAlpha3Code: "NGA" }).ShouldBeTrue();
		countries.Any(c => c is { M49Code: "010", Name: "Antarctica", ParentM49Code: null }).ShouldBeTrue();
	}

	[Fact]
	void Map_emits_byte_identical_tsv_output_against_the_committed_seed_files()
	{
		using var reader = TabularReader.OpenDelimited(RawDatasets.UnsdM49(), ';');
		var (regions, countries) = UnsdM49Mapper.Map(reader);

		var regionPath = Path.GetTempFileName();
		var countryPath = Path.GetTempFileName();
		try
		{
			UnsdM49Writer.WriteRegions(regionPath, regions);
			UnsdM49Writer.WriteCountries(countryPath, countries);

			File.ReadAllBytes(regionPath).ShouldBe(File.ReadAllBytes(RegionTsvPath));
			File.ReadAllBytes(countryPath).ShouldBe(File.ReadAllBytes(CountryOrAreaTsvPath));
		}
		finally
		{
			File.Delete(regionPath);
			File.Delete(countryPath);
		}
	}
}
