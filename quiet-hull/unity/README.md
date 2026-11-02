# Quiet Hull — Unity data layer (prologue)

Drop `Assets/QuietHull/` into the Unity 6 (URP) project. C# 9, no .NET 8 APIs in runtime code.

## What's here

| Assembly | Folder | Contents |
| --- | --- | --- |
| `QuietHull.Data` (runtime) | `Data/Runtime` | ScriptableObject definitions: items, weapons, tools, cargo, sentries, units, aliens, recipes, workstations, room ambience, loot caches, waves, mission beats, mission scripts, and the `DefinitionCatalog` with a thread-safe id lookup. No third-party dependencies. |
| `QuietHull.Data.Editor` | `Data/Editor` | `PrologueContentBuilder` (every prologue value, one place), the asset generator, catalog validation, Serilog + OpenTelemetry wiring. |
| `QuietHull.Data.Tests.Editor` | `Data/Tests/Editor` | Edit-mode tests that check the design doc's tuning targets (ammo left after the stream, masking per room, material totals, rupture times, alloy value). |

## Setup

1. Install [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) and add:
   - `Serilog` (3.1+)
   - `System.Diagnostics.DiagnosticSource` (8.0+; provides `ActivitySource` for OpenTelemetry spans)
2. Make sure the Test Framework package is installed (Window ▸ Package Manager ▸ Test Framework).
3. **Quiet Hull ▸ Prologue ▸ Generate Data Assets** writes everything to `Assets/QuietHull/Content/Prologue/`.
   Running it again updates assets in place (GUIDs stay stable).
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

- Ids are `lower_snake_case` with a prefix: `itm_`, `mat_`, `rcp_`, `bp_`, `ws_`, `unit_`, `alien_`, `amb_`,
  `cache_`, `wave_`, `beat_`, `mission_`, `cat_`.
- One ScriptableObject class per file, named after the class (Unity needs this for asset references).
- Definitions are read-only at runtime (internal serialized fields, public getters), so the sim can read
  them from any thread once loaded.
- Change prologue numbers in `PrologueContentBuilder`, regenerate, run the tests.
