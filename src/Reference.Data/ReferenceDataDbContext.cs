using Microsoft.EntityFrameworkCore;
using Norse.Persistence.EntityFramework;

namespace Norse.Reference.Data;

/// <summary>
/// The Entity Framework Core context for reference-data entities (Regions and Countries or Areas).
/// </summary>
public sealed partial class ReferenceDataDbContext(DbContextOptions<ReferenceDataDbContext> options) :
	NorseDbContext(options);
