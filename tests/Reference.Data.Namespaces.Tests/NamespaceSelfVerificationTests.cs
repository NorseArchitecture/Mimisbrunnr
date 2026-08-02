using System.Globalization;
using Norse.Primitives.Identifiers;

namespace Norse.Reference.Data.Namespaces.Tests;

public sealed class NamespaceSelfVerificationTests
{
	[Fact]
	void The_iso3166_namespace_rechains_from_root() =>
		new DeterministicGuid(ReferenceNamespaces.Root, "iso3166-1").Value.ShouldBe(ReferenceNamespaces.Iso3166);

	[Fact]
	void Every_shipped_row_guid_recomputes_via_deterministic_guid()
	{
		foreach (var country in Iso3166.All)
			new DeterministicGuid(ReferenceNamespaces.Iso3166, ((ushort)country.Code).ToString("D3", CultureInfo.InvariantCulture))
				.Value.ShouldBe(country.Id, $"{country.Alpha3} drifted");
	}
}
