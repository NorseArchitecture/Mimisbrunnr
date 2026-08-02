using Microsoft.EntityFrameworkCore;
using Norse.Persistence.EntityFramework;
using Norse.Persistence.EntityFramework.Design;
using Norse.Persistence.EntityFramework.PostgreSQL;

namespace Norse.Reference.Data.EntityFramework.Migrations.PostgreSQL;

/// <summary>
/// Design-time factory for <see cref="ReferenceDbContext"/>, used only by <c>dotnet ef</c> tooling
/// (e.g. <c>dotnet ef migrations add</c>) to construct a context instance outside of DI at design time.
/// </summary>
public sealed class ReferenceDbContextFactory : NorseDesignTimeDbContextFactory<ReferenceDbContext>
{
	/// <inheritdoc />
	protected override INorseEfProvider ProviderBinding => NorsePostgresEfProvider.Instance;

	/// <inheritdoc />
	protected override string DatabaseName => "norse_reference";

	/// <inheritdoc />
	protected override ReferenceDbContext CreateContext(DbContextOptions<ReferenceDbContext> options) =>
		new(options);
}
