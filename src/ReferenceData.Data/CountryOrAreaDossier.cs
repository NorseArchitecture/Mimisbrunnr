namespace Norse.ReferenceData.Data;

/// <summary>
/// The nested country-or-area dossier document read from the <c>country_or_area_dossier</c> view.
/// </summary>
public sealed record CountryOrAreaDossier(
	string Code,
	string Alpha2,
	string Alpha3,
	string Name,
	bool IsLeastDevelopedCountry,
	bool IsLandLockedDevelopingCountry,
	bool IsSmallIslandDevelopingState,
	RegionDossier? Region);

/// <summary>The region ancestor of a <see cref="CountryOrAreaDossier"/>.</summary>
public sealed record RegionDossier(string Code, string Name, SubregionDossier? Subregion);

/// <summary>The subregion ancestor of a <see cref="RegionDossier"/>.</summary>
public sealed record SubregionDossier(string Code, string Name, IntermediateRegionDossier? IntermediateRegion);

/// <summary>The intermediate-region ancestor of a <see cref="SubregionDossier"/>.</summary>
public sealed record IntermediateRegionDossier(string Code, string Name);
