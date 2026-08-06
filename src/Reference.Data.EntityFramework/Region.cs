using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norse.Persistence.EntityFramework;
using Norse.Primitives.Identifiers;

namespace Norse.Reference.Data.EntityFramework;

/// <summary>
/// A geographic region per UN M49 (Region, Subregion, or Intermediate Region). System-versioned: UN
/// canon changes rarely, and the record of when a region was renamed or re-parented is exactly what
/// system-time history is for.
/// </summary>
public sealed record Region : NorseEntityBase<Region>, INorseEntity<Region>, ITemporalEntity
{
	/// <summary>The region identifier.</summary>
	public required DeterministicGuid Id { get; init; }
	/// <summary>The UN M49 code (3 digits).</summary>
	public required ushort Code { get; init; }
	/// <summary>The region name in English.</summary>
	public required string Name { get; init; }
	/// <summary>The hierarchical level of this region.</summary>
	public required RegionLevel Level { get; init; }
	/// <summary>The parent region identifier, if this region is a child.</summary>
	public DeterministicGuid? ParentRegionId { get; init; }

	/// <summary>The parent region, if this region is a child.</summary>
	public Region ParentRegion { get; init; } = null!;

	/// <summary>Child region navigation property</summary>
	public ICollection<Region> ChildRegions { get; init; } = [];

	/// <summary>Countries or areas</summary>
	public ICollection<CountryOrArea> CountriesOrAreas { get; init; } = [];

	/// <summary>Configures the EF entity mapping.</summary>
	public static void Configure(EntityTypeBuilder<Region> builder)
	{
		builder.HasKey(r => r.Id);
		builder.Property(r => r.Name).HasMaxLength(256);
		builder.HasIndex(r => r.Code).IsUnique();
		builder
			.HasOne(r => r.ParentRegion)
			.WithMany(c => c.ChildRegions)
			.HasForeignKey(r => r.ParentRegionId);
	}
}
