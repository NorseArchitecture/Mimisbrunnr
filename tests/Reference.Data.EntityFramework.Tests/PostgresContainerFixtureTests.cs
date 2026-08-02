using Npgsql;

namespace Norse.Reference.Data.EntityFramework.Tests;

[Collection("Postgres")]
public sealed class PostgresContainerFixtureTests(PostgresContainerFixture fixture)
{
	[Fact]
	async Task Container_accepts_a_real_connection()
	{
		await using NpgsqlConnection connection = new(fixture.ConnectionString);
		await connection.OpenAsync(TestContext.Current.CancellationToken);

		connection.State.ShouldBe(System.Data.ConnectionState.Open);
	}
}
