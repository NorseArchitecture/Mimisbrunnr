using Norse.Primitives.Ingestion;
using Norse.SeedTool.Mappers;

namespace Norse.SeedTool.Tests;

public sealed class UnsdM49RealFileTests
{
	const string SourcePath = "../../../../../seeds/raw/UNSD — Methodology.csv";

	[Fact]
	void Map_produces_the_expected_counts_and_known_rows_from_the_real_source()
	{
		using ITabularReader reader = TabularReader.OpenDelimited(SourcePath, ';');
		var (regions, countries) = UnsdM49Mapper.Map(reader);

		// 5 Regions + 17 Sub-regions + 7 Intermediate Regions, per the approved M49 spec's
		// verified data facts (Glitnir/docs/Mimisbrunnr/specs/2026-07-04-unsd-m49-reference-data-design.md §1).
		regions.Count.ShouldBe(29);
		countries.Count.ShouldBe(248);

		countries.Any(c => c is { M49Code: "566", Name: "Nigeria", IsoAlpha2Code: "NG", IsoAlpha3Code: "NGA" }).ShouldBeTrue();
		countries.Any(c => c is { M49Code: "010", Name: "Antarctica", ParentM49Code: null }).ShouldBeTrue();
	}
}
