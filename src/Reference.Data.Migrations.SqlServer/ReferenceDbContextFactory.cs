using Microsoft.EntityFrameworkCore;
using Norse.Persistence.EntityFramework;
using Norse.Persistence.EntityFramework.Design;
using Norse.Persistence.EntityFramework.SqlServer;
using Norse.Reference.Data.EntityFramework;

namespace Norse.Reference.Data.Migrations.SqlServer;

/// <summary>
/// Design-time factory for <see cref="ReferenceDbContext"/>, used only by <c>dotnet ef</c> tooling
/// (e.g. <c>dotnet ef migrations add</c>) to construct a context instance outside of DI at design time.
/// </summary>
public sealed class ReferenceDbContextFactory : NorseDesignTimeDbContextFactory<ReferenceDbContext>
{
	/// <inheritdoc />
	protected override INorseEfProvider ProviderBinding => NorseSqlServerEfProvider.Instance;

	/// <inheritdoc />
	protected override string DatabaseName => "norse_reference";

	/// <inheritdoc />
	protected override ReferenceDbContext CreateContext(DbContextOptions<ReferenceDbContext> options) =>
		new(options);
}
