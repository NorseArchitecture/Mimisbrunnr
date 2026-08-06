using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Norse.Persistence.EntityFramework;
using Norse.Persistence.EntityFramework.PostgreSQL;

namespace Norse.Reference.Data.EntityFramework.Tests;

/// <summary>
/// Which reference tables are system-versioned, pinned from both sides. Ruled 2026-08-05: both root
/// tables go temporal — ISO/UN canon is static data that changes rarely, and the record of when it
/// changed is exactly what system-time history is for. The owned <see cref="CountryOrAreaView"/> jsonb
/// document graph takes no marker: owned and JSON-mapped types are outside the temporal contract by
/// chassis validation, and the view column's contents ride the owner's history like any other column.
/// The scope is a ruling, not an implementation detail — adding or dropping a marker without amending
/// it breaks these facts by design.
/// </summary>
public sealed class ReferenceTemporalModelTests
{
	// Built through the provider binding rather than a bare UseNpgsql, so the snake_case rewriter is in
	// play and the table names asserted below are the ones the apparatus actually derives from.
	static readonly Lazy<IModel> _model = new(() =>
	{
		DbContextOptionsBuilder<ReferenceDbContext> builder = new();
		builder.ApplyNorseProviderOptions(NorsePostgresEfProvider.Instance,
			NorsePostgresEfProvider.Instance.DesignTimePlaceholderConnectionString("norse_reference"), null);
		using ReferenceDbContext context = new(builder.Options);
		return context.Model;
	});

	static IModel Model => _model.Value;

	/// <summary>The two ruled temporal entities — the realm's root tables, and all of them.</summary>
	static readonly Type[] _temporalEntities = [typeof(Region), typeof(CountryOrArea)];

	[Theory]
	[InlineData(typeof(Region), "region")]
	[InlineData(typeof(CountryOrArea), "country_or_area")]
	void Both_reference_root_tables_carry_the_temporal_stamp(Type entityType, string table)
	{
		var entity = Model.FindEntityType(entityType)!;

		entity.FindAnnotation(NorseAnnotationNames.Temporal).ShouldNotBeNull().Value.ShouldBe(true);
		// The root table name rides along per case, so the ruling's table list stays greppable from the
		// entity list and a silent DbSet or ToTable rename can't quietly re-point a ruled mark.
		entity.GetTableName().ShouldBe(table);
	}

	[Theory]
	[InlineData(typeof(CountryOrAreaView))]
	[InlineData(typeof(RegionNode))]
	[InlineData(typeof(SubregionNode))]
	[InlineData(typeof(IntermediateRegionNode))]
	void The_owned_view_document_graph_takes_no_stamp(Type entityType) =>
		Model.FindEntityType(entityType)!.FindAnnotation(NorseAnnotationNames.Temporal).ShouldBeNull();

	[Fact]
	void Exactly_the_two_root_entities_carry_the_stamp_and_nothing_else() =>
		// The per-case theories above pin the named entities; this one closes the model, so a schema
		// addition that arrives already marked has to pass through the ruling to get here.
		Model.GetEntityTypes()
			.Where(entity => entity.FindAnnotation(NorseAnnotationNames.Temporal) is not null)
			.Select(entity => entity.ClrType)
			.OrderBy(type => type.Name, StringComparer.Ordinal)
			.ShouldBe(_temporalEntities.OrderBy(type => type.Name, StringComparer.Ordinal));

	[Fact]
	void The_system_period_never_enters_the_postgres_mapped_model()
	{
		// The period is database-owned (spec §3.2): on Postgres it exists only in migration SQL, never as
		// a mapped column and never on the CLR type, which is what keeps ITemporalEntity memberless. The
		// chassis convention guards the collision; this pins the outcome, so a marker that ever started
		// contributing a shadow property here would be caught in this realm rather than downstream.
		// Postgres-only by name as well as by model: SQL Server realizes the period natively and its model
		// carries SystemPeriodStart/SystemPeriodEnd shadow properties by design, so the same fact would be
		// false there and means something else entirely.
		foreach (var entityType in _temporalEntities)
		{
			var entity = Model.FindEntityType(entityType)!;
			var table = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
			var columns = entity.GetProperties().Select(property => property.GetColumnName(table)).ToList();

			// Non-vacuity guard: an empty or all-null column list would satisfy the real assertion below
			// for the wrong reason.
			columns.ShouldContain("id");
			columns.ShouldNotContain(column =>
				string.Equals(column, "system_period", StringComparison.OrdinalIgnoreCase));
		}
	}
}
