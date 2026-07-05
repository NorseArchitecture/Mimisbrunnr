using Microsoft.EntityFrameworkCore;
using Norse.EntityFramework;
using Norse.Primitives.Identifiers;
using Norse.ReferenceData.Data.Migrations;

namespace Norse.ReferenceData.Data.Tests;

[Collection("Postgres")]
public class ReferenceDataSeedContributorTests(PostgresContainerFixture fixture)
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
	public async Task SeedAsync_loads_248_countries_and_their_region_ancestors()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		using var context = await MigratedContextAsync(fixture.ConnectionString, cancellationToken);

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
	public async Task SeedAsync_is_idempotent_on_a_second_run()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		using var context = await MigratedContextAsync(fixture.ConnectionString, cancellationToken);
		var contributor = new ReferenceDataSeedContributor(context);

		try
		{
			await contributor.SeedAsync(cancellationToken);
			var firstRunCount = await context.Set<CountryOrArea>().CountAsync(cancellationToken);

			await contributor.SeedAsync(cancellationToken);
			var secondRunCount = await context.Set<CountryOrArea>().CountAsync(cancellationToken);

			secondRunCount.ShouldBe(firstRunCount);
		}
		finally
		{
			await context.Set<CountryOrArea>().ExecuteDeleteAsync(cancellationToken);
			await context.Set<Region>().ExecuteDeleteAsync(cancellationToken);
		}
	}

	[Fact]
	public async Task Reseeding_from_scratch_produces_byte_identical_ids()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		using var contextA = await MigratedContextAsync(fixture.ConnectionString, cancellationToken);

		try
		{
			await new ReferenceDataSeedContributor(contextA).SeedAsync(cancellationToken);
			var nigeriaIdFirstRun = await contextA.Set<CountryOrArea>().Where(c => c.M49Code == "566").Select(c => c.Id).SingleAsync(cancellationToken);

			nigeriaIdFirstRun.ShouldBe(new DeterministicGuid(
				new DeterministicGuid(DeterministicGuid.Namespaces.Dns, "country-or-area.m49.referencedata.norse"), "566"));
		}
		finally
		{
			await contextA.Set<CountryOrArea>().ExecuteDeleteAsync(cancellationToken);
			await contextA.Set<Region>().ExecuteDeleteAsync(cancellationToken);
		}
	}
}
