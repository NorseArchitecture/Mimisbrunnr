namespace Norse.SeedTool.Mappers;

sealed record CountryOrAreaRow(
	string M49Code,
	string IsoAlpha2Code,
	string IsoAlpha3Code,
	string Name,
	string? ParentM49Code,
	bool IsLeastDevelopedCountry,
	bool IsLandLockedDevelopingCountry,
	bool IsSmallIslandDevelopingState);
