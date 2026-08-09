# CLAUDE.md — Mímisbrunnr (`Norse.Reference.Data`)

## 0. Wrong Root — Halt

Session root must be **Bifröst**, not this repo directly — org-wide settings (`superpowers`, permission rules) only apply from the actual root, and Claude Code never merges a submodule's own `.claude/settings.json` into a parent-launched session. If `claude` was run from inside **Mímisbrunnr**, stop: don't read further, don't propose changes, don't run anything — tell the user to `cd ../Bifrost` and start there. (This repo's `.claude/settings.json` carries a `SessionStart` hook meant to block this before you ever see this file; if you're reading this anyway, the hook was bypassed, disabled, or failed — halt regardless.)

> **Do not commit, push, or rewrite git history** — stage (`git add`), show the diff, stop; the human reviews and commits. This applies even when a skill's flow includes a commit step. **US English spelling** everywhere — code, comments, docs, commits.

## What This Realm Is

Mímisbrunnr is **the reference-data store** — `Norse.Reference.Data`: canonical external-standard data this platform doesn't get to redefine. First tenants: UN M49 regions and countries (live), with ISO currency codes, languages, scripts, locales, and IANA time zones sketched but unconverged. A deliberate pair with Urðarbrunnr: both are wells at the tree's roots — Urðarbrunnr holds the record of what has happened (the migrations chassis), Mímisbrunnr holds what is known (external canon). It ships as pure NuGet classlibs, spun up from Yggdrasil's composition root — no standalone host: declare in a realm, host in Yggdrasil, compose in Bifröst.

| Project | Carries | Rule |
|---|---|---|
| `Reference.Data.Contracts` | The generated, browser-supported primitives surface (`IsoCountryCode`, `Iso3166`) | Authored **here, in-realm** — the migration and seed contributors resolve country codes through this realm's own generated surface, never a companion-repo dependency |
| `Reference.Data.Namespaces` | The generated `ReferenceNamespaces` surface | Generator-mounted only; no other dependency |
| `Reference.Data.EntityFramework` | The entities (`Region`, `CountryOrArea`, the `RegionNode` hierarchy), `ReferenceDbContext` (inherits `NorseDbContext`), and view models (the owned `CountryOrAreaView` jsonb document graph) | The interop boundary with Mímir — see the SemVer contract below |
| `Reference.Data.EntityFramework.Migrations` | `NorseReferenceMigrationContributor` and `ReferenceDataSeedContributor` — loads the committed `seeds/*.tsv` via Svartálfheim's `Primitives.Ingestion` | `ISeedContributor` lives here, inside the existing migrations project — no new assemblies (seeding-framework law) |
| `Reference.Data.EntityFramework.Migrations.PostgreSQL` / `.SqlServer` | Per-provider `InitialCreate` (temporal apparatus and all), embedded `schema/norse_reference.sql` DDL, offline design-time factory | One `InitialCreate` per provider — squash, never stack (see Build & Test) |
| `gen/Reference.Data.Contracts.Generator` | The Roslyn generator emitting the `Primitives`/`Namespaces` surfaces | Mounted as an analyzer by both consuming projects; consumers of the published packages receive generated code, never the generator |
| `tools/SeedTool` | Dev-only console app converting `seeds/raw/UNSD — Methodology.csv` into the committed `seeds/region.tsv`/`seeds/country-or-area.tsv` | Never packed, never AOT-published |

**Dependency posture:** rides Urðarbrunnr's EF foundation (`Persistence.EntityFramework` + `.Migrations` + the PostgreSQL/SqlServer provider legs) and everything below, plus Asgard's `Abstractions.Backend` and `Abstractions.Emit`, and Svartálfheim's `Primitives`/`Primitives.Ingestion`. The README's chart is the persistence-consumer stencil — deliberately congruent with Himinbjörg's; a future `norse_{context}` realm should render the same shape with the names swapped.

**Doc index** (under `../Glitnir/docs/`):

| Subject | Document |
|---|---|
| Seeding framework (`ISeedContributor` placement law) | `Platform/specs/2026-07-03-seeding-framework-design.md` |
| Tabular ingestion + SeedTool | `Platform/specs/2026-07-04-tabular-ingestion-and-seed-tooling-design.md` |
| UNSD M49 reference data (the live seed case) | `Mimisbrunnr/specs/2026-07-04-unsd-m49-reference-data-design.md` |
| Temporal tables chassis | `Platform/specs/2026-08-04-temporal-tables-persistence-chassis-design.md` |
| Realm's own specs/plans | `Mimisbrunnr/` — spec-first: brainstorm → spec → plan before scaffolding anything new |

## Build & Test

