using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Norse.EntityFramework;

namespace Norse.ReferenceData.Data.Migrations;

/// <summary>
/// Design-time factory for <see cref="ReferenceDataDbContext"/>, used only by <c>dotnet ef</c> tooling
/// (e.g. <c>dotnet ef migrations add</c>) to construct a context instance outside of DI at design time.
/// </summary>
public sealed class ReferenceDataDbContextFactory : IDesignTimeDbContextFactory<ReferenceDataDbContext>
{
	/// <inheritdoc />
	public ReferenceDataDbContext CreateDbContext(string[] args)
	{
		var connectionString =
			Environment.GetEnvironmentVariable("DOTNET_EFTOOLS_CONNECTIONSTRING")
			?? "Host=localhost;Port=5432;Database=norse_referencedata;Username=postgres;Password=devpassword";

		var optionsBuilder = new DbContextOptionsBuilder<ReferenceDataDbContext>()
			.UseNpgsql(connectionString,
				o => o.MigrationsAssembly(typeof(ReferenceDataDbContextFactory).Assembly.GetName().Name));

		NorseDbContextOptionsExtensions.ApplyNorseConventions(optionsBuilder);

		return new ReferenceDataDbContext(optionsBuilder.Options);
	}
}
