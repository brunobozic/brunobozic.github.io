# QuietHull.Sim — the simulation core

Engine-free C# (`noEngineReferences: true`), deterministic, fixed tick (`SimTime.TicksPerSecond = 10`).
Built test-first: every rule has a test in `Tests/`.

| Piece | What it does |
| --- | --- |
| `HulkGraph` / `HulkGraphBuilder` | Rooms (with ambient noise) and doors. Immutable once built; share it freely. |
| `DoorStates` | Open / Closed (muffles) / Sealed (blocks) per door, per run. |
| `NoisePropagation` | Spreads a sound room by room: −2 per hop, −3 more through a closed door, stopped by a sealed one; each room keeps the loudest path. `HeardAt` subtracts the room's ambient noise (masking). |
| `AwarenessGauge` | Heard noise fills it, silence drains it; Dormant → Restless (40) → Hunt (80), never back. Fixed-point, so runs replay exactly. |
| `Simulation` | One run: noise sources (working Joes), the hive's listening rooms, doors, the tick, and the event log. One thread per simulation; run many in parallel. |

Run the tests outside Unity: `cd quiet-hull/dotnet/QuietHull.Sim.Tests && dotnet test`.
In Unity: Test Runner ▸ EditMode (the `QuietHull.Sim.Tests` assembly).
