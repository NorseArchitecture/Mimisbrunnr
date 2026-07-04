using System.Text;

namespace Norse.SeedTool.Mappers;

static class UnsdM49Writer
{
	static readonly UTF8Encoding _utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

	public static void WriteRegions(string path, IReadOnlyList<RegionRow> regions)
	{
		using StreamWriter writer = new(path, append: false, _utf8NoBom);
		writer.WriteLine("M49Code\tName\tLevel\tParentM49Code");
		foreach (var region in regions)
			writer.WriteLine($"{region.M49Code}\t{region.Name}\t{region.Level}\t{region.ParentM49Code}");
	}

	public static void WriteCountries(string path, IReadOnlyList<CountryOrAreaRow> countries)
	{
		using StreamWriter writer = new(path, append: false, _utf8NoBom);
		writer.WriteLine("M49Code\tIsoAlpha2Code\tIsoAlpha3Code\tName\tParentM49Code\tIsLeastDevelopedCountry\tIsLandLockedDevelopingCountry\tIsSmallIslandDevelopingState");
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

	static string FormatFlag(bool value) =>
		value ? "true" : "false";
}
