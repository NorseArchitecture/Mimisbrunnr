using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norse.EntityFramework;

namespace Norse.ReferenceData.Data;

/// <summary>
/// A geographic region per UN M49 (Region, Subregion, or Intermediate Region).
/// </summary>
public sealed class Region : NorseEntityBase<Region>, INorseEntity<Region>
{
	/// <summary>The region identifier.</summary>
	public Guid Id { get; init; }
	/// <summary>The UN M49 code (3 digits).</summary>
	public string M49Code { get; init; } = null!;
	/// <summary>The region name in English.</summary>
	public string Name { get; init; } = null!;
	/// <summary>The hierarchical level of this region.</summary>
	public RegionLevel Level { get; init; }
	/// <summary>The parent region identifier, if this region is a child.</summary>
	public Guid? ParentRegionId { get; init; }
	/// <summary>The parent region, if this region is a child.</summary>
	public Region ParentRegion { get; init; } = null!;

	/// <summary>Configures the EF entity mapping.</summary>
	public static void Configure(EntityTypeBuilder<Region> builder)
	{
		builder.ToTable("regions");
		builder.HasKey(r => r.Id);
		builder.Property(r => r.M49Code).HasMaxLength(3).IsRequired();
		builder.Property(r => r.Name).HasMaxLength(256).IsRequired();
		builder.HasIndex(r => r.M49Code).IsUnique().HasDatabaseName("uq_regions_m49_code");
		builder.HasOne(r => r.ParentRegion).WithMany().HasForeignKey(r => r.ParentRegionId).IsRequired(false);
	}
}
