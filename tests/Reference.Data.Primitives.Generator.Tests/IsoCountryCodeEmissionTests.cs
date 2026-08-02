using Microsoft.CodeAnalysis;

namespace Norse.Reference.Data.Primitives.Generator.Tests;

public sealed class IsoCountryCodeEmissionTests
{
	// Semicolon-delimited excerpt matching the real file's header + tricky rows.
	const string Csv =
		"""
		Global Code;Global Name;Region Code;Region Name;Sub-region Code;Sub-region Name;Intermediate Region Code;Intermediate Region Name;Country or Area;M49 Code;ISO-alpha2 Code;ISO-alpha3 Code;Least Developed Countries (LDC);Land Locked Developing Countries (LLDC);Small Island Developing States (SIDS)
		001;World;019;Americas;021;Northern America;;;United States of America;840;US;USA;;;
		001;World;150;Europe;155;Western Europe;;;Austria;040;AT;AUT;;;
		001;World;002;Africa;011;Western Africa;;;Côte d’Ivoire;384;CI;CIV;;;
		001;World;150;Europe;154;Northern Europe;830;Channel Islands;Channel Islands;830;;;;;
		001;World;019;Americas;419;Latin America and the Caribbean;029;Caribbean;Bonaire, Sint Eustatius and Saba;535;BQ;BES;;;
		""";

	[Fact]
	void Emits_sanitized_members_with_m49_values_and_skips_non_iso_rows()
	{
		var generated = GeneratorTestHarness.Run(Csv);
		generated.ShouldContain("UnitedStatesOfAmerica = 840");
		generated.ShouldContain("Austria = 40");
		generated.ShouldContain("CoteDIvoire = 384");
		generated.ShouldContain("BonaireSintEustatiusAndSaba = 535");
		// The skip rule (spec §6) has no live exercise in the real export — every one of its 248
		// data rows is ISO-bearing — so this synthetic ISO-less row is the rule's only pin.
		generated.ShouldNotContain("ChannelIslands");
		generated.ShouldContain("None = 0");
	}

	[Fact]
	void Emitted_source_compiles_clean()
	{
		GeneratorTestHarness.RunAndCompile(Csv).GetDiagnostics(TestContext.Current.CancellationToken)
			.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
	}
}
