# Quiet Hull — Unity data layer (prologue, missions 2 and 3, tarot rollout 1–10)

Drop `Assets/QuietHull/` into the Unity 6 (URP) project. C# 9, no .NET 8 APIs in runtime code.

## What's here

| Assembly | Folder | Contents |
| --- | --- | --- |
| `QuietHull.Data` (runtime) | `Data/Runtime` | ScriptableObject definitions: items, weapons, tools, cargo, key items, sentries, units, aliens, augments (8 slots), specializations, Omen cards (with the rarity math), shuttle modules, market offers, contracts, recipes, workstations, room ambience, hazards, room treatments, fixtures, loot caches, waves, mission beats and scripts, announcements, and the `DefinitionCatalog` with a thread-safe id lookup. `RarityMath` and `StatMath` are pure helpers. No third-party dependencies. |
| `QuietHull.Data.Editor` | `Data/Editor` | Content builders (prologue, Cold Sleep, the Tremolino, augments, ordnance, factions, the sun/belt sample: every value in one place), the asset generator, catalog validation, Serilog + OpenTelemetry wiring. |
| `QuietHull.Data.Tests.Editor` | `Data/Tests/Editor` | Edit-mode tests that check the design doc's tuning targets: the rarity matrix, ammo per trip, masking, material totals, rupture and freeze times, the hub economy, specialization balance. |

## Setup

1. Install [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) and add:
   - `Serilog` (3.1+)
   - `System.Diagnostics.DiagnosticSource` (8.0+; provides `ActivitySource` for OpenTelemetry spans)
2. Make sure the Test Framework package is installed (Window ▸ Package Manager ▸ Test Framework).
3. **Quiet Hull ▸ Content ▸ Generate Prologue**, **Generate Mission 2 (Cold Sleep)** and **Generate Mission 3 (The Tremolino)** write everything under
   `Assets/QuietHull/Content/`. **Generate Tarot Rollout (Missions 1-10)** adds the Omen schedule (slots, drops,
   rarity ceilings, the Cartomancer's table, the bench), the 16 Major and 5 Minor cards of missions 4–10, the Anchorite,
   the Cartomancer's rules (card levels, ascending, mutations, corruption) and the 22 Catalysts (incl. the card-targeting Beacon, Sigil and Echo). Running them again updates assets in place (GUIDs stay stable); shared
   definitions (materials, sentries, aliens) stay single assets.
4. **Quiet Hull ▸ Validate All Catalogs** validates every catalog in the project.
5. Window ▸ General ▸ Test Runner ▸ EditMode ▸ Run All.

## Telemetry

- **Logs:** Serilog, enriched with Application, UnityVersion, ProductName, Platform, MachineName, ProcessId,
  ThreadId, TraceId, SpanId, ParentSpanId, Operation, plus per-call context (Tool, RootFolder, CatalogId,
  DefinitionId, DefinitionType, Field, AssetPath). Output goes to the Unity console now; add
  `Serilog.Sinks.Graylog` in `EditorTelemetry.CreateLogger` to ship to Graylog.
- **Spans:** `ActivitySource("QuietHull.Editor.Data")` with `quiethull.*` tags
  (definition id/type, asset path and action, catalog id, counts, validation results) and OTEL exception tags.
  Add the OpenTelemetry SDK with an OTLP exporter to send them to Jaeger; until then `SpanLogListener`
  logs each finished span with its duration.

## Conventions

- Ids are `lower_snake_case` with a prefix: `itm_`, `mat_`, `rcp_`, `bp_`, `ws_`, `unit_`, `alien_`, `enemy_`, `amb_`,
  `hz_`, `rt_`, `fx_`, `cache_`, `aug_`, `spec_`, `omen_`, `mod_`, `offer_`, `con_`, `wave_`, `beat_`, `ann_`, `mission_`, `cat_`.
- One ScriptableObject class per file, named after the class (Unity needs this for asset references).
- Definitions are read-only at runtime (internal serialized fields, public getters), so the sim can read
  them from any thread once loaded.
- Change numbers in the content builders, regenerate, run the tests.
