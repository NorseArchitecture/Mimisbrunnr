using Microsoft.EntityFrameworkCore;
using Norse.Persistence.EntityFramework;
using Norse.Reference.Data.Migrations;
using Norse.Reference.Data.Migrations.PostgreSQL;

namespace Norse.Reference.Data.Tests;

[Collection("Postgres")]
public class NorseReferenceDataMigrationContributorTests(PostgresContainerFixture fixture)
{
	[Fact]
	public async Task MigrateAsync_creates_regions_and_country_or_areas_tables()
	{
		var optionsBuilder = new DbContextOptionsBuilder<ReferenceDataDbContext>()
			.UseNpgsql(fixture.ConnectionString,
				o => o.MigrationsAssembly(typeof(ReferenceDataDbContextFactory).Assembly.GetName().Name));
		NorseDbContextOptionsExtensions.ApplyNorseConventions(optionsBuilder);
		var options = optionsBuilder.Options;
		using var context = new ReferenceDataDbContext(options);
		var contributor = new NorseReferenceDataMigrationContributor(context);

		await contributor.MigrateAsync(TestContext.Current.CancellationToken);

		(await context.Database.GetAppliedMigrationsAsync(TestContext.Current.CancellationToken))
			.ShouldContain(m => m.Contains("InitialCreate", StringComparison.Ordinal));
	}
}
