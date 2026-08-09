namespace Norse.Reference;

/// <summary>
///     UN classification flags for a country or area — Least Developed Country, Land Locked
///     Developing Country, and Small Island Developing State are independent, non-exclusive designations
///     a country or area can hold in combination. Test membership via <see cref="Enum.HasFlag" />.
///     Lives on the browser-safe contracts surface because wire responses carry the flags member
///     directly: the binary channel rides the composed varint, and the text channels render the
///     governed-name array form (ruled 2026-08-09) — the persistence side consumes it by reference.
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
