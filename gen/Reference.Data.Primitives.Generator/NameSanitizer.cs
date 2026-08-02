using System.Globalization;
using System.Text;

namespace Norse.Reference.Data.Primitives.Generator;

/// <summary>
/// Deterministic identifier rule that turns a UNSD English short name into a valid C# enum member
/// name: Unicode-normalize and strip combining marks (<c>Côte d'Ivoire</c> → <c>Cote d'Ivoire</c>),
/// drop any parenthesized segment (<c>Bolivia (Plurinational State of)</c> → <c>Bolivia</c>), then
/// PascalCase every remaining alphanumeric run, treating any other character as a word boundary.
/// Collision detection lives with the caller — this type only sanitizes one name at a time.
/// </summary>
static class NameSanitizer
{
	/// <summary>Sanitizes a raw UNSD country/area name into a PascalCase C# identifier.</summary>
	/// <param name="rawName">The raw "Country or Area" column value.</param>
	/// <returns>The sanitized identifier. Empty when the name carries no alphanumeric characters.</returns>
	internal static string Sanitize(string rawName)
	{
		var normalized = rawName.Normalize(NormalizationForm.FormD);
		var withoutMarks = StripCombiningMarks(normalized);
		var withoutParens = RemoveParenthesizedSegments(withoutMarks);
		return PascalCase(withoutParens);
	}

	static string StripCombiningMarks(string value)
	{
		StringBuilder sb = new(value.Length);
		foreach (var c in value)
		{
			if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
				sb.Append(c);
		}

		return sb.ToString();
	}

	static string RemoveParenthesizedSegments(string value)
	{
		StringBuilder sb = new(value.Length);
		var depth = 0;
		foreach (var c in value)
		{
			switch (c)
			{
				case '(':
					depth++;
					break;
				case ')':
					if (depth > 0)
						depth--;
					break;
				default:
					if (depth == 0)
						sb.Append(c);
					break;
			}
		}

		return sb.ToString();
	}

	static string PascalCase(string value)
	{
		StringBuilder sb = new(value.Length);
		var startOfWord = true;
		foreach (var c in value)
		{
			if (!char.IsLetterOrDigit(c))
			{
				startOfWord = true;
				continue;
			}

			sb.Append(startOfWord ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c));
			startOfWord = false;
		}

		return sb.ToString();
	}
}
