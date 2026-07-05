# CLAUDE.md — Mimisbrunnr (`Norse.ReferenceData.Data`)

## 0. Wrong Root — Halt

If you are reading this because **Mimisbrunnr itself is the Claude Code session root** — someone ran `claude` from inside this directory instead of `../Bifrost` — stop here. Do not read further, do not propose changes, do not run anything.

Tell the user: every Norse Architecture session starts from **Bifrost**. Org-wide settings (the `superpowers` plugin, permission rules) only apply when Bifrost is the actual session root — Claude Code never merges a submodule's own `.claude/settings.json` into a parent-launched session. Exit, `cd ../Bifrost`, and run `claude` there instead.

This repo's own `.claude/settings.json` carries a `SessionStart` hook that should already have blocked this session before this file was ever read. If you're reading this anyway, hooks were bypassed, disabled, or failed — halt regardless; this rule does not depend on the hook to hold.

---

> **Do not commit, push, or rewrite git history.** Stage edits (`git add`), show the diff, and stop — the human reviews and commits.

> **Use US English spelling** in code, identifiers, comments, docs, and commit/PR copy.

## 1. What This Repository Is

Mimisbrunnr is **the reference-data store** — `Norse.ReferenceData.Data`: entities, view models, TSV seeders (nietras Sep — AOT-friendly), and EF migrations for canonical external-standard data. First tenants: ISO country codes, ISO currency codes, IANA time zones; not an exhaustive list. In the dependency chain it rides on Urdarbrunnr's EF foundation and everything below, same as every EF-backed realm. It ships as a pure NuGet classlib, spun up from Yggdrasil's composition root — no standalone host, matching the shape the migrations framework already established (declare in a realm, host in Yggdrasil, compose in Bifröst).

**Mímir is the companion repository** — `Norse.ReferenceData.Components` / `.Web.Server` / `.Worker`: Blazor components, the gRPC service host, and the background worker that keeps data current. The two are split into separate repositories deliberately, not by default. Verified 2026-07-03 against Ginnungagap's `release-nuget.yml`: a git tag triggers `dotnet pack` at repo root and publishes every packable project in that repo together — there is no per-project release scoping. Reference-data content (IANA reissues tzdata multiple times a year; ISO adds or redenominates currencies on its own clock) churns far more often than service or component code, so this pair genuinely needs independent release cadence. That is the *only* reason for the split — it is not the platform's default recommendation for every bounded context, and should not be copied reflexively.

**The interop boundary is the entities and view models in this repository**, and SemVer here is a pure schema-contract signal:
- **Patch/minor** — a new migration adding rows, or a non-breaking addition to an entity or view model. Mímir does not have to react.
- **Major (1.x → 2.x)** — an entity or view model shape actually changes. That is the one event that forces `Components`/`Web.Server`/`Worker` to update.

Ordinary data-freshness updates (the next tzdata release, a new ISO currency) are **not** a reason to cut a `Data` release at all — that is Mímir's `Worker` refreshing rows at runtime against the existing schema. TSV + seeder here are bootstrap-to-a-known-state; keeping that state current afterward is Mímir's job.

**This pair is the platform's reference-data template.** It exists to show how little ceremony a domain-specific reference-data need takes to stand up on the substrate, and to give anyone building their own bounded context's reference-data seeding a working pattern to point at — not a dependency anyone is obligated to take. See the seeding-framework spec (`../Glitnir/docs/Platform/specs/2026-07-03-seeding-framework-design.md`, shipped — Asgard/Midgard/Urdarbrunnr all merged): `ISeedContributor` lives inside the existing `.Migrations` project, no new assemblies.

Named for the well of wisdom at Yggdrasil's roots, guarded by Mímir, where Odin traded an eye for a single drink of it — a deliberate pair with Urdarbrunnr's Well of Urð. Both are foundational wells at the tree's roots; Urdarbrunnr holds the record of what has happened (the migrations chassis itself), Mimisbrunnr holds what is known (external canon this platform doesn't get to redefine).

**This repository is no longer a bare shell.** The first seed case — UN M49 reference data — has its raw-source-to-TSV conversion tooling live: `tools/SeedTool` (dev-only console app, never packed or AOT-published) converts `seeds/raw/UNSD — Methodology.csv` into the committed `seeds/region.tsv`/`seeds/country-or-area.tsv`, consuming Svartalfheim's `Norse.Primitives.Ingestion` (`../Glitnir/docs/Platform/specs/2026-07-04-tabular-ingestion-and-seed-tooling-design.md`). The EF entities, migration, and seed contributor that actually load these TSVs (`../Glitnir/docs/Mimisbrunnr/specs/2026-07-04-unsd-m49-reference-data-design.md`, approved, not yet planned/coded) are the next slice. Everything beyond this first seed case remains unconverged — brainstorm → spec → plan, recorded in `../Glitnir/docs/Mimisbrunnr/`, per the org's spec-first discipline, before any further project is scaffolded here. When that plan is written, its REQUIRED SUB-SKILL line names `superpowers:subagent-driven-development` as the default (not a recommendation among equals — `executing-plans` is the narrow fallback for separate-session review checkpoints) paired with `superpowers:test-driven-development` — implementation here is subagent-orchestrated and test-driven, never one without the other (`../Glitnir/CLAUDE.md` §2.8).

See `../Bifrost/CLAUDE.md` (§2 The Naming Model) and `../Glitnir/CLAUDE.md` (§3 Bounded Context Map) for the full realm table and how Mimisbrunnr fits the rest of the cosmos.
