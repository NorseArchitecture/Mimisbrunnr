using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norse.Persistence.EntityFramework;
using Norse.Primitives.Identifiers;

namespace Norse.Reference.Data;

/// <summary>
/// A country or area per UN M49 with ISO and LDC classifications.
/// </summary>
public sealed record CountryOrArea : NorseEntityBase<CountryOrArea>, INorseEntity<CountryOrArea>
{
	/// <summary>The country-or-area identifier.</summary>
	public required DeterministicGuid Id { get; init; }
	/// <summary>The UN M49 code (3 digits).</summary>
	public required ushort Code { get; init; }
	/// <summary>The ISO 3166-1 alpha-2 code (2 letters).</summary>
	[FixedLength(2)]
	public required string Alpha2 { get; init; }
	/// <summary>The ISO 3166-1 alpha-3 code (3 letters).</summary>
	[FixedLength(3)]
	public required string Alpha3 { get; init; }
	/// <summary>The country or area name in English.</summary>
	public required string Name { get; init; }
	/// <summary>The parent region identifier, if applicable.</summary>
	public DeterministicGuid? ParentRegionId { get; init; }
	/// <summary>The parent region, if applicable.</summary>
	public Region ParentRegion { get; init; } = null!;
	/// <summary>The UN classification flags this country or area holds. Test with <see cref="Enum.HasFlag"/>.</summary>
	public required Classification Classification { get; init; }

	/// <summary>
	/// The denormalized read-model column: this row's own scalar fields alongside the ancestor
	/// Region/Subregion/IntermediateRegion chain, hydrated by the seed contributor and stored as an
	/// owned JSON document. Always present — only <see cref="CountryOrAreaView.Region"/> is
	/// <see langword="null"/>, and only for Antarctica, which has no ancestor at all. Named
	/// <c>View</c> as a deliberate homage to the SQL view it replaced: this is the platform's first
	/// "peer + ancestry" read column, one per entity, queried without joins.
	/// </summary>
	public required CountryOrAreaView View { get; init; }

	/// <summary>Configures the EF entity mapping.</summary>
	public static void Configure(EntityTypeBuilder<CountryOrArea> builder)
	{
		builder.HasKey(c => c.Id);
		builder.Property(c => c.Name).HasMaxLength(256);
		builder.HasIndex(c => c.Code).IsUnique();
		builder.HasIndex(c => c.Alpha2).IsUnique();
		builder.HasIndex(c => c.Alpha3).IsUnique();
		builder
			.HasOne(c => c.ParentRegion)
			.WithMany(c => c.CountriesOrAreas)
			.HasForeignKey(c => c.ParentRegionId);
		// View model map
		builder.OwnsOne(c => c.View, view =>
		{
			view.ToJson();
			view.OwnsOne(v => v.Region, region =>
				region.OwnsOne(r => r.Subregion,
					sub => sub.OwnsOne(s => s.IntermediateRegion)));
		});
	}
}
