namespace Norse.ReferenceData.Data;

/// <summary>
/// The Region ancestor of a <see cref="CountryOrArea.View"/> graph — an owned JSON document,
/// never a separately-queried table or view. Hydrated by the seed contributor at seed time.
/// </summary>
public sealed class RegionNode
{
	/// <summary>The Region's UN M49 code.</summary>
	public string Code { get; init; } = null!;
	/// <summary>The Region's name.</summary>
	public string Name { get; init; } = null!;
	/// <summary>The Subregion beneath this Region, if the leaf country resolved through one.</summary>
	public SubregionNode? Subregion { get; init; }
}
