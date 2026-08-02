using Norse.Primitives.Identifiers;

namespace Norse.Reference.Data.EntityFramework;

/// <summary>The Subregion ancestor nested within a <see cref="RegionNode"/>.</summary>
public sealed record SubregionNode
{
	/// <summary>The Subregion's identifier.</summary>
	public required DeterministicGuid Id { get; init; }

	/// <summary>The Subregion's UN M49 code.</summary>
	public required string Code { get; init; }

	/// <summary>The Subregion's name.</summary>
	public required string Name { get; init; }

	/// <summary>The Intermediate Region beneath this Subregion, if one exists.</summary>
	public IntermediateRegionNode? IntermediateRegion { get; init; }
}
