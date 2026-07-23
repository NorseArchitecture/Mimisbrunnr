# Norse.Reference.Data.Migrations

Migration contributor and `ISeedContributor` that loads the UN M49 TSVs, provider-agnostic. Migration tooling only — never referenced from a runtime container.

Provider-specific `IDesignTimeDbContextFactory` implementations and checked-in EF migrations live in the sibling `Reference.Data.Migrations.PostgreSQL` and `Reference.Data.Migrations.SqlServer` projects, each of which references this one.

Part of the [Norse Architecture](https://github.com/NorseArchitecture) platform.
