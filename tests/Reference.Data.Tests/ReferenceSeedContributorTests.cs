using Microsoft.EntityFrameworkCore;
using Norse.Persistence.EntityFramework;
using Norse.Primitives.Identifiers;
using Norse.Reference.Data.Migrations;
using Norse.Reference.Data.Migrations.PostgreSQL;

namespace Norse.Reference.Data.Tests;

[Collection("Postgres")]
public sealed class ReferenceSeedContributorTests(PostgresContainerFixture fixture)
{
	static async Task<ReferenceDbContext> MigratedContextAsync(string connectionString,
		CancellationToken cancellationToken)
	{
		var optionsBuilder = new DbContextOptionsBuilder<ReferenceDbContext>()
			.UseNpgsql(connectionString, o =>
				o.MigrationsAssembly(typeof(ReferenceDbContextFactory).Assembly.GetName().Name));
		optionsBuilder
			.ApplyNorseConventions()
			.ApplyNorseTrackingBehavior();
		ReferenceDbContext context = new(optionsBuilder.Options);
		await new NorseReferenceMigrationContributor(context).MigrateAsync(cancellationToken);
		return context;
	}

	[Fact]
	async Task SeedAsync_loads_248_countries_and_their_region_ancestors()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		await using var context = await MigratedContextAsync(fixture.ConnectionString, cancellationToken);

		try
		{
			await new ReferenceDataSeedContributor(context).SeedAsync(cancellationToken);

			(await context.Set<CountryOrArea>().CountAsync(cancellationToken)).ShouldBe(248);
			(await context.Set<Region>().CountAsync(cancellationToken)).ShouldBeGreaterThan(0);
		}
		finally
		{
			// The shared Postgres container is never truncated between tests (Task 4's lesson) — and
			// this contributor loads the entire real UN M49 dataset, not a handful of hand-picked
			// codes, so an unconditional full-table clear is the correct cleanup, not a filtered one.
			await context.Set<CountryOrArea>().ExecuteDeleteAsync(cancellationToken);
			await context.Set<Region>().ExecuteDeleteAsync(cancellationToken);
		}
	}

	[Fact]
	async Task SeedAsync_is_idempotent_on_a_second_run()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		await using var context = await MigratedContextAsync(fixture.ConnectionString, cancellationToken);
		ReferenceDataSeedContributor contributor = new(context);

		var set = context.Set<CountryOrArea>();
		try
		{
			await contributor.SeedAsync(cancellationToken);
			var firstRunCount = await set.CountAsync(cancellationToken);

			await contributor.SeedAsync(cancellationToken);
			var secondRunCount = await set.CountAsync(cancellationToken);

			secondRunCount.ShouldBe(firstRunCount);
		}
		finally
		{
			await set.ExecuteDeleteAsync(cancellationToken);
			await context.Set<Region>().ExecuteDeleteAsync(cancellationToken);
		}
	}

	[Fact]
	async Task Reseeding_from_scratch_produces_byte_identical_ids()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		await using var contextA = await MigratedContextAsync(fixture.ConnectionString, cancellationToken);
		var set = contextA.Set<CountryOrArea>();
		try
		{
			await new ReferenceDataSeedContributor(contextA).SeedAsync(cancellationToken);
			var nigeriaIdFirstRun =
				await set.Where(c => c.Code == 566).Select(c => c.Id).SingleAsync(cancellationToken);

			nigeriaIdFirstRun.ShouldBe(new(
				new DeterministicGuid(DeterministicGuid.Namespaces.Dns, "country-or-area.m49.referencedata.norse"),
				"566"));
		}
		finally
		{
			await set.ExecuteDeleteAsync(cancellationToken);
			await contextA.Set<Region>().ExecuteDeleteAsync(cancellationToken);
		}
	}

	[Fact]
	async Task SeedAsync_hydrates_View_for_all_three_verified_shapes()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		await using var context = await MigratedContextAsync(fixture.ConnectionString, cancellationToken);
		var set = context.Set<CountryOrArea>();
		try
		{
			await new ReferenceDataSeedContributor(context).SeedAsync(cancellationToken);
			context.ChangeTracker.Clear();

			var nigeria = await set.Where(c => c.Code == 566).Select(c => c.View).SingleAsync(cancellationToken);
			nigeria.ShouldNotBeNull();
			nigeria.Id.ShouldBe(nigeria.Id);
			nigeria.Alpha2.ShouldBe("NG");
			nigeria.Region.ShouldNotBeNull();
			nigeria.Region.Code.ShouldBe("002");
			nigeria.Region.Subregion.ShouldNotBeNull();
			nigeria.Region.Subregion.IntermediateRegion.ShouldNotBeNull();
			nigeria.Region.Subregion.IntermediateRegion.Code.ShouldBe("011");

			var algeria = await set.Where(c => c.Code == 12).Select(c => c.View).SingleAsync(cancellationToken);
			algeria.ShouldNotBeNull();
			algeria.Region.ShouldNotBeNull();
			algeria.Region.Subregion.ShouldNotBeNull();
			algeria.Region.Subregion.IntermediateRegion.ShouldBeNull();

			var antarctica = await set.Where(c => c.Code == 10).Select(c => c.View).SingleAsync(cancellationToken);
			antarctica.ShouldNotBeNull();
			antarctica.Id.ShouldBe(antarctica.Id);
			antarctica.Alpha2.ShouldBe("AQ");
			antarctica.Region.ShouldBeNull();
		}
		finally
		{
			await set.ExecuteDeleteAsync(cancellationToken);
			await context.Set<Region>().ExecuteDeleteAsync(cancellationToken);
		}
	}

	[Fact]
	async Task SeedAsync_hydrates_matching_Region_ids_at_every_View_level()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		await using var context = await MigratedContextAsync(fixture.ConnectionString, cancellationToken);
		var countrySet = context.Set<CountryOrArea>();
		var regionSet = context.Set<Region>();
		try
		{
			await new ReferenceDataSeedContributor(context).SeedAsync(cancellationToken);
			context.ChangeTracker.Clear();

			var nigeria = await countrySet.Where(c => c.Code == 566).Select(c => c.View).SingleAsync(cancellationToken);
			nigeria.Region.ShouldNotBeNull();
			nigeria.Region.Subregion.ShouldNotBeNull();
			nigeria.Region.Subregion.IntermediateRegion.ShouldNotBeNull();

			var africaId = await regionSet.Where(r => r.Code == 2).Select(r => r.Id).SingleAsync(cancellationToken);
			var subSaharanAfricaId =
				await regionSet.Where(r => r.Code == 202).Select(r => r.Id).SingleAsync(cancellationToken);
			var westernAfricaId =
				await regionSet.Where(r => r.Code == 11).Select(r => r.Id).SingleAsync(cancellationToken);

			nigeria.Region.Id.ShouldBe(africaId);
			nigeria.Region.Subregion.Id.ShouldBe(subSaharanAfricaId);
			nigeria.Region.Subregion.IntermediateRegion.Id.ShouldBe(westernAfricaId);
		}
		finally
		{
			await countrySet.ExecuteDeleteAsync(cancellationToken);
			await regionSet.ExecuteDeleteAsync(cancellationToken);
		}
	}
}
