using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Norse.Abstractions.Backend;

namespace Norse.Reference.Data.Tests;

public sealed class ReferenceDbContextModelTests
{
	static ReferenceDbContext CreateContext()
	{
		var options = new DbContextOptionsBuilder<ReferenceDbContext>()
			.UseNpgsql("Host=localhost;Database=model-build-only")
			.Options;
		return new(options);
	}

	[Fact]
	void Model_configures_Region_with_unique_Code_index_and_self_referencing_FK()
	{
		using var context = CreateContext();
		IEntityType entityType = context.Model.FindEntityType(typeof(Region))!;

		entityType.ShouldNotBeNull();
		entityType.GetIndexes().Any(i => i.IsUnique && i.Properties.Single().Name == nameof(Region.Code)).ShouldBeTrue();
		entityType.GetForeignKeys().Single().PrincipalEntityType.ClrType.ShouldBe(typeof(Region));
	}

	[Fact]
	void Model_configures_CountryOrArea_with_three_unique_indexes_and_FK_to_Region()
	{
		using var context = CreateContext();
		IEntityType entityType = context.Model.FindEntityType(typeof(CountryOrArea))!;

		entityType.ShouldNotBeNull();
		entityType.GetIndexes().Count(i => i.IsUnique).ShouldBe(3);
		entityType.GetForeignKeys().Single().PrincipalEntityType.ClrType.ShouldBe(typeof(Region));
	}

	[Fact]
	void CountryOrArea_implements_IViewBearer_of_its_own_View()
	{
		typeof(IViewBearer<CountryOrAreaView>).IsAssignableFrom(typeof(CountryOrArea)).ShouldBeTrue();
	}

	[Fact]
	void Model_maps_CountryOrArea_Code_through_a_ushort_conversion_on_entity_and_JSON_view_member()
	{
		// HasConversion<ushort>() pins the intermediate provider value, but Npgsql carries no native
		// unsigned-integer store type — the type-mapping source folds the final provider CLR type down
		// to int, matching the "integer"/"int" column the migrations actually scaffold on both
		// providers. This asserts that resolved provider type, not the intermediate ushort hop.
		using var context = CreateContext();
		IEntityType entityType = context.Model.FindEntityType(typeof(CountryOrArea))!;

		var entityCodeProperty = entityType.FindProperty(nameof(CountryOrArea.Code))!;
		entityCodeProperty.GetTypeMapping().Converter!.ProviderClrType.ShouldBe(typeof(int));

		var viewEntityType = entityType.FindNavigation(nameof(CountryOrArea.View))!.TargetEntityType;
		var viewCodeProperty = viewEntityType.FindProperty(nameof(CountryOrAreaView.Code))!;
		viewCodeProperty.GetTypeMapping().Converter!.ProviderClrType.ShouldBe(typeof(int));
	}
}
