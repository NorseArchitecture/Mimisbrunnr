using Microsoft.EntityFrameworkCore;
using Norse.EntityFramework;
using Norse.ReferenceData.Data.Migrations;

namespace Norse.ReferenceData.Data.Tests;

[Collection("Postgres")]
public class CountryOrAreaDossierTests(PostgresContainerFixture fixture)
{
	static async Task<ReferenceDataDbContext> MigratedContextAsync(string connectionString, CancellationToken cancellationToken)
	{
		var optionsBuilder = new DbContextOptionsBuilder<ReferenceDataDbContext>()
			.UseNpgsql(connectionString,
				o => o.MigrationsAssembly(typeof(NorseReferenceDataMigrationContributor).Assembly.GetName().Name));
		NorseDbContextOptionsExtensions.ApplyNorseConventions(optionsBuilder);
		var context = new ReferenceDataDbContext(optionsBuilder.Options);
		await new NorseReferenceDataMigrationContributor(context).MigrateAsync(cancellationToken).ConfigureAwait(false);
		return context;
	}

	[Fact]
	public async Task Dossier_nests_region_subregion_and_intermediate_region_for_Nigeria()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		using var context = await MigratedContextAsync(fixture.ConnectionString, cancellationToken);

		try
		{
			var africa = new Region { Id = Guid.NewGuid(), M49Code = "002", Name = "Africa", Level = RegionLevel.Region };
			var subSaharan = new Region { Id = Guid.NewGuid(), M49Code = "202", Name = "Sub-Saharan Africa", Level = RegionLevel.Subregion, ParentRegionId = africa.Id };
			var westernAfrica = new Region { Id = Guid.NewGuid(), M49Code = "011", Name = "Western Africa", Level = RegionLevel.IntermediateRegion, ParentRegionId = subSaharan.Id };
			context.Set<Region>().AddRange(africa, subSaharan, westernAfrica);
			context.Set<CountryOrArea>().Add(new CountryOrArea
			{
				Id = Guid.NewGuid(),
				M49Code = "566",
				IsoAlpha2Code = "NG",
				IsoAlpha3Code = "NGA",
				Name = "Nigeria",
				ParentRegionId = westernAfrica.Id,
			});
			await context.SaveChangesAsync(cancellationToken);

			var dossier = await context.GetCountryOrAreaDossierAsync("566", cancellationToken);

			dossier.ShouldNotBeNull();
			dossier.Region.ShouldNotBeNull();
			dossier.Region.Subregion.ShouldNotBeNull();
			dossier.Region.Subregion.IntermediateRegion.ShouldNotBeNull();
			dossier.Region.Subregion.IntermediateRegion.Code.ShouldBe("011");
		}
		finally
		{
			// The shared Postgres container is never truncated between tests, so this test's own
			// rows must be deleted here — otherwise they leak into every later test in this collection
			// (and, in Task 5, collide with the real UN M49 seed data's row count and unique codes).
			await context.Set<CountryOrArea>().Where(c => c.M49Code == "566").ExecuteDeleteAsync(cancellationToken);
			await context.Set<Region>().Where(r => r.M49Code == "002" || r.M49Code == "202" || r.M49Code == "011").ExecuteDeleteAsync(cancellationToken);
		}
	}

	[Fact]
	public async Task Dossier_has_null_intermediate_region_for_Algeria()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		using var context = await MigratedContextAsync(fixture.ConnectionString, cancellationToken);

		try
		{
			// A second, distinct "Africa" row (not "002") — the fixture's Postgres container is shared across
			// this whole test collection, and uq_regions_m49_code is a real unique index, so reusing the
			// Nigeria test's M49Code would collide. Only the M49Code differs; it is not asserted below.
			var africa = new Region { Id = Guid.NewGuid(), M49Code = "902", Name = "Africa", Level = RegionLevel.Region };
			var northernAfrica = new Region { Id = Guid.NewGuid(), M49Code = "015", Name = "Northern Africa", Level = RegionLevel.Subregion, ParentRegionId = africa.Id };
			context.Set<Region>().AddRange(africa, northernAfrica);
			context.Set<CountryOrArea>().Add(new CountryOrArea
			{
				Id = Guid.NewGuid(),
				M49Code = "012",
				IsoAlpha2Code = "DZ",
				IsoAlpha3Code = "DZA",
				Name = "Algeria",
				ParentRegionId = northernAfrica.Id,
			});
			await context.SaveChangesAsync(cancellationToken);

			var dossier = await context.GetCountryOrAreaDossierAsync("012", cancellationToken);

			dossier.ShouldNotBeNull();
			dossier.Region.ShouldNotBeNull();
			dossier.Region.Subregion.ShouldNotBeNull();
			dossier.Region.Subregion.IntermediateRegion.ShouldBeNull();
		}
		finally
		{
			// See the Nigeria test above: the shared container is never reset between tests, so
			// this test's own rows must be removed here to avoid leaking into later tests/tasks.
			await context.Set<CountryOrArea>().Where(c => c.M49Code == "012").ExecuteDeleteAsync(cancellationToken);
			await context.Set<Region>().Where(r => r.M49Code == "902" || r.M49Code == "015").ExecuteDeleteAsync(cancellationToken);
		}
	}

	[Fact]
	public async Task Dossier_has_null_region_for_Antarctica()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		using var context = await MigratedContextAsync(fixture.ConnectionString, cancellationToken);

		try
		{
			context.Set<CountryOrArea>().Add(new CountryOrArea
			{
				Id = Guid.NewGuid(),
				M49Code = "010",
				IsoAlpha2Code = "AQ",
				IsoAlpha3Code = "ATA",
				Name = "Antarctica",
				ParentRegionId = null,
			});
			await context.SaveChangesAsync(cancellationToken);

			var dossier = await context.GetCountryOrAreaDossierAsync("010", cancellationToken);

			dossier.ShouldNotBeNull();
			dossier.Region.ShouldBeNull();
		}
		finally
		{
			// See the Nigeria test above: the shared container is never reset between tests, so
			// this test's own row must be removed here to avoid leaking into later tests/tasks.
			await context.Set<CountryOrArea>().Where(c => c.M49Code == "010").ExecuteDeleteAsync(cancellationToken);
		}
	}
}
