using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Norse.Abstractions.Migrations.Seeding;
using Norse.Primitives.Identifiers;
using Norse.Primitives.Ingestion;

namespace Norse.Reference.Data.Migrations;

/// <summary>
/// Seeds <see cref="Region"/> and <see cref="CountryOrArea"/> rows from the committed UN M49 TSVs
/// (<c>seeds/region.tsv</c>, <c>seeds/country-or-area.tsv</c>), idempotently, and hydrates each
/// <see cref="CountryOrArea.View"/> from the same region rows.
/// </summary>
/// <param name="context">The reference-data context instance resolved from DI.</param>
public sealed class ReferenceDataSeedContributor(ReferenceDataDbContext context) : ISeedContributor
{
	static readonly Guid _namespaceRegion =
		new DeterministicGuid(DeterministicGuid.Namespaces.Dns, "region.m49.referencedata.norse");

	static readonly Guid _namespaceCountryOrArea =
		new DeterministicGuid(DeterministicGuid.Namespaces.Dns, "country-or-area.m49.referencedata.norse");

	/// <inheritdoc />
	public string Name => "Norse.Reference";

	/// <inheritdoc />
	public async Task SeedAsync(CancellationToken cancellationToken)
	{
		var regionsByCode = await SeedRegionsAsync(cancellationToken).ConfigureAwait(false);
		await SeedCountriesAsync(regionsByCode, cancellationToken).ConfigureAwait(false);
	}

	async Task<Dictionary<string, RegionRow>> SeedRegionsAsync(CancellationToken cancellationToken)
	{
		Dictionary<string, RegionRow> regionsByCode = [];

		using var reader = TabularReader.OpenDelimited(
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

			regionsByCode[m49Code] = new RegionRow(id, m49Code, reader[nameOrdinal].ToString(), level, parentCode.Length == 0 ? null : parentCode);
		}

		var existingIds = (await context.Set<Region>().Select(r => r.Id).ToListAsync(cancellationToken).ConfigureAwait(false)).ToHashSet();

		foreach (var row in regionsByCode.Values)
		{
			if (existingIds.Contains(row.Id))
				continue;

			context.Set<Region>().Add(new Region
			{
				Id = row.Id,
				Code = ushort.Parse(row.M49Code, CultureInfo.InvariantCulture),
				Name = row.Name,
				Level = row.Level,
				ParentRegionId = row.ParentM49Code is null ? null : regionsByCode[row.ParentM49Code].Id,
			});
		}

		await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
		return regionsByCode;
	}

	async Task SeedCountriesAsync(Dictionary<string, RegionRow> regionsByCode, CancellationToken cancellationToken)
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
				Code = ushort.Parse(row.M49Code, CultureInfo.InvariantCulture),
				Alpha2 = row.Alpha2Code,
				Alpha3 = row.Alpha3Code,
				Name = row.Name,
				ParentRegionId = row.ParentM49Code is null ? null : regionsByCode[row.ParentM49Code].Id,
				View = BuildView(row.ParentM49Code, regionsByCode),
				Classification = BuildClassification(row.IsLeastDevelopedCountry, row.IsLandLockedDevelopingCountry, row.IsSmallIslandDevelopingState),
			});
		}

		await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Walks <paramref name="leafCode"/> up through <paramref name="regionsByCode"/> via each row's own
	/// <c>ParentM49Code</c>, then re-nests the chain from the root down (Region contains Subregion
	/// contains IntermediateRegion), classifying each ancestor by its own <see cref="RegionLevel"/>
	/// rather than assuming a fixed position — a country's direct parent may be a Subregion or an
	/// IntermediateRegion, never a bare positional offset.
	/// </summary>
	static RegionNode? BuildView(string? leafCode, Dictionary<string, RegionRow> regionsByCode)
	{
		if (leafCode is null)
			return null;

		List<RegionRow> chain = [];
		for (var code = leafCode; code is not null; code = regionsByCode[code].ParentM49Code)
			chain.Add(regionsByCode[code]);

		var intermediateRow = chain.SingleOrDefault(r => r.Level == RegionLevel.IntermediateRegion);
		var subregionRow = chain.SingleOrDefault(r => r.Level == RegionLevel.Subregion);
		var regionRow = chain.Single(r => r.Level == RegionLevel.Region);

		var intermediate = intermediateRow is null
			? null
			: new IntermediateRegionNode { Code = intermediateRow.M49Code, Name = intermediateRow.Name };

		var subregion = subregionRow is null
			? null
			: new SubregionNode { Code = subregionRow.M49Code, Name = subregionRow.Name, IntermediateRegion = intermediate };

		return new RegionNode { Code = regionRow.M49Code, Name = regionRow.Name, Subregion = subregion };
	}

	static Classification BuildClassification(bool isLeastDevelopedCountry, bool isLandLockedDevelopingCountry, bool isSmallIslandDevelopingState) =>
		(isLeastDevelopedCountry ? Classification.LeastDevelopedCountry : Classification.None)
		| (isLandLockedDevelopingCountry ? Classification.LandLockedDevelopingCountry : Classification.None)
		| (isSmallIslandDevelopingState ? Classification.SmallIslandDevelopingState : Classification.None);

	sealed record RegionRow(Guid Id, string M49Code, string Name, RegionLevel Level, string? ParentM49Code);
}
