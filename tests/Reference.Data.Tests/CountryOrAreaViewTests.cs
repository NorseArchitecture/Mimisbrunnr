using Microsoft.EntityFrameworkCore;
using Norse.Persistence.EntityFramework;
using Norse.Reference.Data.Migrations;
using Norse.Reference.Data.Migrations.PostgreSQL;

namespace Norse.Reference.Data.Tests;

[Collection("Postgres")]
public class CountryOrAreaViewTests(PostgresContainerFixture fixture)
{
	static async Task<ReferenceDbContext> MigratedContextAsync(string connectionString, CancellationToken cancellationToken)
	{
		var optionsBuilder = new DbContextOptionsBuilder<ReferenceDbContext>()
			.UseNpgsql(connectionString, o =>
				o.MigrationsAssembly(typeof(ReferenceDbContextFactory).Assembly.GetName().Name));
		optionsBuilder.ApplyNorseConventions();
		ReferenceDbContext context = new(optionsBuilder.Options);
		await new NorseReferenceMigrationContributor(context).MigrateAsync(cancellationToken).ConfigureAwait(false);
		return context;
	}

	[Fact]
	public async Task View_round_trips_all_three_levels_for_Nigeria_shape()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		using var context = await MigratedContextAsync(fixture.ConnectionString, cancellationToken);
		var countryId = Guid.NewGuid();
		var set = context.Set<CountryOrArea>();
		try
		{
			set.Add(new()
			{
				Id = countryId,
				Code = 566,
				Alpha2 = "NG",
				Alpha3 = "NGA",
				Name = "Nigeria",
				View = new()
				{
					Code = "002",
					Name = "Africa",
					Subregion = new()
					{
						Code = "202",
						Name = "Sub-Saharan Africa",
						IntermediateRegion = new() { Code = "011", Name = "Western Africa" }
					}
				}
			});
			await context.SaveChangesAsync(cancellationToken);
			context.ChangeTracker.Clear();

			var reread = await set.SingleAsync(c => c.Id == countryId, cancellationToken);

			reread.View.ShouldNotBeNull();
			reread.View.Code.ShouldBe("002");
			reread.View.Subregion.ShouldNotBeNull();
			reread.View.Subregion.IntermediateRegion.ShouldNotBeNull();
			reread.View.Subregion.IntermediateRegion.Code.ShouldBe("011");
		}
		finally
		{
			await set.Where(c => c.Id == countryId).ExecuteDeleteAsync(cancellationToken);
		}
	}

	[Fact]
	public async Task View_has_null_intermediate_region_for_Algeria_shape()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		using var context = await MigratedContextAsync(fixture.ConnectionString, cancellationToken);
		var countryId = Guid.NewGuid();
		var set = context.Set<CountryOrArea>();
		try
		{
			set.Add(new()
			{
				Id = countryId,
				Code = 12,
				Alpha2 = "DZ",
				Alpha3 = "DZA",
				Name = "Algeria",
				View = new()
				{
					Code = "002",
					Name = "Africa",
					Subregion = new() { Code = "015", Name = "Northern Africa", IntermediateRegion = null },
				},
			});
			await context.SaveChangesAsync(cancellationToken);
			context.ChangeTracker.Clear();

			var reread = await set.SingleAsync(c => c.Id == countryId, cancellationToken);

			reread.View.ShouldNotBeNull();
			reread.View.Subregion.ShouldNotBeNull();
			reread.View.Subregion.IntermediateRegion.ShouldBeNull();
		}
		finally
		{
			await set.Where(c => c.Id == countryId).ExecuteDeleteAsync(cancellationToken);
		}
	}

	[Fact]
	public async Task View_is_null_for_Antarctica_shape()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		using var context = await MigratedContextAsync(fixture.ConnectionString, cancellationToken);
		var countryId = Guid.NewGuid();
		var set = context.Set<CountryOrArea>();
		try
		{
			set.Add(new()
			{
				Id = countryId,
				Code = 10,
				Alpha2 = "AQ",
				Alpha3 = "ATA",
				Name = "Antarctica",
				View = null,
			});
			await context.SaveChangesAsync(cancellationToken);
			context.ChangeTracker.Clear();

			var reread = await set.SingleAsync(c => c.Id == countryId, cancellationToken);

			reread.View.ShouldBeNull();
		}
		finally
		{
			await set.Where(c => c.Id == countryId).ExecuteDeleteAsync(cancellationToken);
		}
	}
}
