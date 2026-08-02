using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace Norse.Reference.Data.Primitives.Generator.Tests;

public sealed class Iso3166EmissionTests
{
	// Same excerpt as IsoCountryCodeEmissionTests — one ISO-bearing row (United States of America)
	// plus the synthetic ISO-less row (Channel Islands) that must never reach the dataset either.
	const string Csv =
		"""
		Global Code;Global Name;Region Code;Region Name;Sub-region Code;Sub-region Name;Intermediate Region Code;Intermediate Region Name;Country or Area;M49 Code;ISO-alpha2 Code;ISO-alpha3 Code;Least Developed Countries (LDC);Land Locked Developing Countries (LLDC);Small Island Developing States (SIDS)
		001;World;019;Americas;021;Northern America;;;United States of America;840;US;USA;;;
		001;World;150;Europe;155;Western Europe;;;Austria;040;AT;AUT;;;
		001;World;150;Europe;154;Northern Europe;830;Channel Islands;Channel Islands;830;;;;;
		""";

	// RFC 9562 version 5 / variant bits: third group starts with "5", fourth group's first nibble is 8-b.
	static readonly Regex _v5GuidLiteral = new(
		"""new global::System\.Guid\("[0-9a-f]{8}-[0-9a-f]{4}-5[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}"\)""",
		RegexOptions.None);

	[Fact]
	void Emits_the_iso3166_dataset_with_a_v5_guid_for_the_us_row()
	{
		var generated = GeneratorTestHarness.Run(Csv);
		generated.ShouldContain("sealed record Iso3166Country(IsoCountryCode Code, string Alpha2, string Alpha3, string Name, global::System.Guid Id)");
		generated.ShouldContain("new(IsoCountryCode.UnitedStatesOfAmerica, \"US\", \"USA\", \"United States of America\",");
		_v5GuidLiteral.IsMatch(generated).ShouldBeTrue();
		generated.ShouldNotContain("ChannelIslands");
	}

	[Fact]
	void Emits_the_ids_frozen_dictionary()
	{
		var generated = GeneratorTestHarness.Run(Csv);
		generated.ShouldContain("static readonly global::System.Collections.Frozen.FrozenDictionary<IsoCountryCode, global::System.Guid> Ids");
		generated.ShouldContain("[IsoCountryCode.UnitedStatesOfAmerica] = new global::System.Guid(\"");
	}

	[Fact]
	void Emitted_source_compiles_clean()
	{
		GeneratorTestHarness.RunAndCompile(Csv).GetDiagnostics(TestContext.Current.CancellationToken)
			.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
	}
}
