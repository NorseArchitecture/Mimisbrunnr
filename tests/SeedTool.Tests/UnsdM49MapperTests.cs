using Norse.Primitives.Ingestion;
using Norse.SeedTool.Mappers;

namespace Norse.SeedTool.Tests;

public sealed class UnsdM49MapperTests
{
	const string Header =
		"Global Code;Global Name;Region Code;Region Name;Sub-region Code;Sub-region Name;" +
		"Intermediate Region Code;Intermediate Region Name;Country or Area;M49 Code;" +
		"ISO-alpha2 Code;ISO-alpha3 Code;Least Developed Countries (LDC);" +
		"Land Locked Developing Countries (LLDC);Small Island Developing States (SIDS)";

	static readonly string[] _rows =
	[
		"001;World;002;Africa;202;Sub-Saharan Africa;011;Western Africa;Nigeria;566;NG;NGA;;;",
		"001;World;002;Africa;015;Northern Africa;;;Algeria;012;DZ;DZA;;;",
		"001;World;;;;;;;Antarctica;010;AQ;ATA;;;",
		"001;World;002;Africa;014;Eastern Africa;;;Ethiopia;231;ET;ETH;x;x;",
	];

	[Fact]
	void Map_deduplicates_the_region_tree_and_resolves_parents()
	{
		var path = WriteFixture();
		try
		{
			using ITabularReader reader = TabularReader.OpenDelimited(path, ';');
			var (regions, _) = UnsdM49Mapper.Map(reader);

			regions.Count.ShouldBe(5);
			regions.Any(r => r is { M49Code: "002", Name: "Africa", Level: "Region", ParentM49Code: null }).ShouldBeTrue();
			regions.Any(r => r is { M49Code: "202", Name: "Sub-Saharan Africa", Level: "Subregion", ParentM49Code: "002" }).ShouldBeTrue();
			regions.Any(r => r is { M49Code: "015", Name: "Northern Africa", Level: "Subregion", ParentM49Code: "002" }).ShouldBeTrue();
			regions.Any(r => r is { M49Code: "014", Name: "Eastern Africa", Level: "Subregion", ParentM49Code: "002" }).ShouldBeTrue();
		}
		finally
		{
			File.Delete(path);
		}
	}

	[Fact]
	void Map_resolves_a_three_level_deep_country()
	{
		var path = WriteFixture();
		try
		{
			using ITabularReader reader = TabularReader.OpenDelimited(path, ';');
			var (regions, countries) = UnsdM49Mapper.Map(reader);

			regions.Any(r => r is { M49Code: "011", Name: "Western Africa", Level: "IntermediateRegion", ParentM49Code: "202" }).ShouldBeTrue();
			countries.Any(c => c is { M49Code: "566", Name: "Nigeria", ParentM49Code: "011" }).ShouldBeTrue();
		}
		finally
		{
			File.Delete(path);
		}
	}

	[Fact]
	void Map_leaves_antarctica_with_no_ancestor_at_all()
	{
		var path = WriteFixture();
		try
		{
			using ITabularReader reader = TabularReader.OpenDelimited(path, ';');
			var (_, countries) = UnsdM49Mapper.Map(reader);

			countries.Any(c => c is { M49Code: "010", Name: "Antarctica", ParentM49Code: null }).ShouldBeTrue();
		}
		finally
		{
			File.Delete(path);
		}
	}

	[Fact]
	void Map_resolves_classification_flags()
	{
		var path = WriteFixture();
		try
		{
			using ITabularReader reader = TabularReader.OpenDelimited(path, ';');
			var (_, countries) = UnsdM49Mapper.Map(reader);

			countries.Any(c => c is
			{
				M49Code: "231",
				IsLeastDevelopedCountry: true,
				IsLandLockedDevelopingCountry: true,
				IsSmallIslandDevelopingState: false,
			}).ShouldBeTrue();
			countries.Any(c => c is
			{
				M49Code: "566",
				IsLeastDevelopedCountry: false,
				IsLandLockedDevelopingCountry: false,
				IsSmallIslandDevelopingState: false,
			}).ShouldBeTrue();
		}
		finally
		{
			File.Delete(path);
		}
	}

	[Fact]
	void Map_throws_on_a_malformed_m49_code()
	{
		var path = WriteFixture(_rows[0].Replace("566", "abc"));
		try
		{
			using ITabularReader reader = TabularReader.OpenDelimited(path, ';');

			Should.Throw<InvalidOperationException>(() => UnsdM49Mapper.Map(reader));
		}
		finally
		{
			File.Delete(path);
		}
	}

	static string WriteFixture(string? replacementFirstRow = null)
	{
		string[] rows = replacementFirstRow is null ? _rows : [replacementFirstRow, .. _rows[1..]];
		var path = Path.GetTempFileName();
		File.WriteAllLines(path, [Header, .. rows]);
		return path;
	}
}
