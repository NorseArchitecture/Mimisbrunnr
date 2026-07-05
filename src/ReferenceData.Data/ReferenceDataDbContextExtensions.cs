using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Norse.ReferenceData.Data;

/// <summary>
/// Query extensions for <see cref="ReferenceDataDbContext"/>.
/// </summary>
public static class ReferenceDataDbContextExtensions
{
	static readonly JsonSerializerOptions _dossierJsonOptions = new() { PropertyNameCaseInsensitive = true };

	/// <summary>
	/// Retrieves the nested country-or-area dossier document for the given UN M49 <paramref name="code"/>,
	/// deserialized from the <c>country_or_area_dossier</c> view's <c>jsonb</c> column.
	/// </summary>
	/// <param name="context">The reference-data context.</param>
	/// <param name="code">The UN M49 code of the country or area.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>The dossier, or <see langword="null"/> if no country or area matches <paramref name="code"/>.</returns>
	public static async Task<CountryOrAreaDossier?> GetCountryOrAreaDossierAsync(
		this ReferenceDataDbContext context, string code, CancellationToken cancellationToken)
	{
		var row = await context.Set<CountryOrAreaDossierRow>()
			.Where(r => r.Code == code)
			.SingleOrDefaultAsync(cancellationToken)
			.ConfigureAwait(false);

		return row is null ? null : JsonSerializer.Deserialize<CountryOrAreaDossier>(row.Dossier, _dossierJsonOptions);
	}
}
