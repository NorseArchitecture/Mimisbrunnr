using Microsoft.EntityFrameworkCore;
using Norse.Persistence.EntityFramework.Design.PostgreSQL;

namespace Norse.Reference.Data.Migrations.PostgreSQL;

/// <summary>
/// Design-time factory for <see cref="ReferenceDataDbContext"/>, used only by <c>dotnet ef</c> tooling
/// (e.g. <c>dotnet ef migrations add</c>) to construct a context instance outside of DI at design time.
/// </summary>
public sealed class ReferenceDataDbContextFactory : NorsePostgreSqlDesignTimeDbContextFactory<ReferenceDataDbContext>
{
	/// <inheritdoc />
	protected override string DatabaseName => "norse_referencedata";

	/// <inheritdoc />
	protected override ReferenceDataDbContext CreateContext(DbContextOptions<ReferenceDataDbContext> options) =>
		new(options);
}
