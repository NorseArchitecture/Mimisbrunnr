using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norse.EntityFramework;

namespace Norse.ReferenceData.Data;

/// <summary>
/// The keyless read model backing the <c>country_or_area_dossier</c> view (raw-SQL view migration,
/// no corresponding table). Internal — <see cref="ReferenceDataDbContextExtensions.GetCountryOrAreaDossierAsync"/>
/// is the public query surface; callers never see this row shape directly.
/// </summary>
sealed class CountryOrAreaDossierRow : NorseEntityBase<CountryOrAreaDossierRow>, INorseEntity<CountryOrAreaDossierRow>
{
	public string Code { get; init; } = null!;
	public string Alpha2 { get; init; } = null!;
	public string Alpha3 { get; init; } = null!;
	public string Dossier { get; init; } = null!;

	/// <summary>Configures the keyless view mapping.</summary>
	public static void Configure(EntityTypeBuilder<CountryOrAreaDossierRow> builder)
	{
		builder.HasNoKey();
		builder.ToView("country_or_area_dossier");
		builder.Property(r => r.Code).HasMaxLength(3);
		builder.Property(r => r.Alpha2).HasMaxLength(2);
		builder.Property(r => r.Alpha3).HasMaxLength(3);
		builder.Property(r => r.Dossier).HasColumnName("dossier").HasColumnType("jsonb").HasMaxLength(-1);
	}
}
