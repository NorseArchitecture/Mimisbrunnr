namespace Norse.Reference.Data;

/// <summary>
/// The hierarchical level of a geographic region per UN M49.
/// </summary>
public enum RegionLevel : byte
{
	/// <summary>Sentinel CLR default — never a valid level; a region always declares its tier.</summary>
	Unspecified = 0,
	/// <summary>Region level.</summary>
	Region = 1,
	/// <summary>Subregion level.</summary>
	Subregion = 2,
	/// <summary>Intermediate region level.</summary>
	IntermediateRegion = 3
}
