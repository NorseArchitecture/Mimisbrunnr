using System.Globalization;
using Norse.Primitives;
using Norse.Primitives.Ingestion;

namespace Norse.SeedTool.Mappers;

static class UnsdM49Mapper
{
	static readonly Dictionary<string, int> _levelRank = new(StringComparer.Ordinal)
	{
		["Region"] = 1,
		["Subregion"] = 2,
		["IntermediateRegion"] = 3,
	};

	public static (IReadOnlyList<RegionRow> Regions, IReadOnlyList<CountryOrAreaRow> Countries) Map(ITabularReader reader)
	{
		var regionCodeOrdinal = reader.Ordinal("Region Code");
		var regionNameOrdinal = reader.Ordinal("Region Name");
		var subregionCodeOrdinal = reader.Ordinal("Sub-region Code");
		var subregionNameOrdinal = reader.Ordinal("Sub-region Name");
		var intermediateCodeOrdinal = reader.Ordinal("Intermediate Region Code");
		var intermediateNameOrdinal = reader.Ordinal("Intermediate Region Name");
		var countryNameOrdinal = reader.Ordinal("Country or Area");
		var m49Ordinal = reader.Ordinal("M49 Code");
		var iso2Ordinal = reader.Ordinal("ISO-alpha2 Code");
		var iso3Ordinal = reader.Ordinal("ISO-alpha3 Code");
		var ldcOrdinal = reader.Ordinal("Least Developed Countries (LDC)");
		var llcOrdinal = reader.Ordinal("Land Locked Developing Countries (LLDC)");
		var sidsOrdinal = reader.Ordinal("Small Island Developing States (SIDS)");

		Dictionary<string, RegionRow> regions = [];
		List<CountryOrAreaRow> countries = [];
		var rowNumber = 1; // header is row 1

		while (reader.Read())
		{
			rowNumber++;

			var regionCode = reader[regionCodeOrdinal];
			var subregionCode = reader[subregionCodeOrdinal];
			var intermediateCode = reader[intermediateCodeOrdinal];

			if (!regionCode.IsEmpty)
				AddRegionIfAbsent(regions, regionCode, reader[regionNameOrdinal], "Region", null, rowNumber, "Region Code");

			if (!subregionCode.IsEmpty)
				AddRegionIfAbsent(regions, subregionCode, reader[subregionNameOrdinal], "Subregion",
					ValidateM49Code(regionCode, rowNumber, "Region Code"), rowNumber, "Sub-region Code");

			if (!intermediateCode.IsEmpty)
				AddRegionIfAbsent(regions, intermediateCode, reader[intermediateNameOrdinal], "IntermediateRegion",
					ValidateM49Code(subregionCode, rowNumber, "Sub-region Code"), rowNumber, "Intermediate Region Code");

			var parentCode =
				!intermediateCode.IsEmpty ? ValidateM49Code(intermediateCode, rowNumber, "Intermediate Region Code")
				: !subregionCode.IsEmpty ? ValidateM49Code(subregionCode, rowNumber, "Sub-region Code")
				: !regionCode.IsEmpty ? ValidateM49Code(regionCode, rowNumber, "Region Code")
				: null;

			countries.Add(new CountryOrAreaRow(
				M49Code: ValidateM49Code(reader[m49Ordinal], rowNumber, "M49 Code"),
				IsoAlpha2Code: ValidateIsoAlpha(reader[iso2Ordinal], 2, rowNumber, "ISO-alpha2 Code"),
				IsoAlpha3Code: ValidateIsoAlpha(reader[iso3Ordinal], 3, rowNumber, "ISO-alpha3 Code"),
				Name: reader[countryNameOrdinal].ToString(),
				ParentM49Code: parentCode,
				IsLeastDevelopedCountry: ValidateFlag(reader[ldcOrdinal], rowNumber, "Least Developed Countries (LDC)"),
				IsLandLockedDevelopingCountry: ValidateFlag(reader[llcOrdinal], rowNumber, "Land Locked Developing Countries (LLDC)"),
				IsSmallIslandDevelopingState: ValidateFlag(reader[sidsOrdinal], rowNumber, "Small Island Developing States (SIDS)")));
		}

		var orderedRegions = regions.Values
			.OrderBy(r => _levelRank[r.Level])
			.ThenBy(r => r.M49Code, StringComparer.Ordinal)
			.ToList();

		return (orderedRegions, countries);
	}

	static void AddRegionIfAbsent(
		Dictionary<string, RegionRow> regions,
		ReadOnlySpan<char> codeSpan,
		ReadOnlySpan<char> nameSpan,
		string level,
		string? parentM49Code,
		int rowNumber,
		string columnName)
	{
		var code = ValidateM49Code(codeSpan, rowNumber, columnName);
		if (!regions.ContainsKey(code))
			regions[code] = new RegionRow(code, nameSpan.ToString(), level, parentM49Code);
	}

	static string ValidateM49Code(ReadOnlySpan<char> span, int rowNumber, string columnName)
	{
		var result = Parser.ParseRequired<ushort>(span, CultureInfo.InvariantCulture);
		if (result.TryGetValue(out Failure failure))
			throw new InvalidOperationException($"Row {rowNumber}, column '{columnName}': {failure.Reason} (\"{failure.Input}\").");
		result.TryGetValue(out Success<ushort> success);
		return success.Value.ToString("D3", CultureInfo.InvariantCulture);
	}

	static string ValidateIsoAlpha(ReadOnlySpan<char> span, int expectedLength, int rowNumber, string columnName)
	{
		if (span.Length != expectedLength || !AllUpperAscii(span))
			throw new InvalidOperationException($"Row {rowNumber}, column '{columnName}': expected {expectedLength} uppercase letters, got \"{span}\".");
		return span.ToString();
	}

	static bool AllUpperAscii(ReadOnlySpan<char> span)
	{
		foreach (var c in span)
			if (c is < 'A' or > 'Z')
				return false;
		return true;
	}

	static bool ValidateFlag(ReadOnlySpan<char> span, int rowNumber, string columnName)
	{
		var trimmed = span.Trim();
		if (trimmed.IsEmpty)
			return false;
		if (trimmed.Equals("x", StringComparison.OrdinalIgnoreCase))
			return true;
		throw new InvalidOperationException($"Row {rowNumber}, column '{columnName}': expected \"x\" or blank, got \"{trimmed}\".");
	}
}
