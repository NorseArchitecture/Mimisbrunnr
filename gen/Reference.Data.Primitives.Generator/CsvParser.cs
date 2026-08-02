using System.Text;

namespace Norse.Reference.Data.Primitives.Generator;

/// <summary>
/// Minimal hand-rolled semicolon-delimited CSV reader for the UNSD raw file, netstandard2.0-clean
/// (no <c>nietras.SeparatedValues</c> — a generator runs inside the compiler process and cannot take
/// a runtime CSV dependency). Handles quoted fields defensively (RFC 4180 double-quote escaping) even
/// though the current UNSD export carries none, since a country or area name can legitimately contain
/// the field delimiter.
/// </summary>
static class CsvParser
{
	internal const string CountryOrAreaColumn = "Country or Area";
	internal const string M49CodeColumn = "M49 Code";
	internal const string Alpha2CodeColumn = "ISO-alpha2 Code";
	internal const string Alpha3CodeColumn = "ISO-alpha3 Code";

	static readonly string[] _requiredColumns =
	[
		CountryOrAreaColumn,
		M49CodeColumn,
		Alpha2CodeColumn,
		Alpha3CodeColumn
	];

	/// <summary>
	/// Parses the raw CSV text into rows carrying exactly the four columns this generator consumes.
	/// A row whose alpha-2 or alpha-3 code is empty is skipped — it names a region/grouping, not an
	/// ISO-bearing country or area.
	/// </summary>
	/// <param name="csvText">The raw file contents.</param>
	/// <param name="rows">The parsed, ISO-bearing rows. Empty when parsing fails.</param>
	/// <param name="missingColumn">
	/// The first expected column name absent from the header row, or <see langword="null"/> when the
	/// header carries every expected column.
	/// </param>
	/// <returns><see langword="true"/> when the header carried every expected column.</returns>
	internal static bool TryParse(string csvText, out IReadOnlyList<CsvRow> rows, out string? missingColumn)
	{
		var lines = SplitLines(csvText);
		if (lines.Count == 0)
		{
			rows = [];
			missingColumn = CountryOrAreaColumn;
			return false;
		}

		var header = SplitFields(lines[0]);
		var indexByColumn = new Dictionary<string, int>(header.Count);
		for (var i = 0; i < header.Count; i++)
			indexByColumn[header[i]] = i;

		foreach (var required in _requiredColumns)
		{
			if (indexByColumn.ContainsKey(required))
				continue;

			rows = [];
			missingColumn = required;
			return false;
		}

		var countryIndex = indexByColumn[CountryOrAreaColumn];
		var m49Index = indexByColumn[M49CodeColumn];
		var alpha2Index = indexByColumn[Alpha2CodeColumn];
		var alpha3Index = indexByColumn[Alpha3CodeColumn];

		List<CsvRow> parsed = [];
		for (var i = 1; i < lines.Count; i++)
		{
			var line = lines[i];
			if (line.Length == 0)
				continue;

			var fields = SplitFields(line);
			var alpha2 = Field(fields, alpha2Index);
			var alpha3 = Field(fields, alpha3Index);
			if (alpha2.Length == 0 || alpha3.Length == 0)
				continue;

			parsed.Add(new CsvRow(Field(fields, countryIndex), Field(fields, m49Index), alpha2, alpha3));
		}

		rows = parsed;
		missingColumn = null;
		return true;
	}

	static string Field(List<string> fields, int index) =>
		index < fields.Count ? fields[index] : string.Empty;

	static List<string> SplitLines(string text)
	{
		List<string> lines = [];
		var start = 0;
		for (var i = 0; i < text.Length; i++)
		{
			if (text[i] != '\n')
				continue;

			var end = i > start && text[i - 1] == '\r' ? i - 1 : i;
			lines.Add(text.Substring(start, end - start));
			start = i + 1;
		}

		if (start < text.Length)
			lines.Add(text.Substring(start));

		return lines;
	}

	static List<string> SplitFields(string line)
	{
		List<string> fields = [];
		StringBuilder field = new();
		var inQuotes = false;

		for (var i = 0; i < line.Length; i++)
		{
			var c = line[i];
			if (inQuotes)
			{
				if (c != '"')
				{
					field.Append(c);
					continue;
				}

				if (i + 1 < line.Length && line[i + 1] == '"')
				{
					field.Append('"');
					i++;
					continue;
				}

				inQuotes = false;
				continue;
			}

			if (c == '"' && field.Length == 0)
			{
				inQuotes = true;
				continue;
			}

			if (c == ';')
			{
				fields.Add(field.ToString());
				field.Clear();
				continue;
			}

			field.Append(c);
		}

		fields.Add(field.ToString());
		return fields;
	}
}

/// <summary>One data row's worth of the four columns <see cref="CsvParser"/> consumes.</summary>
readonly struct CsvRow
{
	internal CsvRow(string countryOrArea, string m49Code, string alpha2, string alpha3)
	{
		CountryOrArea = countryOrArea;
		M49Code = m49Code;
		Alpha2 = alpha2;
		Alpha3 = alpha3;
	}

	internal string CountryOrArea { get; }

	internal string M49Code { get; }

	internal string Alpha2 { get; }

	internal string Alpha3 { get; }
}
