using Testcontainers.PostgreSql;

namespace Norse.Reference.Data.Tests;

public sealed class PostgresContainerFixture : IAsyncLifetime
{
	readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:19beta2")
		.WithDatabase("norse_reference")
		.Build();

	public string ConnectionString { get; private set; } = null!;

	public async ValueTask InitializeAsync()
	{
		await _container.StartAsync().ConfigureAwait(false);
		ConnectionString = _container.GetConnectionString();
	}

	public async ValueTask DisposeAsync() => await _container.DisposeAsync().ConfigureAwait(false);
}
