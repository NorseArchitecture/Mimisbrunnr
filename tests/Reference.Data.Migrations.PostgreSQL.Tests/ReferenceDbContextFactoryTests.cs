using Microsoft.EntityFrameworkCore;

namespace Norse.Reference.Data.Migrations.PostgreSQL.Tests;

public sealed class ReferenceDbContextFactoryTests
{
	[Fact]
	void CreateDbContext_applies_snake_case_naming()
	{
		ReferenceDbContextFactory factory = new();
		using var context = factory.CreateDbContext([]);

		var entityType = context.Model.FindEntityType(typeof(CountryOrArea));

		entityType.ShouldNotBeNull();
		entityType.GetTableName().ShouldBe("country_or_area");
		entityType.FindProperty(nameof(CountryOrArea.Alpha2))!.GetColumnName().ShouldBe("alpha2");
	}

	[Fact]
	void CreateDbContext_stores_the_IsoCountryCode_conversion_as_a_Postgres_integer_column()
	{
		ReferenceDbContextFactory factory = new();
		using var context = factory.CreateDbContext([]);

		var entityType = context.Model.FindEntityType(typeof(CountryOrArea));

		entityType.ShouldNotBeNull();
		entityType.FindProperty(nameof(CountryOrArea.Code))!.GetColumnType().ShouldBe("integer");
	}

	[Fact]
	void CreateDbContext_forces_no_tracking()
	{
		ReferenceDbContextFactory factory = new();
		using var context = factory.CreateDbContext([]);

		context.ChangeTracker.QueryTrackingBehavior.ShouldBe(QueryTrackingBehavior.NoTracking);
	}
}
