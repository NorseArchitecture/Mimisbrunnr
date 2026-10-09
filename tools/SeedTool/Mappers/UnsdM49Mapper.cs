using System.Globalization;
using HyperTabular;
using Norse.Primitives;

namespace Norse.SeedTool.Mappers;

static class UnsdM49Mapper
{
	/// <summary>The UNSD methodology CSV: semicolon-separated, header first, RFC quoting (unused), UTF-8 (a BOM is skipped).</summary>
	internal static readonly Dialect SourceDialect = Dialect.Csv with { Separator = ';' };

	// Output ordinals of the plan bound in Map, in plan order.
	const int
		RegionCode = 0,
		RegionName = 1,
		SubregionCode = 2,
		SubregionName = 3,
		IntermediateCode = 4,
		IntermediateName = 5,
		CountryName = 6,
		M49 = 7,
		Iso2 = 8,
		Iso3 = 9,
		Ldc = 10,
		Llc = 11,
		Sids = 12;

	static readonly Dictionary<string, int> _levelRank = new(StringComparer.Ordinal)
	{
		["Region"] = 1,
		["Subregion"] = 2,
		["IntermediateRegion"] = 3
	};

	public static (IReadOnlyList<RegionRow> Regions, IReadOnlyList<CountryOrAreaRow> Countries) Map(
		DelimitedReader reader)
	{
		var header = reader.Header ?? throw new InvalidOperationException("The UNSD source declares a header row.");
		reader.Bind(
		[
			Column.Text(header.Ordinal("Region Code")),
			Column.Text(header.Ordinal("Region Name")),
			Column.Text(header.Ordinal("Sub-region Code")),
			Column.Text(header.Ordinal("Sub-region Name")),
			Column.Text(header.Ordinal("Intermediate Region Code")),
			Column.Text(header.Ordinal("Intermediate Region Name")),
			Column.Text(header.Ordinal("Country or Area")),
			Column.Text(header.Ordinal("M49 Code")),
			Column.Text(header.Ordinal("ISO-alpha2 Code")),
			Column.Text(header.Ordinal("ISO-alpha3 Code")),
			Column.Text(header.Ordinal("Least Developed Countries (LDC)")),
			Column.Text(header.Ordinal("Land Locked Developing Countries (LLDC)")),
			Column.Text(header.Ordinal("Small Island Developing States (SIDS)"))
		]);

		Dictionary<string, RegionRow> regions = [];
		List<CountryOrAreaRow> countries = [];

		foreach (var row in reader.Rows())
		{
			var line = row.Line;
			var regionCode = row.GetChars(RegionCode);
			var subregionCode = row.GetChars(SubregionCode);
			var intermediateCode = row.GetChars(IntermediateCode);

			if (!regionCode.IsEmpty)
				AddRegionIfAbsent(regions, regionCode, row.GetChars(RegionName), "Region", null, line, "Region Code");

			if (!subregionCode.IsEmpty)
				AddRegionIfAbsent(regions, subregionCode, row.GetChars(SubregionName), "Subregion",
					ValidateM49Code(regionCode, line, "Region Code"), line, "Sub-region Code");

			if (!intermediateCode.IsEmpty)
				AddRegionIfAbsent(regions, intermediateCode, row.GetChars(IntermediateName), "IntermediateRegion",
					ValidateM49Code(subregionCode, line, "Sub-region Code"), line, "Intermediate Region Code");

			var parentCode =
				!intermediateCode.IsEmpty ? ValidateM49Code(intermediateCode, line, "Intermediate Region Code")
				: !subregionCode.IsEmpty ? ValidateM49Code(subregionCode, line, "Sub-region Code")
				: !regionCode.IsEmpty ? ValidateM49Code(regionCode, line, "Region Code")
				: null;

			countries.Add(new CountryOrAreaRow(
				ValidateM49Code(row.GetChars(M49), line, "M49 Code"),
				ValidateIsoAlpha(row.GetChars(Iso2), 2, line, "ISO-alpha2 Code"),
				ValidateIsoAlpha(row.GetChars(Iso3), 3, line, "ISO-alpha3 Code"),
				row.GetChars(CountryName).ToString(),
				parentCode,
				ValidateFlag(row.GetChars(Ldc), line, "Least Developed Countries (LDC)"),
				ValidateFlag(row.GetChars(Llc), line, "Land Locked Developing Countries (LLDC)"),
				ValidateFlag(row.GetChars(Sids), line, "Small Island Developing States (SIDS)")));
		}

		List<RegionRow> orderedRegions =
		[
			.. regions.Values
				.OrderBy(r => _levelRank[r.Level])
				.ThenBy(r => r.M49Code, StringComparer.Ordinal)
		];

		return (orderedRegions, countries);
	}

	static void AddRegionIfAbsent(
		Dictionary<string, RegionRow> regions,
		ReadOnlySpan<char> codeSpan,
		ReadOnlySpan<char> nameSpan,
		string level,
		string? parentM49Code,
		int line,
		string columnName)
	{
		var code = ValidateM49Code(codeSpan, line, columnName);
		if (!regions.ContainsKey(code))
			regions[code] = new RegionRow(code, nameSpan.ToString(), level, parentM49Code);
	}

	static string ValidateM49Code(ReadOnlySpan<char> span, int line, string columnName)
	{
		var result = Parser.ParseRequired<ushort>(span, CultureInfo.InvariantCulture);
		if (result.TryGetValue(out Failure failure))
			throw new InvalidOperationException(
				$"Row {line}, column '{columnName}': {failure.Reason} (\"{failure.Input}\").");
		result.TryGetValue(out Success<ushort> success);
		return success.Value.ToString("D3", CultureInfo.InvariantCulture);
	}

	static string ValidateIsoAlpha(ReadOnlySpan<char> span, int expectedLength, int line, string columnName)
	{
		if (span.Length != expectedLength || !AllUpperAscii(span))
			throw new InvalidOperationException(
				$"Row {line}, column '{columnName}': expected {expectedLength} uppercase letters, got \"{span}\".");
		return span.ToString();
	}

	static bool AllUpperAscii(ReadOnlySpan<char> span)
	{
		foreach (var c in span)
			if (c is < 'A' or > 'Z')
				return false;
		return true;
	}

	static bool ValidateFlag(ReadOnlySpan<char> span, int line, string columnName)
	{
		var trimmed = span.Trim();
		if (trimmed.IsEmpty)
			return false;
		if (trimmed.Equals("x", StringComparison.OrdinalIgnoreCase))
			return true;
		throw new InvalidOperationException(
			$"Row {line}, column '{columnName}': expected \"x\" or blank, got \"{trimmed}\".");
	}
}
