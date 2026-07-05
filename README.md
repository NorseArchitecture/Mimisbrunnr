# Mímisbrunnr

> Mímisbrunnr — the well of wisdom at Yggdrasil's roots, guarded by Mímir, where Odin traded an eye for a single drink of it.

![Mímisbrunnr — the well of wisdom at Yggdrasil's roots, guarded by Mímir, where Odin traded an eye for a single drink of it](https://github.com/user-attachments/assets/d3af978b-2608-4975-8559-2531b0e87aa1 "Mímisbrunnr — the well Odin paid an eye to drink from")

*Image credit: [@norsemythologyclips](https://www.instagram.com/norsemythologyclips/) — go follow them.*

The reference-data store of the Norse Architecture — **`Norse.ReferenceData.Data`**: entities, view models, TSV seeders (nietras Sep), and EF migrations for canonical external-standard data. First tenants: ISO country codes, ISO currency codes, and IANA time zones. In the dependency chain it rides on Urdarbrunnr's EF foundation and everything below; Mímir rides on it.

## Status

The first seed case — UN M49 reference data (`Region`/`CountryOrArea`) — has its raw-source-to-TSV conversion tooling live: `tools/SeedTool` (a dev-only console app, never packed or AOT-published) reads `seeds/raw/UNSD — Methodology.csv` via Svartalfheim's `Norse.Primitives.Ingestion` and produces the curated `seeds/region.tsv`/`seeds/country-or-area.tsv`, both committed as this realm's real seed data. The EF entities, migration, and seed contributor that will actually load these TSVs into `norse_referencedata` are not yet built — that's the next slice, specced in Glitnir's `docs/Mimisbrunnr/` but not yet planned/coded. Everything beyond this first seed case (currency, language, script, locale, timezone — see the ERD sketch below) remains unconverged; design happens first: brainstorm → spec → plan, recorded in Glitnir's `docs/Mimisbrunnr/`, before any further project is scaffolded here.

## Initial sketch of reference data plan
```mermaid
erDiagram
  region {
    char(3) un_m49_code PK
    varchar name
    char(3) parent_m49 FK
    smallint level
  }

  country {
    char(2) iso_3166_1_alpha2 PK
    char(3) iso_3166_1_alpha3
    char(3) iso_3166_1_numeric
    char(3) un_m49_code FK
    varchar common_name
    varchar official_name
    bool is_independent
    bool is_active
  }

  subdivision {
    varchar iso_3166_2_code PK
    char(2) country_alpha2 FK
    varchar name
    varchar type
  }

  language {
    char(3) iso_639_3_code PK
    char(2) iso_639_1_code
    varchar name
    char(4) iso_15924_script FK
    varchar scope
    varchar lang_type
  }

  script {
    char(4) iso_15924_code PK
    varchar name
    smallint unicode_number
    varchar unicode_alias
  }

  locale {
    varchar bcp47_tag PK
    char(3) language_code FK
    char(4) script_code FK
    char(2) country_alpha2 FK
    varchar display_name
    varchar number_format
    varchar date_format
    varchar first_day_of_week
  }

  currency {
    char(3) iso_4217_code PK
    varchar name
    varchar symbol
    smallint minor_unit
    bool is_active
  }

  country_currency {
    char(2) country_alpha2 FK
    char(3) currency_code FK
    bool is_primary
  }

  country_language {
    char(2) country_alpha2 FK
    char(3) language_code FK
    varchar status
  }

  timezone {
    varchar iana_key PK
    varchar canonical_key FK
    char(2) country_alpha2 FK
    varchar region_hint
    bool is_canonical
  }

  utc_offset {
    varchar iana_key FK
    varchar offset_code
    smallint offset_minutes
    varchar abbreviation
    bool is_dst
    date valid_from
    date valid_until
  }

  calling_code {
    serial id PK
    char(2) country_alpha2 FK
    varchar e164_prefix
    bool is_primary
  }

  region ||--o{ region : "parent"
  region ||--o{ country : "contains"
  country ||--o{ subdivision : "has"
  country ||--o{ country_currency : "uses"
  country ||--o{ country_language : "speaks"
  country ||--o{ timezone : "observes"
  country ||--o{ calling_code : "assigned"
  currency ||--o{ country_currency : "used by"
  language ||--o{ country_language : "spoken in"
  language }o--|| script : "written in"
  script ||--o{ locale : "used by"
  language ||--o{ locale : "defines"
  country ||--o{ locale : "applies"
  timezone ||--o{ utc_offset : "has rules"
  timezone }o--o| timezone : "alias of"
```

## Why two repos

Mímisbrunnr and Mímir are one bounded context split across two repositories for a specific, verified reason: reference-data content (IANA reissuing time zone data, ISO adding or redenominating currencies) changes far more often than the service and component code that serves it, and the platform's release tooling only supports repo-scoped tags — packing and publishing happen for an entire repo at once, not per project. Splitting the repository is what lets `Data` cut a release without dragging `Components`/`Web.Server`/`Worker` along, and vice versa. This pair is a template for anyone whose own reference data has the same shape — not a pattern the platform applies by default.

## The cosmos

Mímisbrunnr is one realm of the [Norse Architecture](https://github.com/NorseArchitecture). The whole platform composes at [Bifröst](https://github.com/NorseArchitecture/Bifrost) — clone once, cross the bridge, and every session starts there so decisions get brainstormed across the entire landscape, not in isolation. Every design is tried in [Glitnir](https://github.com/NorseArchitecture/Glitnir), the design court, before code is forged here; this realm's specs and plans will live in the court's [docs/Mimisbrunnr/](https://github.com/NorseArchitecture/Glitnir/tree/master/docs/Mimisbrunnr) once they converge.

## Soundtrack: Óðinn við Mímis brunn (Odin at the Well of Mimir)
[![Soundtrack: Óðinn við Mímis brunn (Odin at the Well of Mimir)](https://img.youtube.com/vi/luOqIm__-l8/maxresdefault.jpg)](https://www.youtube.com/watch?v=luOqIm__-l8)
