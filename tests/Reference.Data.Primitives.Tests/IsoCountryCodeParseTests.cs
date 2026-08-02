using Norse.Primitives;

namespace Norse.Reference.Data.Primitives.Tests;

public sealed class IsoCountryCodeParseTests
{
	[Theory]
	[InlineData("US")]
	[InlineData("us")]
	[InlineData("USA")]
	[InlineData(" usa ")]
	[InlineData("840")]
	void All_three_forms_parse_to_the_united_states(string input)
	{
		IsoCountryCodes.Parse(input).TryGetValue(out Success<IsoCountryCode> success).ShouldBeTrue();
		success.Value.ShouldBe(IsoCountryCode.UnitedStatesOfAmerica);
	}

	[Fact]
	void Unpadded_numerics_parse_without_string_laundering()
	{
		IsoCountryCodes.Parse("40").TryGetValue(out Success<IsoCountryCode> success).ShouldBeTrue();
		success.Value.ShouldBe(IsoCountryCode.Austria);
	}

	[Theory]
	[InlineData("")]
	[InlineData("Q")]
	[InlineData("USAX")]
	[InlineData("99999")]
	[InlineData("banana")]
	void Garbage_fails_as_a_result_problem(string input) =>
		IsoCountryCodes.Parse(input).TryGetValue(out Success<IsoCountryCode> _).ShouldBeFalse();

	[Fact]
	void The_span_overload_allocates_nothing()
	{
		// Allocation gate (acceptance 2). Warm up, then measure.
		Span<char> buffer = ['U', 'S', 'A'];
		IsoCountryCodes.Parse(buffer);
		var before = GC.GetAllocatedBytesForCurrentThread();
		for (var i = 0; i < 1_000; i++)
			IsoCountryCodes.Parse(buffer);
		(GC.GetAllocatedBytesForCurrentThread() - before).ShouldBe(0);
	}
}
