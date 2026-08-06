using Microsoft.EntityFrameworkCore;

namespace Norse.Reference.Data.EntityFramework.Migrations.SqlServer.Tests;

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

	[Theory]
	[InlineData(typeof(Region))]
	[InlineData(typeof(CountryOrArea))]
	void CreateDbContext_realizes_the_temporal_stamp_as_engine_native_system_versioning(Type entityType)
	{
		// The realm's only proof that the realization hook reaches this context at all: ReferenceDbContext
		// inherits NorseDbContext, which reads the hook off its own options, so nothing here is wired by
		// hand. Postgres supplies no hook — it realizes temporality in migration SQL generation, never in
		// the model — so SQL Server is where a missing wire would show.
		ReferenceDbContextFactory factory = new();
		using var context = factory.CreateDbContext([]);

		context.Model.FindEntityType(entityType)!.IsTemporal().ShouldBeTrue();
	}

	[Fact]
	void CreateDbContext_forces_no_tracking()
	{
		ReferenceDbContextFactory factory = new();
		using var context = factory.CreateDbContext([]);

		context.ChangeTracker.QueryTrackingBehavior.ShouldBe(QueryTrackingBehavior.NoTracking);
	}
}
