using Norse.Primitives;

namespace Norse.Reference.Data.Contracts.Tests;

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
	void The_baked_identifier_parses_back_to_its_country()
	{
		// The fourth form: the deterministic v5 identifier Iso3166 bakes is accepted as input, so a
		// consumer holding only the FK from an EF row hydrates the full ISO canon without a join —
		// and the wire's own response Id round-trips as the next request's input.
		var id = Iso3166.Ids[IsoCountryCode.UnitedStatesOfAmerica];

		IsoCountryCodes.Parse(id.ToString()).TryGetValue(out Success<IsoCountryCode> success).ShouldBeTrue();
		success.Value.ShouldBe(IsoCountryCode.UnitedStatesOfAmerica);
	}

	[Fact]
	void Every_baked_identifier_round_trips()
	{
		foreach (var row in Iso3166.All)
		{
			IsoCountryCodes.Parse(row.Id.ToString()).TryGetValue(out Success<IsoCountryCode> success).ShouldBeTrue();
			success.Value.ShouldBe(row.Code);
		}
	}

	[Fact]
	void A_well_formed_but_unknown_guid_fails_as_malformed()
	{
		// Well-formed is not known: a Guid outside the baked dataset denotes nothing and fails
		// exactly like any other unrecognized text — the taxonomy stays two-reason (Empty/Malformed).
		IsoCountryCodes.Parse("11111111-2222-3333-4444-555555555555")
			.TryGetValue(out Failure failure).ShouldBeTrue();
		failure.Reason.ShouldBe(ParseFailure.Malformed);
	}

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
