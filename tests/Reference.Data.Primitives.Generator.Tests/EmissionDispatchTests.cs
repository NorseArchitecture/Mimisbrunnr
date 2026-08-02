using Microsoft.CodeAnalysis;

namespace Norse.Reference.Data.Primitives.Generator.Tests;

public sealed class EmissionDispatchTests
{
	const string Csv =
		"""
		Global Code;Global Name;Region Code;Region Name;Sub-region Code;Sub-region Name;Intermediate Region Code;Intermediate Region Name;Country or Area;M49 Code;ISO-alpha2 Code;ISO-alpha3 Code;Least Developed Countries (LDC);Land Locked Developing Countries (LLDC);Small Island Developing States (SIDS)
		001;World;019;Americas;021;Northern America;;;United States of America;840;US;USA;;;
		""";

	[Fact]
	void The_primitives_assembly_gets_the_enum_and_dataset_but_never_the_namespaces_class()
	{
		var generated = GeneratorTestHarness.Run(Csv);
		generated.ShouldContain("public enum IsoCountryCode : ushort");
		generated.ShouldContain("public static class Iso3166");
		generated.ShouldNotContain("class ReferenceNamespaces");
	}

	[Fact]
	void The_namespaces_assembly_gets_only_the_namespaces_class()
	{
		var generated = GeneratorTestHarness.Run(Csv, "Norse.Reference.Data.Namespaces");
		generated.ShouldContain("public static class ReferenceNamespaces");
		generated.ShouldContain("public static readonly global::System.Guid Root = new(\"8db01f36-dd6e-4cd1-8233-7ab1ec672fff\")");
		generated.ShouldContain("public static readonly global::System.Guid Iso3166 = new(\"");
		generated.ShouldNotContain("enum IsoCountryCode");
	}

	[Fact]
	void Any_other_assembly_gets_nothing() =>
		GeneratorTestHarness.Run(Csv, "Norse.Something.Else").ShouldBeEmpty();

	[Fact]
	void The_namespaces_emission_compiles_clean() =>
		GeneratorTestHarness.RunAndCompile(Csv, "Norse.Reference.Data.Namespaces")
			.GetDiagnostics(TestContext.Current.CancellationToken)
			.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
}