- `dotnet build Mimisbrunnr.slnx` — warnings are errors; a single warning fails.
- `dotnet test Mimisbrunnr.slnx` — xUnit v3 + Shouldly on Microsoft.Testing.Platform. **VSTest `--filter` does NOT work** — use `dotnet test tests/<Project> -- --filter-class "*.<ClassName>"`. `ReferenceTemporalApparatusContainerTests` needs Docker (Testcontainers `postgres:19beta2`).
- SDK pinned by `global.json`: `11.0.100-` prerelease.
- **Migrations CLI:** commands live in README's Migrations CLI section — fully offline, inert placeholder connection strings; every `add`/`remove` auto-refreshes the embedded `schema/norse_reference.sql` (scaffolder-emitted, never hand-edited). **One `InitialCreate` per provider is law** (this realm's spec §7.1): delete the provider's `Migrations/` folder and re-add — never stack. The cost is paid on the dev box: a re-issued `InitialCreate` orphans any database that applied the prior one (`MigrateAsync` dies on `CREATE TABLE region` with "relation already exists", naming neither cause nor cure) — drop `norse_reference` or Bifröst's named `norse-pg-primary` volume; migration plus seed restores full state by construction, which is exactly what buys the squash posture until the preview-7 V1 memorialization.

## Architecture Facts (decided — do not re-litigate)

- **The generated surface is authored in-realm.** Mímir no longer hosts the generator or the `IsoCountryCode` enum — it is wire contracts and serving only, consuming this realm's generated surface and entities by reference. Never reintroduce a companion-repo dependency for something this realm generates.
- **The Mímir split exists for exactly one reason: release cadence.** Verified 2026-07-03 against Ginnungagap's release ceremony (`publish-nuget.yml`): a git tag packs and publishes an entire repo — there is no per-project release scoping. Reference-data content (IANA reissues tzdata multiple times a year; ISO redenominates on its own clock) churns far faster than serving code, so the pair genuinely needs independent cadence. It is **not** the platform's default recommendation — do not copy the split reflexively.
- **SemVer here is a pure schema-contract signal:** patch/minor = new rows or non-breaking additions (Mímir doesn't react); major = an entity or view-model shape change (the one event forcing Mímir to update). Ordinary data freshness (next tzdata, a new currency) is **not** a `Data` release at all — that's Mímir's `Worker` refreshing rows at runtime. TSV + seeder are bootstrap-to-a-known-state; keeping it current is Mímir's job.
- **This pair is the platform's reference-data template** — proof of how little ceremony a bounded context's reference-data need takes on the substrate. Mímisbrunnr stood up `norse_reference` with zero new framework code.
- **Temporal (2), and that is the whole realm** (ruled 2026-08-05): `Region` and `CountryOrArea` → tables `region` and `country_or_area`. ISO/UN canon changes rarely, and the record of *when* it changed is exactly what system-time history is for. There is deliberately no non-temporal side — no secret stores, counters, or prunable runtime state exists here, so nothing rhymes with Himinbjörg's exclusion list. Adoption is `: ITemporalEntity` on two classes plus a re-issued `InitialCreate` — **zero hand-written temporal SQL**; `ReferenceDbContext` inherits `NorseDbContext`, so no context code names the realization hook anywhere.
- **The owned `CountryOrAreaView` jsonb graph takes no marker** — nor do `RegionNode`/`SubregionNode`/`IntermediateRegionNode`. Owned and JSON-mapped types sit outside the temporal contract by chassis validation; the `view` column's contents ride the owner's history like any other column. Both sides are pinned by `ReferenceTemporalModelTests`, including a closing fact that no *other* entity carries the stamp — changing a marker without amending the ruling breaks tests by design. SQL Server's engine-native realization is pinned in `Reference.Data.EntityFramework.Migrations.SqlServer.Tests`, the only place the hook's arrival is observable (Postgres realizes in migration SQL, not the model).
- **Seeding never versions, and that is load-bearing.** An INSERT opens a version; it does not close one. `ReferenceDataSeedContributor` mints zero history rows — the seeded state is the opening version of the record, not churn. The container suite pins that alongside the amendment case (one `ExecuteUpdateAsync` closes exactly one gapless `country_or_area_history` row and leaves `region_history` untouched). **Test cleanup is `TRUNCATE`, never `DELETE`** — a delete mints the very history rows the facts count.
- **Never assert on a migration name** — the squash cycle re-mints ids. Container facts assert on the PostgreSQL catalog (`pg_class`/`pg_proc`/`pg_trigger`), which survives every squash.

## Process

Spec-first, always: brainstorm → spec → plan in `../Glitnir/docs/Mimisbrunnr/`, human greenlight at each transition — everything beyond the M49 seed case (currency, language, script, locale, timezone — the README's ERD sketch) is unconverged; do not scaffold ahead of a converged spec. Implementation is subagent-orchestrated and test-driven: every plan's REQUIRED SUB-SKILL line names `superpowers:subagent-driven-development` (the default; `superpowers:executing-plans` is the narrow separate-session fallback) paired with `superpowers:test-driven-development`. Full rule: `../Glitnir/CLAUDE.md` §2.8.

See `../Bifrost/CLAUDE.md` (§2 The Naming Model) and `../Glitnir/CLAUDE.md` (§3 Bounded Context Map) for the full realm table and how Mímisbrunnr fits the cosmos.
