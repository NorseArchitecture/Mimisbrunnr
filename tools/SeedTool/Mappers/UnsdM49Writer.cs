using System.Text;

namespace Norse.SeedTool.Mappers;

static class UnsdM49Writer
{
	static readonly UTF8Encoding _utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

	public static void WriteRegions(string path, IReadOnlyList<RegionRow> regions)
	{
		using var writer = OpenSeedWriter(path);
		writer.WriteLine("M49Code\tName\tLevel\tParentM49Code");
		foreach (var region in regions)
			writer.WriteLine($"{region.M49Code}\t{region.Name}\t{region.Level}\t{region.ParentM49Code}");
	}

	public static void WriteCountries(string path, IReadOnlyList<CountryOrAreaRow> countries)
	{
		using var writer = OpenSeedWriter(path);
		writer.WriteLine(
			"M49Code\tIsoAlpha2Code\tIsoAlpha3Code\tName\tParentM49Code\tIsLeastDevelopedCountry\tIsLandLockedDevelopingCountry\tIsSmallIslandDevelopingState");
		foreach (var country in countries)
			writer.WriteLine(string.Join('\t',
				country.M49Code,
				country.IsoAlpha2Code,
				country.IsoAlpha3Code,
				country.Name,
				country.ParentM49Code,
				FormatFlag(country.IsLeastDevelopedCountry),
				FormatFlag(country.IsLandLockedDevelopingCountry),
				FormatFlag(country.IsSmallIslandDevelopingState)));
	}

	// The committed seeds are UTF-8 without BOM and LF-only, and .gitattributes pins them that way
	// (`seeds/*.tsv eol=lf`). StreamWriter defaults NewLine to Environment.NewLine, so left alone this
	// tool emits CRLF on a Windows dev box and LF everywhere else — a re-run would rewrite every seed
	// line for no content change, and the byte-identity fact in SeedTool.Tests fails on Windows only.
	static StreamWriter OpenSeedWriter(string path) =>
		new(path, append: false, _utf8NoBom) { NewLine = "\n" };

	static string FormatFlag(bool value) =>
		value ?
			"true" :
			"false";
}
