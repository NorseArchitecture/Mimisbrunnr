namespace Norse.Reference.Data.Primitives.Tests;

public sealed class Iso3166DatasetTests
{
	[Fact]
	void The_dataset_carries_every_iso_bearing_row() =>
		// Arithmetic, verified against the committed export 2026-07-31: the raw file is 249 lines
		// = 1 header + 248 data rows, and ZERO rows lack ISO alpha codes — so the ISO-bearing
		// count equals the data-row count exactly. If this assertion ever fails, the EXPORT
		// changed (a UNSD reissue): re-run the arithmetic against the new file and update this
		// number with the new count-minus-ISO-less breakdown in this comment — never edit the
		// number to whatever passes.
		Iso3166.All.Count.ShouldBe(248);
}
