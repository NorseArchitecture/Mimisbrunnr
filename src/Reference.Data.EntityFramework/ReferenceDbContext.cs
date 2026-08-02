using Microsoft.EntityFrameworkCore;
using Norse.Persistence.EntityFramework;

namespace Norse.Reference.Data.EntityFramework;

/// <summary>
/// The Entity Framework Core context for reference-data entities (Regions and Countries or Areas).
/// </summary>
public sealed partial class ReferenceDbContext(DbContextOptions<ReferenceDbContext> options) :
	NorseDbContext(options)
{
	/// <summary>
	/// The well root Midgard's <c>AddWell&lt;TContext&gt;()</c> discovers by reflection (well-and-wire
	/// spec §3.1) — discovery scans this context's public <see cref="DbSet{TEntity}"/> properties for
	/// <c>IViewBearer&lt;TView&gt;</c> implementors, so this accessor is what makes
	/// <c>CountryOrArea</c>/<c>CountryOrAreaView</c> a registrable well, not a convenience wrapper —
	/// entity configuration itself is driven by the generated <c>ConfigureNorseEntities</c> override
	/// (Urðarbrunnr's <c>EntityConfigurationApplicationGenerator</c>) independent of this property.
	/// Deliberately named to match <see cref="CountryOrArea"/>'s own type name, not pluralized: EF
	/// Core's table-naming convention resolves from the DbSet property name once one exists, and the
	/// already-shipped migration/model snapshot fixes the table as <c>country_or_area</c>/
	/// <c>CountryOrArea</c> (singular) — a pluralized property name here would silently rename the
	/// table out from under the committed schema (squash law, spec §7.1).
	/// </summary>
	public DbSet<CountryOrArea> CountryOrArea => Set<CountryOrArea>();
}
