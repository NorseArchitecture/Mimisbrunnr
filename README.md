# Mímisbrunnr

> Mímisbrunnr — the well of wisdom at Yggdrasil's roots, guarded by Mímir, where Odin traded an eye for a single drink of it.

![Mímisbrunnr — the well of wisdom at Yggdrasil's roots, guarded by Mímir, where Odin traded an eye for a single drink of it](https://github.com/user-attachments/assets/d3af978b-2608-4975-8559-2531b0e87aa1 "Mímisbrunnr — the well Odin paid an eye to drink from")

*Image credit: [@norsemythologyclips](https://www.instagram.com/norsemythologyclips/) — go follow them.*

The reference-data store of the Norse Architecture — **`Norse.ReferenceData.Data`**: entities, view models, TSV seeders (nietras Sep), and EF migrations for canonical external-standard data. First tenants: ISO country codes, ISO currency codes, and IANA time zones. In the dependency chain it rides on Urdarbrunnr's EF foundation and everything below; Mímir rides on it.

## Status

This realm is currently a bare shell — no code, no specs converged yet. Design happens first: brainstorm → spec → plan, recorded in Glitnir's `docs/Mimisbrunnr/`, before any project is scaffolded here.

## Why two repos

Mímisbrunnr and Mímir are one bounded context split across two repositories for a specific, verified reason: reference-data content (IANA reissuing time zone data, ISO adding or redenominating currencies) changes far more often than the service and component code that serves it, and the platform's release tooling only supports repo-scoped tags — packing and publishing happen for an entire repo at once, not per project. Splitting the repository is what lets `Data` cut a release without dragging `Components`/`Web.Server`/`Worker` along, and vice versa. This pair is a template for anyone whose own reference data has the same shape — not a pattern the platform applies by default.

## The cosmos

Mímisbrunnr is one realm of the [Norse Architecture](https://github.com/NorseArchitecture). The whole platform composes at [Bifröst](https://github.com/NorseArchitecture/Bifrost) — clone once, cross the bridge, and every session starts there so decisions get brainstormed across the entire landscape, not in isolation. Every design is tried in [Glitnir](https://github.com/NorseArchitecture/Glitnir), the design court, before code is forged here; this realm's specs and plans will live in the court's [docs/Mimisbrunnr/](https://github.com/NorseArchitecture/Glitnir/tree/master/docs/Mimisbrunnr) once they converge.
