using Norse.EntityFramework.Migrations;

namespace Norse.ReferenceData.Data.Migrations;

/// <summary>
/// Migration contributor for <see cref="ReferenceDataDbContext"/>, discovered by the migrations
/// service and executed at startup to apply pending reference-data schema migrations.
/// </summary>
/// <param name="context">The reference-data context instance resolved from DI.</param>
[MigrationConnectionString("norse_referencedata")]
public sealed class NorseReferenceDataMigrationContributor(ReferenceDataDbContext context)
	: EfMigrationContributor<ReferenceDataDbContext>(context)
{
	/// <inheritdoc />
	public override string Name => "Norse.ReferenceData";
}
