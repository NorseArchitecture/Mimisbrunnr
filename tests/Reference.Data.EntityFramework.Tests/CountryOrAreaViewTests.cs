using Microsoft.EntityFrameworkCore;
using Norse.Persistence.EntityFramework;
using Norse.Persistence.EntityFramework.PostgreSQL;
using Norse.Primitives.Identifiers;
using Norse.Reference.Data.EntityFramework.Migrations;
using Norse.Reference.Data.EntityFramework.Migrations.PostgreSQL;

namespace Norse.Reference.Data.EntityFramework.Tests;

[Collection("Postgres")]
public sealed class CountryOrAreaViewTests(PostgresContainerFixture fixture)
{
	static async Task<ReferenceDbContext> MigratedContextAsync(string connectionString, CancellationToken cancellationToken)
	{
		DbContextOptionsBuilder<ReferenceDbContext> optionsBuilder = new();
		optionsBuilder.ApplyNorseProviderOptions(NorsePostgresEfProvider.Instance,
			connectionString, typeof(ReferenceDbContextFactory).Assembly.GetName().Name);
		ReferenceDbContext context = new(optionsBuilder.Options);
		await new NorseReferenceMigrationContributor(context).MigrateAsync(cancellationToken);
		return context;
	}

	[Fact]
	async Task View_round_trips_all_three_levels_for_Nigeria_shape()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		await using var context = await MigratedContextAsync(fixture.ConnectionString, cancellationToken);
		DeterministicGuid
			countryId = new(DeterministicGuid.Namespaces.Dns, "NG"),
			regionId = new(DeterministicGuid.Namespaces.Dns, "002"),
			subregionId = new(DeterministicGuid.Namespaces.Dns, "202"),
			intermediateRegionId = new(DeterministicGuid.Namespaces.Dns, "011");
		var set = context.Set<CountryOrArea>();
		try
		{
			set.Add(new()
			{
				Id = countryId,
				Code = IsoCountryCode.Nigeria,
				Alpha2 = "NG",
				Alpha3 = "NGA",
				Name = "Nigeria",
				Classification = Classification.None,
				View = new()
				{
					Id = countryId,
					Code = IsoCountryCode.Nigeria,
					Alpha2 = "NG",
					Alpha3 = "NGA",
					Name = "Nigeria",
					Classification = Classification.None,
					Region = new()
					{
						Id = regionId,
						Code = "002",
						Name = "Africa",
						Subregion = new()
						{
							Id = subregionId,
							Code = "202",
							Name = "Sub-Saharan Africa",
							IntermediateRegion = new()
							{
								Id = intermediateRegionId,
								Code = "011",
								Name = "Western Africa"
							}
						}
					}
				}
			});
			await context.SaveChangesAsync(cancellationToken);
			context.ChangeTracker.Clear();

			var reread = await set.Where(c => c.Id == countryId).Select(c => c.View).SingleAsync(cancellationToken);

			reread.ShouldNotBeNull();
			reread.Id.ShouldBe(countryId);
			reread.Alpha2.ShouldBe("NG");
			reread.Region.ShouldNotBeNull();
			reread.Region.Id.ShouldBe(regionId);
			reread.Region.Code.ShouldBe("002");
			reread.Region.Subregion.ShouldNotBeNull();
			reread.Region.Subregion.Id.ShouldBe(subregionId);
			reread.Region.Subregion.IntermediateRegion.ShouldNotBeNull();
			reread.Region.Subregion.IntermediateRegion.Id.ShouldBe(intermediateRegionId);
			reread.Region.Subregion.IntermediateRegion.Code.ShouldBe("011");
		}
		finally
		{
			await set.Where(c => c.Id == countryId).ExecuteDeleteAsync(cancellationToken);
		}
	}

	[Fact]
	async Task View_has_null_intermediate_region_for_Algeria_shape()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		await using var context = await MigratedContextAsync(fixture.ConnectionString, cancellationToken);
		DeterministicGuid
			countryId = new(DeterministicGuid.Namespaces.Dns, "DZ"),
			regionId = new(DeterministicGuid.Namespaces.Dns, "002"),
			subregionId = new(DeterministicGuid.Namespaces.Dns, "015");
		var set = context.Set<CountryOrArea>();
		try
		{
			set.Add(new()
			{
				Id = countryId,
				Code = IsoCountryCode.Algeria,
				Alpha2 = "DZ",
				Alpha3 = "DZA",
				Name = "Algeria",
				Classification = Classification.None,
				View = new()
				{
					Id = countryId,
					Code = IsoCountryCode.Algeria,
					Alpha2 = "DZ",
					Alpha3 = "DZA",
					Name = "Algeria",
					Classification = Classification.None,
					Region = new()
					{
						Id = regionId,
						Code = "002",
						Name = "Africa",
						Subregion = new() { Id = subregionId, Code = "015", Name = "Northern Africa" }
					}
				}
			});
			await context.SaveChangesAsync(cancellationToken);
			context.ChangeTracker.Clear();

			var reread = await set.Where(c => c.Id == countryId).Select(c => c.View).SingleAsync(cancellationToken);

			reread.ShouldNotBeNull();
			reread.Region.ShouldNotBeNull();
			reread.Region.Subregion.ShouldNotBeNull();
			reread.Region.Subregion.IntermediateRegion.ShouldBeNull();
		}
		finally
		{
			await set.Where(c => c.Id == countryId).ExecuteDeleteAsync(cancellationToken);
		}
	}

	[Fact]
	async Task View_has_null_region_for_Antarctica_shape()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		await using var context = await MigratedContextAsync(fixture.ConnectionString, cancellationToken);
		DeterministicGuid countryId = new(DeterministicGuid.Namespaces.Dns, "AQ");
		var set = context.Set<CountryOrArea>();
		try
		{
			set.Add(new()
			{
				Id = countryId,
				Code = IsoCountryCode.Antarctica,
				Alpha2 = "AQ",
				Alpha3 = "ATA",
				Name = "Antarctica",
				Classification = Classification.None,
				View = new()
				{
					Id = countryId,
					Code = IsoCountryCode.Antarctica,
					Alpha2 = "AQ",
					Alpha3 = "ATA",
					Name = "Antarctica",
					Classification = Classification.None
				}
			});
			await context.SaveChangesAsync(cancellationToken);
			context.ChangeTracker.Clear();

			var reread = await set.Where(c => c.Id == countryId).Select(c => c.View).SingleAsync(cancellationToken);

			reread.ShouldNotBeNull();
			reread.Id.ShouldBe(countryId);
			reread.Alpha2.ShouldBe("AQ");
			reread.Region.ShouldBeNull();
		}
		finally
		{
			await set.Where(c => c.Id == countryId).ExecuteDeleteAsync(cancellationToken);
		}
	}
}
