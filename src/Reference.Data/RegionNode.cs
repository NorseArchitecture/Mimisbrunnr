using Norse.Primitives.Identifiers;

namespace Norse.Reference.Data;

/// <summary>
/// The Region ancestor of a <see cref="CountryOrAreaView"/> graph — an owned JSON document,
/// never a separately-queried table or view. Hydrated by the seed contributor at seed time.
/// </summary>
public sealed record RegionNode
{
	/// <summary>The Region's identifier.</summary>
	public required DeterministicGuid Id { get; init; }
	/// <summary>The Region's UN M49 code.</summary>
	public required string Code { get; init; }
	/// <summary>The Region's name.</summary>
	public required string Name { get; init; }

	/// <summary>The Subregion beneath this Region, if the leaf country resolved through one.</summary>
	public SubregionNode Subregion { get; init; } = null!;
}
