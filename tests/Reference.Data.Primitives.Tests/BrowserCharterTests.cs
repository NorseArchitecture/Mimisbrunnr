namespace Norse.Reference.Data.Primitives.Tests;

public sealed class BrowserCharterTests
{
	[Fact]
	void The_assembly_references_no_ef_and_never_the_namespaces_assembly()
	{
		var referenced = typeof(IsoCountryCode).Assembly.GetReferencedAssemblies()
			.Select(a => a.Name!).ToList();
		referenced.Any(n => n.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal)).ShouldBeFalse();
		referenced.ShouldNotContain("Norse.Reference.Data.Namespaces");
	}
}
