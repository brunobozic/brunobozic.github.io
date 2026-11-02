# QuietHull.Sim — the simulation core

Engine-free C# (`noEngineReferences: true`), deterministic, fixed tick (`SimTime.TicksPerSecond = 10`).
Built test-first: every rule has a test in `Tests/`.

| Piece | What it does |
| --- | --- |
| `HulkGraph` / `HulkGraphBuilder` | Rooms (with ambient noise) and doors. Immutable once built; share it freely. |
| `DoorStates` | Open / Closed (muffles) / Sealed (blocks) per door, per run. |
| `NoisePropagation` | Spreads a sound room by room: −2 per hop, −3 more through a closed door, stopped by a sealed one; each room keeps the loudest path. `HeardAt` subtracts the room's ambient noise (masking). |
| `SenseField` / `SenseRules` | One solver for every sense channel. Sound: −2/hop, −3 more through a closed door, stopped by sealed. Vibration: −1/hop through the hull, doors don't matter. Heat: −3/hop, −2 more through any shut door, never fully blocked. Rooms have ambient levels per channel (masking). |
| `HiveListener` / `SenseWeights` | Where the hive senses from and with what: per-channel percentages, so Crawlers feel the deck, Stalkers see heat. |
| `Emission` | What a working Joe, a cart or a fire gives off on each channel. |
| `PlaceSensor` / `SensorOverlay` | Sentry pups in sensor mode: cover rooms within a hop range; the map overlay shows each channel's level and whether it's masked, and nothing where no pup reads. |
| `AwarenessGauge` | Heard noise fills it, silence drains it; Dormant → Restless (40) → Hunt (80), never back. Fixed-point, so runs replay exactly. |
| `Simulation` | One run: noise sources (working Joes), the hive's listening rooms, doors, the tick, and the event log. One thread per simulation; run many in parallel. |

Run the tests outside Unity: `cd quiet-hull/dotnet/QuietHull.Sim.Tests && dotnet test`.
In Unity: Test Runner ▸ EditMode (the `QuietHull.Sim.Tests` assembly).

## Architecture rules

- **Event-driven:** everything that happens is a `SimEvent` (a struct), appended to the run log and dispatched to
  `ISimEventHandler`s at the end of its tick, in order. UI, audio, telemetry and replays subscribe; nothing polls.
- **Data-oriented swarm (ECS-shaped):** `SwarmStore` keeps aliens as struct-of-arrays, densely packed, with pooled
  slots and generation-checked `AlienHandle`s. Plain value types only, so the layout ports to Unity DOTS
  (Burst jobs over NativeArrays) mechanically if profiling says the swarm needs it. Rendering is Unity's job:
  Entities Graphics or GPU instancing, reading a per-frame snapshot.
- **Pools, not allocations:** the tick allocates nothing in steady state (a test proves it with
  `GC.GetAllocatedBytesForCurrentThread`). Noise solving reuses scratch buffers; pools grow by doubling only.
- **Telemetry by interface:** `IRunTelemetry` gets per-tick measurements for the fun gates; the Serilog /
  OpenTelemetry adapter lives outside the core, so the core stays dependency-free and Burst-friendly.
- **Determinism:** fixed tick, integer and fixed-point maths, ordered iteration. Same inputs, same run.
- **One simulation, one thread:** no locks in the tick. Scale by running many simulations in parallel (the bots).
