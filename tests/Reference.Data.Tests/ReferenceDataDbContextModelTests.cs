using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Norse.Reference.Data.Tests;

public class ReferenceDataDbContextModelTests
{
	static ReferenceDataDbContext CreateContext()
	{
		var options = new DbContextOptionsBuilder<ReferenceDataDbContext>()
			.UseNpgsql("Host=localhost;Database=model-build-only")
			.Options;
		return new ReferenceDataDbContext(options);
	}

	[Fact]
	public void Model_configures_Region_with_unique_M49Code_index_and_self_referencing_FK()
	{
		using var context = CreateContext();
		IEntityType entityType = context.Model.FindEntityType(typeof(Region))!;

		entityType.ShouldNotBeNull();
		entityType.GetIndexes().Any(i => i.IsUnique && i.Properties.Single().Name == nameof(Region.M49Code)).ShouldBeTrue();
		entityType.GetForeignKeys().Single().PrincipalEntityType.ClrType.ShouldBe(typeof(Region));
	}

	[Fact]
	public void Model_configures_CountryOrArea_with_three_unique_indexes_and_FK_to_Region()
	{
		using var context = CreateContext();
		IEntityType entityType = context.Model.FindEntityType(typeof(CountryOrArea))!;

		entityType.ShouldNotBeNull();
		entityType.GetIndexes().Count(i => i.IsUnique).ShouldBe(3);
		entityType.GetForeignKeys().Single().PrincipalEntityType.ClrType.ShouldBe(typeof(Region));
	}
}
