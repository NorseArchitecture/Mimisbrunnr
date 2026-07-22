namespace Norse.Reference.Data;

/// <summary>The Intermediate Region ancestor nested within a <see cref="SubregionNode"/>.</summary>
public sealed class IntermediateRegionNode
{
	/// <summary>The Intermediate Region's UN M49 code.</summary>
	public string Code { get; init; } = null!;
	/// <summary>The Intermediate Region's name.</summary>
	public string Name { get; init; } = null!;
}
