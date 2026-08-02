using Norse.Persistence.EntityFramework.Migrations;
using Norse.Reference.Data.EntityFramework;

namespace Norse.Reference.Data.Migrations;

/// <summary>
/// Migration contributor for <see cref="ReferenceDbContext"/>, discovered by the migrations
/// service and executed at startup to apply pending reference-data schema migrations.
/// </summary>
/// <param name="context">The reference-data context instance resolved from DI.</param>
[MigrationConnectionString("norse_reference")]
public sealed class NorseReferenceMigrationContributor(ReferenceDbContext context) :
	EfMigrationContributor<ReferenceDbContext>(context)
{
	/// <inheritdoc />
	public override string Name => "Norse.Reference";
}
