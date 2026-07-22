using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norse.Persistence.EntityFramework;

namespace Norse.Reference.Data;

/// <summary>
/// A country or area per UN M49 with ISO and LDC classifications.
/// </summary>
public sealed class CountryOrArea : NorseEntityBase<CountryOrArea>, INorseEntity<CountryOrArea>
{
	/// <summary>The country-or-area identifier.</summary>
	public Guid Id { get; init; }
	/// <summary>The UN M49 code (3 digits).</summary>
	public string M49Code { get; init; } = null!;
	/// <summary>The ISO 3166-1 alpha-2 code (2 letters).</summary>
	public string IsoAlpha2Code { get; init; } = null!;
	/// <summary>The ISO 3166-1 alpha-3 code (3 letters).</summary>
	public string IsoAlpha3Code { get; init; } = null!;
	/// <summary>The country or area name in English.</summary>
	public string Name { get; init; } = null!;
	/// <summary>The parent region identifier, if applicable.</summary>
	public Guid? ParentRegionId { get; init; }
	/// <summary>The parent region, if applicable.</summary>
	public Region ParentRegion { get; init; } = null!;
	/// <summary>True if this is a Least Developed Country per UN classification.</summary>
	public bool IsLeastDevelopedCountry { get; init; }
	/// <summary>True if this is a Land Locked Developing Country per UN classification.</summary>
	public bool IsLandLockedDevelopingCountry { get; init; }
	/// <summary>True if this is a Small Island Developing State per UN classification.</summary>
	public bool IsSmallIslandDevelopingState { get; init; }
	/// <summary>
	/// The denormalized read-model column: the ancestor Region/Subregion/IntermediateRegion chain,
	/// hydrated by the seed contributor and stored as an owned JSON document — <see langword="null"/>
	/// only for Antarctica. Named <c>View</c> as a deliberate homage to the SQL view it replaced: this
	/// is the platform's first "peer + ancestry" read column, one per entity, queried without joins.
	/// </summary>
	public RegionNode? View { get; init; }

	/// <summary>Configures the EF entity mapping.</summary>
	public static void Configure(EntityTypeBuilder<CountryOrArea> builder)
	{
		builder.HasKey(c => c.Id);
		builder.Property(c => c.M49Code).HasMaxLength(3).IsRequired();
		builder.Property(c => c.IsoAlpha2Code).HasMaxLength(2).IsRequired();
		builder.Property(c => c.IsoAlpha3Code).HasMaxLength(3).IsRequired();
		builder.Property(c => c.Name).HasMaxLength(256).IsRequired();
		builder.HasIndex(c => c.M49Code).IsUnique();
		builder.HasIndex(c => c.IsoAlpha2Code).IsUnique();
		builder.HasIndex(c => c.IsoAlpha3Code).IsUnique();
		builder
			.HasOne(c => c.ParentRegion)
			.WithMany()
			.HasForeignKey(c => c.ParentRegionId)
			.IsRequired(false);
		builder.OwnsOne(c => c.View, region =>
		{
			region.ToJson();
			region.OwnsOne(r => r.Subregion, sub => sub.OwnsOne(s => s.IntermediateRegion));
		});
	}
}
