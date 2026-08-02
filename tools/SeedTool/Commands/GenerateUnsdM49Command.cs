using System.ComponentModel;
using Norse.Primitives.Ingestion;
using Norse.SeedTool.Mappers;
using Spectre.Console.Cli;

namespace Norse.SeedTool.Commands;

sealed class GenerateUnsdM49Command : Command<GenerateUnsdM49Command.Settings>
{
	public sealed class Settings : CommandSettings
	{
		[CommandArgument(0, "[outputDirectory]")]
		[Description("Directory to write region.tsv and country-or-area.tsv into.")]
		public string OutputDirectory { get; init; } = "seeds";

		[CommandArgument(1, "[inputFile]")]
		[Description("Path to the raw UNSD M49 methodology CSV.")]
		public string InputFile { get; init; } = Path.Combine("seeds", "raw", "UNSD — Methodology.csv");
	}

	protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
	{
		using ITabularReader reader = TabularReader.OpenDelimited(settings.InputFile, ';');
		var (regions, countries) = UnsdM49Mapper.Map(reader);

		var regionPath = Path.Combine(settings.OutputDirectory, "region.tsv");
		var countryPath = Path.Combine(settings.OutputDirectory, "country-or-area.tsv");

		UnsdM49Writer.WriteRegions(regionPath, regions);
		UnsdM49Writer.WriteCountries(countryPath, countries);

		Console.WriteLine($"Wrote {regions.Count} region rows to {regionPath}");
		Console.WriteLine($"Wrote {countries.Count} country rows to {countryPath}");
		return 0;
	}
}
