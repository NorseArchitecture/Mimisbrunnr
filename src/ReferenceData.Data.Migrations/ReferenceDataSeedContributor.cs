using Microsoft.EntityFrameworkCore;
using Norse.Abstractions.Migrations.Seeding;
using Norse.Primitives.Identifiers;
using Norse.Primitives.Ingestion;

namespace Norse.ReferenceData.Data.Migrations;

/// <summary>
/// Seeds <see cref="Region"/> and <see cref="CountryOrArea"/> rows from the committed UN M49 TSVs
/// (<c>seeds/region.tsv</c>, <c>seeds/country-or-area.tsv</c>), idempotently.
/// </summary>
/// <param name="context">The reference-data context instance resolved from DI.</param>
public sealed class ReferenceDataSeedContributor(ReferenceDataDbContext context) : ISeedContributor
{
	static readonly Guid _namespaceRegion =
		new DeterministicGuid(DeterministicGuid.Namespaces.Dns, "region.m49.referencedata.norse");

	static readonly Guid _namespaceCountryOrArea =
		new DeterministicGuid(DeterministicGuid.Namespaces.Dns, "country-or-area.m49.referencedata.norse");

	/// <inheritdoc />
	public string Name => "Norse.ReferenceData";

	/// <inheritdoc />
	public async Task SeedAsync(CancellationToken cancellationToken)
	{
		var regionIdByCode = await SeedRegionsAsync(cancellationToken).ConfigureAwait(false);
		await SeedCountriesAsync(regionIdByCode, cancellationToken).ConfigureAwait(false);
	}

	async Task<Dictionary<string, Guid>> SeedRegionsAsync(CancellationToken cancellationToken)
	{
		var regionIdByCode = new Dictionary<string, Guid>();
		List<(Guid Id, string M49Code, string Name, RegionLevel Level, string? ParentM49Code)> rows = [];

		using ITabularReader reader = TabularReader.OpenDelimited(
			Path.Combine(AppContext.BaseDirectory, "seeds", "region.tsv"), '\t');
		var m49Ordinal = reader.Ordinal("M49Code");
		var nameOrdinal = reader.Ordinal("Name");
		var levelOrdinal = reader.Ordinal("Level");
		var parentOrdinal = reader.Ordinal("ParentM49Code");

		while (reader.Read())
		{
			var m49Code = reader[m49Ordinal].ToString();
			// The TSV's Level column holds the enum member name (Region/Subregion/IntermediateRegion),
			// not a numeric value — written that way by tools/SeedTool's UnsdM49Writer.
			var level = Enum.Parse<RegionLevel>(reader[levelOrdinal]);
			var parentCode = reader[parentOrdinal].ToString();
			var id = new DeterministicGuid(_namespaceRegion, m49Code);

			regionIdByCode[m49Code] = id;
			rows.Add((id, m49Code, reader[nameOrdinal].ToString(), level, parentCode.Length == 0 ? null : parentCode));
		}

		var existingIds = (await context.Set<Region>().Select(r => r.Id).ToListAsync(cancellationToken).ConfigureAwait(false)).ToHashSet();

		foreach (var row in rows)
		{
			if (existingIds.Contains(row.Id))
				continue;

			context.Set<Region>().Add(new Region
			{
				Id = row.Id,
				M49Code = row.M49Code,
				Name = row.Name,
				Level = row.Level,
				ParentRegionId = row.ParentM49Code is null ? null : regionIdByCode[row.ParentM49Code],
			});
		}

		await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
		return regionIdByCode;
	}

	async Task SeedCountriesAsync(Dictionary<string, Guid> regionIdByCode, CancellationToken cancellationToken)
	{
		using ITabularReader reader = TabularReader.OpenDelimited(
			Path.Combine(AppContext.BaseDirectory, "seeds", "country-or-area.tsv"), '\t');
		var m49Ordinal = reader.Ordinal("M49Code");
		var alpha2Ordinal = reader.Ordinal("IsoAlpha2Code");
		var alpha3Ordinal = reader.Ordinal("IsoAlpha3Code");
		var nameOrdinal = reader.Ordinal("Name");
		var parentOrdinal = reader.Ordinal("ParentM49Code");
		var ldcOrdinal = reader.Ordinal("IsLeastDevelopedCountry");
		var lldcOrdinal = reader.Ordinal("IsLandLockedDevelopingCountry");
		var sidsOrdinal = reader.Ordinal("IsSmallIslandDevelopingState");

		List<(Guid Id, string M49Code, string Alpha2Code, string Alpha3Code, string Name, string? ParentM49Code, bool IsLeastDevelopedCountry, bool IsLandLockedDevelopingCountry, bool IsSmallIslandDevelopingState)> rows = [];

		while (reader.Read())
		{
			var m49Code = reader[m49Ordinal].ToString();
			var id = new DeterministicGuid(_namespaceCountryOrArea, m49Code);
			var parentCode = reader[parentOrdinal].ToString();

			rows.Add((
				id,
				m49Code,
				reader[alpha2Ordinal].ToString(),
				reader[alpha3Ordinal].ToString(),
				reader[nameOrdinal].ToString(),
				parentCode.Length == 0 ? null : parentCode,
				// The TSV's flag columns hold literal "true"/"false" (written by UnsdM49Writer's
				// FormatFlag), not the "x"/blank convention of the raw UNSD source CSV.
				bool.Parse(reader[ldcOrdinal]),
				bool.Parse(reader[lldcOrdinal]),
				bool.Parse(reader[sidsOrdinal])));
		}

		var existingIds = (await context.Set<CountryOrArea>().Select(c => c.Id).ToListAsync(cancellationToken).ConfigureAwait(false)).ToHashSet();

		foreach (var row in rows)
		{
			if (existingIds.Contains(row.Id))
				continue;

			context.Set<CountryOrArea>().Add(new CountryOrArea
			{
				Id = row.Id,
				M49Code = row.M49Code,
				IsoAlpha2Code = row.Alpha2Code,
				IsoAlpha3Code = row.Alpha3Code,
				Name = row.Name,
				ParentRegionId = row.ParentM49Code is null ? null : regionIdByCode[row.ParentM49Code],
				IsLeastDevelopedCountry = row.IsLeastDevelopedCountry,
				IsLandLockedDevelopingCountry = row.IsLandLockedDevelopingCountry,
				IsSmallIslandDevelopingState = row.IsSmallIslandDevelopingState,
			});
		}

		await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}
}
