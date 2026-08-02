namespace Norse.Reference.Data.EntityFramework;

/// <summary>
/// UN classification flags for a <see cref="CountryOrArea"/> — Least Developed Country, Land Locked
/// Developing Country, and Small Island Developing State are independent, non-exclusive designations
/// a country or area can hold in combination. Test membership via <see cref="Enum.HasFlag"/>.
/// </summary>
[Flags]
public enum Classification : byte
{
	/// <summary>No UN classification applies.</summary>
	None = 0,
	/// <summary>Least Developed Country.</summary>
	LeastDevelopedCountry = 1 << 0,
	/// <summary>Land Locked Developing Country.</summary>
	LandLockedDevelopingCountry = 1 << 1,
	/// <summary>Small Island Developing State.</summary>
	SmallIslandDevelopingState = 1 << 2
}
