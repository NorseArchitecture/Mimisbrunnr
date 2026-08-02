using Norse.Primitives.Identifiers;

namespace Norse.Reference.Data.EntityFramework;

/// <summary>The Intermediate Region ancestor nested within a <see cref="SubregionNode"/>.</summary>
public sealed record IntermediateRegionNode
{
	/// <summary>The Intermediate Region's identifier.</summary>
	public required DeterministicGuid Id { get; init; }
	/// <summary>The Intermediate Region's UN M49 code.</summary>
	public required string Code { get; init; }
	/// <summary>The Intermediate Region's name.</summary>
	public required string Name { get; init; }
}
