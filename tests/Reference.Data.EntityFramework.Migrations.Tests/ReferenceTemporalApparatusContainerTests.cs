using Microsoft.EntityFrameworkCore;
using Norse.Persistence.EntityFramework;
using Norse.Persistence.EntityFramework.PostgreSQL;
using Norse.Reference.Data.EntityFramework.Migrations.PostgreSQL;

namespace Norse.Reference.Data.EntityFramework.Migrations.Tests;

/// <summary>
/// The temporal apparatus against a real <c>postgres:19beta2</c> server: <c>InitialCreate</c> applies
/// clean, the full apparatus stands for both reference root tables — and for nothing else — and the
/// seed-then-amend lifecycle versions the way §3.2 says it should. This realm keeps exactly one
/// <c>InitialCreate</c> per provider (squashed in place, never stacked, spec §7.1), so the apparatus
/// arrives at table birth through the chassis's <c>CreateTable</c> path (§3.1), not through the §3.3
/// enable transition. Scaffolded SQL that reads right and refuses to apply is the failure this suite
/// exists to catch, which is why nothing here asserts on a migration name.
/// </summary>
/// <param name="fixture">The shared container.</param>
[Collection("Postgres")]
public sealed class ReferenceTemporalApparatusContainerTests(PostgresContainerFixture fixture)
{
	// Both root tables, ruled temporal 2026-08-05. The entity-side pin lives in
	// Reference.Data.EntityFramework.Tests; this is the physical-name half.
	static readonly string[] _temporalTables = ["country_or_area", "region"];

	static CancellationToken Cancellation => TestContext.Current.CancellationToken;

	[Fact]
	async Task MigrateAsync_stands_up_the_temporal_apparatus_on_both_reference_root_tables()
	{
		await using var context = await MigratedContextAsync();

		foreach (var table in _temporalTables)
		{
			(await HasSystemPeriodAsync(context, table)).ShouldBeTrue($"{table} should carry system_period");
			(await RelationsAsync(context, $"{table}\\_%")).ShouldBe([$"{table}_history", $"{table}_timeline"]);
			(await FunctionsAsync(context, $"{table}\\_versioning")).ShouldBe([$"{table}_versioning"]);
			(await TriggerBindingsAsync(context, table)).ShouldBe(
			[
				$"{table}_versioning_delete -> {table}_versioning",
				$"{table}_versioning_insert -> {table}_versioning",
				$"{table}_versioning_update -> {table}_versioning"
			]);
		}
	}

	[Fact]
	async Task Nothing_outside_the_two_ruled_tables_carries_any_apparatus()
	{
		// The realm has only two tables today, so the "and nothing else" half has to close over the whole
		// schema rather than name a ruled-out list: any future table that arrives already versioned shows
		// up here as a fourth relation or a third function.
		await using var context = await MigratedContextAsync();

		(await RelationsAsync(context, "%\\_history")).ShouldBe(["country_or_area_history", "region_history"]);
		(await RelationsAsync(context, "%\\_timeline")).ShouldBe(["country_or_area_timeline", "region_timeline"]);
		(await FunctionsAsync(context, "%\\_versioning")).ShouldBe(["country_or_area_versioning", "region_versioning"]);
	}

	[Fact]
	async Task The_TSV_seed_mints_no_history_rows_and_opens_every_row_current()
	{
		await using var context = await MigratedContextAsync();
		try
		{
			await ResetAsync(context);
			await new ReferenceDataSeedContributor(context).SeedAsync(Cancellation);

			// An INSERT opens a version, it never closes one — the seeded state is the opening version of
			// the record, not churn, so history is empty and every seeded row's period runs to infinity.
			foreach (var table in _temporalTables)
			{
				(await CountAsync(context, $"SELECT count(*)::int AS \"Value\" FROM public.{table}_history"))
					.ShouldBe(0, $"{table}_history should be empty after a seed");
				(await CountAsync(context,
					$"SELECT count(*)::int AS \"Value\" FROM public.{table} WHERE upper(system_period) <> 'infinity'"))
					.ShouldBe(0, $"every seeded {table} row should be a current version");
			}
		}
		finally
		{
			await ResetAsync(context);
		}
	}

