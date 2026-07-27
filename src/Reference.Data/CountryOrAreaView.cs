using Norse.Primitives.Identifiers;

namespace Norse.Reference.Data;

/// <summary>
/// The denormalized "peer + ancestry" read-model for a <see cref="CountryOrArea"/> — its own scalar
/// fields alongside the Region/Subregion/IntermediateRegion ancestor chain, hydrated by the seed
/// contributor and stored as an owned JSON document, queried without joins.
/// </summary>
public sealed record CountryOrAreaView
{
	/// <summary>The country-or-area identifier.</summary>
	public required DeterministicGuid Id { get; init; }
	/// <summary>The UN M49 code (3 digits).</summary>
	public required ushort Code { get; init; }
	/// <summary>The ISO 3166-1 alpha-2 code (2 letters).</summary>
	public required string Alpha2 { get; init; }
	/// <summary>The ISO 3166-1 alpha-3 code (3 letters).</summary>
	public required string Alpha3 { get; init; }
	/// <summary>The country or area name in English.</summary>
	public required string Name { get; init; }
	/// <summary>The UN classification flags this country or area holds. Test with <see cref="Enum.HasFlag"/>.</summary>
	public required Classification Classification { get; init; }
	/// <summary>The ancestor Region, if the country resolves through one — <see langword="null"/> only for Antarctica.</summary>
	public RegionNode? Region { get; init; }
}
