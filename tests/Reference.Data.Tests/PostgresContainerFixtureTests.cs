using System.Diagnostics.CodeAnalysis;
using Npgsql;

namespace Norse.Reference.Data.Tests;

[Collection("Postgres")]
public class PostgresContainerFixtureTests(PostgresContainerFixture fixture)
{
	[Fact]
	[SuppressMessage("Performance", "CA2007:Do not directly await a Task", Justification = "xUnit test method context does not require ConfigureAwait")]
	public async Task Container_accepts_a_real_connection()
	{
		await using var connection = new NpgsqlConnection(fixture.ConnectionString);
		await connection.OpenAsync(TestContext.Current.CancellationToken);

		connection.State.ShouldBe(System.Data.ConnectionState.Open);
	}
}
