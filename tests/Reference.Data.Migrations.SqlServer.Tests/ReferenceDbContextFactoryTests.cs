using Microsoft.EntityFrameworkCore;
using Norse.Reference.Data.EntityFramework;

namespace Norse.Reference.Data.Migrations.SqlServer.Tests;

public sealed class ReferenceDbContextFactoryTests
{
	[Fact]
	void CreateDbContext_keeps_engine_native_pascal_case()
	{
		ReferenceDbContextFactory factory = new();
		using var context = factory.CreateDbContext([]);

		var entityType = context.Model.FindEntityType(typeof(CountryOrArea));

		entityType.ShouldNotBeNull();
		entityType.GetTableName().ShouldBe("CountryOrArea");
		entityType.FindProperty(nameof(CountryOrArea.Alpha2))!.GetColumnName().ShouldBe("Alpha2");
	}

	[Fact]
	void CreateDbContext_stores_the_IsoCountryCode_conversion_as_a_SqlServer_int_column()
	{
		ReferenceDbContextFactory factory = new();
		using var context = factory.CreateDbContext([]);

		var entityType = context.Model.FindEntityType(typeof(CountryOrArea));

		entityType.ShouldNotBeNull();
		entityType.FindProperty(nameof(CountryOrArea.Code))!.GetColumnType().ShouldBe("int");
	}

	[Fact]
	void CreateDbContext_forces_no_tracking()
	{
		ReferenceDbContextFactory factory = new();
		using var context = factory.CreateDbContext([]);

		context.ChangeTracker.QueryTrackingBehavior.ShouldBe(QueryTrackingBehavior.NoTracking);
	}
}
