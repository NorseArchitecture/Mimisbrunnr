using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Norse.Abstractions.Emit;

namespace Norse.Reference.Data.Primitives.Generator;

/// <summary>
/// Parses the UNSD M49 raw CSV, carried into the compilation as an <see cref="AdditionalText"/>, and
/// emits the <c>Norse.Reference.IsoCountryCode</c> enum plus its tri-form span-based parser into
/// <c>Norse.Reference.Data.Primitives</c>, and the <c>Norse.Reference.ReferenceNamespaces</c>
/// constants into <c>Norse.Reference.Data.Namespaces</c> — dispatching on the compilation's assembly
/// name so each assembly only ever sees the source it owns. Reports <c>NORSE050</c> when the header
/// is missing an expected column and <c>NORSE051</c> when two rows sanitize to the same identifier —
/// both fail loud rather than emit a partial or ambiguous surface.
/// </summary>
[Generator]
public sealed class ReferenceDataPrimitivesGenerator : IIncrementalGenerator
{
	const string CsvFileName = "UNSD — Methodology.csv";

	const string PrimitivesAssemblyName = "Norse.Reference.Data.Primitives";
	const string NamespacesAssemblyName = "Norse.Reference.Data.Namespaces";

	// ReferenceNamespaces.Root — the single hand-minted act (spec §6); every dataset namespace chains
	// from it. FOREVER: changing it re-keys the universe.
	const string RootUuid = "8db01f36-dd6e-4cd1-8233-7ab1ec672fff";

	static readonly DiagnosticDescriptor _missingColumn = new(
		"NORSE050", "UNSD CSV header is missing an expected column",
		"The UNSD raw CSV header does not contain the expected column '{0}' — refusing to emit an empty IsoCountryCode enum", "Norse.Reference",
		DiagnosticSeverity.Error, isEnabledByDefault: true);

	static readonly DiagnosticDescriptor _identifierCollision = new(
		"NORSE051", "Sanitized country identifier collides with another row",
		"Sanitized identifier '{0}' for '{1}' (M49 {2}) collides with the identifier already produced for '{3}' (M49 {4}) — refusing to disambiguate silently", "Norse.Reference",
		DiagnosticSeverity.Error, isEnabledByDefault: true);

	/// <inheritdoc />
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var assemblyName = context.CompilationProvider
			.Select(static (compilation, _) => compilation.AssemblyName ?? string.Empty);

		context.RegisterSourceOutput(assemblyName, static (ctx, name) =>
		{
			if (name != NamespacesAssemblyName)
				return;

			var iso3166Namespace = Uuid5.Compute(new Guid(RootUuid), Iso3166Emitter.Iso3166NamespaceName);
			ctx.AddSource("ReferenceNamespaces.g.cs",
				SourceText.From(NamespacesEmitter.Emit(RootUuid, iso3166Namespace), Utf8NoBom.Encoding));
		});

		var csvTexts = context.AdditionalTextsProvider
			.Where(static file => Path.GetFileName(file.Path) == CsvFileName)
			.Select(static (file, ct) => file.GetText(ct)?.ToString() ?? string.Empty)
			.Combine(assemblyName);

		context.RegisterSourceOutput(csvTexts, static (ctx, pair) =>
		{
			var (csvText, name) = pair;
			if (name == PrimitivesAssemblyName)
				Emit(ctx, csvText);
		});
	}

	static void Emit(SourceProductionContext ctx, string csvText)
	{
		if (!CsvParser.TryParse(csvText, out var rows, out var missingColumn))
		{
			ctx.ReportDiagnostic(Diagnostic.Create(_missingColumn, Location.None, missingColumn));
			return;
		}

		Dictionary<string, (string Name, string M49Code)> seenIdentifiers = [with(StringComparer.Ordinal)];
		List<CountryMember> members = [];
		var hasCollision = false;

		foreach (var row in rows)
		{
			if (!ushort.TryParse(row.M49Code, NumberStyles.None, CultureInfo.InvariantCulture, out var numeric))
				continue;

			var identifier = NameSanitizer.Sanitize(row.CountryOrArea);
			if (identifier.Length == 0)
				continue;

			if (seenIdentifiers.TryGetValue(identifier, out var existing))
			{
				ctx.ReportDiagnostic(Diagnostic.Create(_identifierCollision, Location.None,
					identifier, row.CountryOrArea, row.M49Code, existing.Name, existing.M49Code));
				hasCollision = true;
				continue;
			}

			seenIdentifiers.Add(identifier, (row.CountryOrArea, row.M49Code));
			members.Add(new CountryMember(identifier, numeric, row.CountryOrArea, row.Alpha2, row.Alpha3));
		}

		if (hasCollision)
			return;

		var source = IsoCountryCodeEmitter.Emit(members);
		ctx.AddSource("IsoCountryCode.g.cs", SourceText.From(source, Utf8NoBom.Encoding));

		var datasetSource = Iso3166Emitter.Emit(members, RootUuid);
		ctx.AddSource("Iso3166.g.cs", SourceText.From(datasetSource, Utf8NoBom.Encoding));
	}
}
