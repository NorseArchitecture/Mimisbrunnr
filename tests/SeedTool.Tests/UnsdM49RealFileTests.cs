using HyperTabular;
using Norse.SeedTool.Mappers;

namespace Norse.SeedTool.Tests;

public sealed class UnsdM49RealFileTests
{
	// Embedded in this test assembly (EmbeddedResource, LogicalName = bare file name), never read
	// off disk — see the csproj for why a relative path from the test binary cannot work here.
	const string RawCsvResource = "UNSD — Methodology.csv";
	const string RegionTsvResource = "region.tsv";
	const string CountryOrAreaTsvResource = "country-or-area.tsv";

	static Stream OpenResource(string name) =>
		typeof(UnsdM49RealFileTests).Assembly.GetManifestResourceStream(name)
		?? throw new InvalidOperationException($"Embedded seed resource '{name}' was not found.");

	static byte[] ReadResource(string name)
	{
		using var stream = OpenResource(name);
		using MemoryStream buffer = new();
		stream.CopyTo(buffer);
		return buffer.ToArray();
	}

	[Fact]
	void Map_produces_the_expected_counts_and_known_rows_from_the_real_source()
	{
		using var reader = new DelimitedReader(OpenResource(RawCsvResource), UnsdM49Mapper.SourceDialect);
		var (regions, countries) = UnsdM49Mapper.Map(reader);

		// 5 Regions + 17 Sub-regions + 7 Intermediate Regions, per the approved M49 spec's
		// verified data facts (Glitnir/docs/Mimisbrunnr/specs/2026-07-04-unsd-m49-reference-data-design.md §1).
		regions.Count.ShouldBe(29);
		countries.Count.ShouldBe(248);

		countries.Any(c => c is { M49Code: "566", Name: "Nigeria", IsoAlpha2Code: "NG", IsoAlpha3Code: "NGA" })
			.ShouldBeTrue();
		countries.Any(c => c is { M49Code: "010", Name: "Antarctica", ParentM49Code: null }).ShouldBeTrue();
	}

	[Fact]
	void Map_emits_byte_identical_tsv_output_against_the_committed_seed_files()
	{
		using var reader = new DelimitedReader(OpenResource(RawCsvResource), UnsdM49Mapper.SourceDialect);
		var (regions, countries) = UnsdM49Mapper.Map(reader);

		var regionPath = Path.GetTempFileName();
		var countryPath = Path.GetTempFileName();
		try
		{
			UnsdM49Writer.WriteRegions(regionPath, regions);
			UnsdM49Writer.WriteCountries(countryPath, countries);

			File.ReadAllBytes(regionPath).ShouldBe(ReadResource(RegionTsvResource));
			File.ReadAllBytes(countryPath).ShouldBe(ReadResource(CountryOrAreaTsvResource));
		}
		finally
		{
			File.Delete(regionPath);
			File.Delete(countryPath);
		}
	}
}
