namespace Norse.Reference.Data.Migrations.Tests;

/// <summary>
/// Contributor-level unit coverage for <see cref="ReferenceDataSeedContributor.ResolveCountryCode"/> — no
/// database, no container. The seed drift guard (spec §9.11, acceptance 11) fires synchronously while a
/// TSV row's M49 code is resolved, before any database I/O, so it is provable at this level alone.
/// </summary>
public sealed class ReferenceDataSeedContributorUnitTests
{
	[Fact]
	void ResolveCountryCode_throws_naming_the_code_when_it_is_unknown_to_the_generated_ISO_3166_1_surface()
	{
		var exception = Should.Throw<InvalidOperationException>(() =>
			ReferenceDataSeedContributor.ResolveCountryCode("999"));

		exception.Message.ShouldContain("999");
	}
}
