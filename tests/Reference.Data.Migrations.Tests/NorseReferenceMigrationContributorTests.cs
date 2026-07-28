using Microsoft.EntityFrameworkCore;
using Norse.Persistence.EntityFramework;
using Norse.Persistence.EntityFramework.PostgreSQL;
using Norse.Reference.Data.Migrations.PostgreSQL;

namespace Norse.Reference.Data.Migrations.Tests;

[Collection("Postgres")]
public sealed class NorseReferenceMigrationContributorTests(PostgresContainerFixture fixture)
{
	[Fact]
	async Task MigrateAsync_creates_regions_and_country_or_areas_tables()
	{
		DbContextOptionsBuilder<ReferenceDbContext> optionsBuilder = new();
		optionsBuilder.ApplyNorseProviderOptions(NorsePostgresEfProvider.Instance,
			fixture.ConnectionString, typeof(ReferenceDbContextFactory).Assembly.GetName().Name);
		using ReferenceDbContext context = new(optionsBuilder.Options);
		NorseReferenceMigrationContributor contributor = new(context);

		await contributor.MigrateAsync(TestContext.Current.CancellationToken);

		(await context.Database.GetAppliedMigrationsAsync(TestContext.Current.CancellationToken))
			.ShouldContain(m => m.Contains("InitialCreate", StringComparison.Ordinal));
	}
}
