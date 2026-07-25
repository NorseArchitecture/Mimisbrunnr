using Npgsql;

namespace Norse.Reference.Data.Tests;

[Collection("Postgres")]
public class PostgresContainerFixtureTests(PostgresContainerFixture fixture)
{
	[Fact]
	public async Task Container_accepts_a_real_connection()
	{
		await using NpgsqlConnection connection = new(fixture.ConnectionString);
		await connection.OpenAsync(TestContext.Current.CancellationToken);

		connection.State.ShouldBe(System.Data.ConnectionState.Open);
	}
}
