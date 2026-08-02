using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Norse.Reference.Data.Primitives.Generator.Tests;

/// <summary>
/// Shared <see cref="GeneratorDriver"/> harness: an in-memory compilation plus an in-memory
/// <see cref="AdditionalText"/> carrying the CSV under test, mirroring the pattern established by
/// Urdarbrunnr's <c>Persistence.EntityFramework.Generator.Tests</c> — the only difference is this
/// generator reads an <see cref="AdditionalText"/> rather than syntax trees.
/// </summary>
static class GeneratorTestHarness
{
	/// <summary>Runs the generator over <paramref name="csv"/> and returns the emitted source text.</summary>
	internal static string Run(string csv, string assemblyName = "Norse.Reference.Data.Primitives") =>
		string.Join("\n", Execute(csv, assemblyName).Result.GeneratedTrees.Select(tree => tree.ToString()));

	/// <summary>Runs the generator over <paramref name="csv"/> and returns the resulting compilation.</summary>
	internal static Compilation RunAndCompile(string csv, string assemblyName = "Norse.Reference.Data.Primitives") =>
		Execute(csv, assemblyName).OutputCompilation;

	static (Compilation OutputCompilation, GeneratorDriverRunResult Result) Execute(string csv, string assemblyName)
	{
		// The real Reference.Data.Primitives.csproj carries ImplicitUsings=enable and LangVersion=preview
		// (Mimisbrunnr's root Directory.Build.props) — both matter to the *emitted* source, which relies on
		// System's implicit global usings (MemoryExtensions.Trim/AsSpan) and the platform's hand-authored
		// C# union feature (Result<T> case-type conversions). Mirrored here so this harness's compilation
		// matches the real consumer, not a bare default.
		CSharpParseOptions parseOptions = new(LanguageVersion.Preview);
		var compilation = CreateCompilation(parseOptions, assemblyName);
		ReferenceDataPrimitivesGenerator generator = new();
		GeneratorDriver driver = CSharpGeneratorDriver.Create(
			[generator.AsSourceGenerator()],
			additionalTexts: [new InMemoryAdditionalText(csv)],
			parseOptions: parseOptions);

		driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _, TestContext.Current.CancellationToken);
		return (outputCompilation, driver.GetRunResult());
	}

	static Compilation CreateCompilation(CSharpParseOptions parseOptions, string assemblyName)
	{
		var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
		IList<MetadataReference> references =
		[
			.. Directory.GetFiles(runtimeDir, "*.dll").Select(path => (MetadataReference)MetadataReference.CreateFromFile(path)),
			MetadataReference.CreateFromFile(typeof(global::Norse.Primitives.Result<>).Assembly.Location)
		];

		// A real net11.0 ImplicitUsings=enable classlib's actual implicit-usings set -- not a
		// superset. No System.Collections.Frozen here: this proves the emitter supplies its own
		// `using System.Collections.Frozen;` rather than relying on an ambient using this test
		// harness happened to hand-supply.
		var globalUsings = CSharpSyntaxTree.ParseText(
			"""
			global using System;
			global using System.Collections.Generic;
			global using System.Linq;
			global using System.Threading.Tasks;
			global using System.IO;
			""", parseOptions);

		return CSharpCompilation.Create(
			assemblyName,
			[globalUsings],
			references,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
	}

	sealed class InMemoryAdditionalText(string csv) : AdditionalText
	{
		readonly SourceText _text = SourceText.From(csv, System.Text.Encoding.UTF8);

		public override string Path { get; } = "seeds/raw/UNSD — Methodology.csv";

		public override SourceText GetText(CancellationToken cancellationToken = default) => _text;
	}
}
