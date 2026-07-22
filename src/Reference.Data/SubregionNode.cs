namespace Norse.Reference.Data;

/// <summary>The Subregion ancestor nested within a <see cref="RegionNode"/>.</summary>
public sealed class SubregionNode
{
	/// <summary>The Subregion's UN M49 code.</summary>
	public string Code { get; init; } = null!;
	/// <summary>The Subregion's name.</summary>
	public string Name { get; init; } = null!;
	/// <summary>The Intermediate Region beneath this Subregion, if one exists.</summary>
	public IntermediateRegionNode? IntermediateRegion { get; init; }
}
