using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Norse.Persistence.EntityFramework.Design;

namespace Norse.Reference.Data.Migrations.PostgreSQL;

/// <summary>
/// Installs <see cref="DdlEmittingMigrationsScaffolder"/> for <c>dotnet ef</c> tooling, so every
/// <c>migrations add</c>/<c>migrations remove</c> run against this project also refreshes
/// <c>schema/norse_reference.sql</c>.
/// </summary>
sealed class DesignTimeServices : IDesignTimeServices
{
	public void ConfigureDesignTimeServices(IServiceCollection services) =>
		services.AddNorseDesignTimeServices("norse_reference");
}