	[Fact]
	async Task An_amendment_to_a_seeded_country_closes_its_prior_version_into_history()
	{
		await using var context = await MigratedContextAsync();
		try
		{
			await ResetAsync(context);
			await new ReferenceDataSeedContributor(context).SeedAsync(Cancellation);

			await context.Set<CountryOrArea>()
				.Where(c => c.Code == IsoCountryCode.Nigeria)
				.ExecuteUpdateAsync(setters => setters.SetProperty(c => c.Name, "Federal Republic of Nigeria"),
					Cancellation);

			(await CountAsync(context, "SELECT count(*)::int AS \"Value\" FROM public.country_or_area_history"))
				.ShouldBe(1);
			// One UPDATE on one table versions that table alone.
			(await CountAsync(context, "SELECT count(*)::int AS \"Value\" FROM public.region_history")).ShouldBe(0);
			// The closed version carries the pre-amendment name, has strictly positive length, and hands off
			// exactly where the current row picks up — gapless by arithmetic, not by inspection.
			(await CountAsync(context,
				"""
				SELECT count(*)::int AS "Value"
				FROM public.country_or_area_history h JOIN public.country_or_area c ON c.id = h.id
				WHERE h.name = 'Nigeria'
					AND NOT isempty(h.system_period)
					AND upper(h.system_period) > lower(h.system_period)
					AND upper(h.system_period) = lower(c.system_period)
					AND upper(c.system_period) = 'infinity'
				""")).ShouldBe(1);
		}
		finally
		{
			await ResetAsync(context);
		}
	}

	/// <summary>
	/// Migrating is idempotent, so every fact here can stand the schema up for itself rather than
	/// depending on which class in the collection ran first.
	/// </summary>
	async Task<ReferenceDbContext> MigratedContextAsync()
	{
		DbContextOptionsBuilder<ReferenceDbContext> optionsBuilder = new();
		optionsBuilder.ApplyNorseProviderOptions(NorsePostgresEfProvider.Instance,
			fixture.ConnectionString, typeof(ReferenceDbContextFactory).Assembly.GetName().Name);
		ReferenceDbContext context = new(optionsBuilder.Options);
		await new NorseReferenceMigrationContributor(context).MigrateAsync(Cancellation);
		return context;
	}

	/// <summary>
	/// The shared container is never truncated between tests (Task 4's lesson), and a plain
	/// <c>DELETE</c> would now mint the very history rows these facts count. <c>TRUNCATE</c> fires no row
	/// triggers, so it is the only cleanup that leaves a genuinely pristine baseline — all four relations
	/// in one statement, since the tables reference each other.
	/// </summary>
	static Task ResetAsync(ReferenceDbContext context) =>
		context.Database.ExecuteSqlAsync(
			$"TRUNCATE TABLE public.country_or_area, public.region, public.country_or_area_history, public.region_history",
			Cancellation);

	static Task<int> CountAsync(ReferenceDbContext context, string sql) =>
		context.Database.SqlQueryRaw<int>(sql).SingleAsync(Cancellation);

	// system_period is database-owned and outside the EF model (spec §3.2), so every reading below is a
	// deliberate trip through the catalog rather than a gap in the mapping.
	static Task<bool> HasSystemPeriodAsync(ReferenceDbContext context, string table)
	{
		var qualified = $"public.{table}";
		return context.Database.SqlQuery<bool>(
			$"""
			SELECT EXISTS (
				SELECT 1 FROM pg_catalog.pg_attribute
				WHERE attrelid = {qualified}::regclass
					AND attname = 'system_period' AND NOT attisdropped) AS "Value"
			""").SingleAsync(Cancellation);
	}

	/// <summary>
	/// Ordinary tables and views only ('r', 'v'): indexes and sequences live in <c>pg_class</c> too and
	/// cannot outlive the table they belong to, so counting them would only add noise.
	/// </summary>
	static Task<List<string>> RelationsAsync(ReferenceDbContext context, string pattern) =>
		context.Database.SqlQuery<string>(
			$"""
			SELECT c.relname AS "Value"
			FROM pg_catalog.pg_class c JOIN pg_catalog.pg_namespace n ON n.oid = c.relnamespace
			WHERE n.nspname = 'public' AND c.relkind IN ('r', 'v') AND c.relname LIKE {pattern}
			ORDER BY c.relname
			""").ToListAsync(Cancellation);

	static Task<List<string>> FunctionsAsync(ReferenceDbContext context, string pattern) =>
		context.Database.SqlQuery<string>(
			$"""
			SELECT p.proname AS "Value"
			FROM pg_catalog.pg_proc p JOIN pg_catalog.pg_namespace n ON n.oid = p.pronamespace
			WHERE n.nspname = 'public' AND p.proname LIKE {pattern}
			ORDER BY p.proname
			""").ToListAsync(Cancellation);

	/// <summary>
	/// Trigger name and the function it is bound to, together: a trigger surviving under its old name and
	/// still bound to a retired function is the failure a name-only check would sail past.
	/// </summary>
	static Task<List<string>> TriggerBindingsAsync(ReferenceDbContext context, string table)
	{
		var qualified = $"public.{table}";
		return context.Database.SqlQuery<string>(
			$"""
			SELECT t.tgname || ' -> ' || p.proname AS "Value"
			FROM pg_catalog.pg_trigger t JOIN pg_catalog.pg_proc p ON p.oid = t.tgfoid
			WHERE t.tgrelid = {qualified}::regclass AND NOT t.tgisinternal
			ORDER BY t.tgname
			""").ToListAsync(Cancellation);
	}
}
