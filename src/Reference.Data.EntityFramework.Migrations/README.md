# Norse.Reference.Data.EntityFramework.Migrations

Migration contributor and `ISeedContributor` that loads the UN M49 TSVs, provider-agnostic. Migration tooling only — never referenced from a runtime container.

Provider-specific `IDesignTimeDbContextFactory` implementations and checked-in EF migrations live in the sibling `Reference.Data.EntityFramework.Migrations.PostgreSQL` and `Reference.Data.EntityFramework.Migrations.SqlServer` projects, each of which references this one.

Part of the [Norse Architecture](https://github.com/NorseArchitecture) platform.
