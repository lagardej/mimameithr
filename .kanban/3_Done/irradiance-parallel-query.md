---
area: Eldr/Irradiance
priority: low
---

# Parallel query execution for Irradiance

Per-cell, embarrassingly parallel: no neighbour reads, output written only to the entity's own
component. `IrradianceSystem.OnUpdate` only reads its own cell's `CellParentRefC`-linked planet
state + the cell's own geographic position, writes only its own `IrradianceC` — confirmed by
reading the current code, not assumed. Textbook fit for Friflo's `QueryJob`/`ParallelJobRunner`
— docs recommend it specifically for "only arithmetic computations like `* / + - sin(), cos()`",
which is exactly `StellarZenithAngle`/`Insolation` (trig, matrix transforms, no branching on
other entities).

Logged from the resource-optimization conversation (`adaptive-tick-scheduling.md`) as a
distinct task — different lever (parallelize one query's execution) than that task's tiering
work (change how often a query runs).

Albedo dropped from scope: needs surface-composition/terrain data that won't exist for a long
time (`AlbedoC` is component-only, no driver, no `AlbedoSystem`). Revisit as its own ticket once
terrain lands.

## Status

`IrradianceSystem.OnUpdate` was blocked on a cell-id → `LatLng` lookup. That lookup already
existed (`IGeodesicGrid.CenterOf(CellId)`, implemented by `BifrostKart`, reachable via
`GridProvider.Get(geometry.GridShape)`) — stale code comment claimed otherwise and pointed to
a non-existent separate ticket. Wired up and `OnUpdate` un-commented. No longer blocked.

## Sub-tasks

1. ~~Wire up cell-id → `LatLng` lookup, un-comment `IrradianceSystem.OnUpdate`~~ — done.
2. ~~Convert `IrradianceSystem.OnUpdate` from `Query.ForEachEntity` to `Query.ForEach(...)` +
   `QueryJob.RunParallel()`~~ — done. `ParallelJobRunner` now owned by `BrunnrEngine` (shared-store
   path) and by `Nornir`'s private-store constructor (headless path), both `IDisposable`,
   `QueryJob` cached on the system instance and reused across ticks.
3. Benchmark before/after at realistic cell counts (R0 = 122 cells/body × up to ~10 bodies
   ≈ 1220 cells today; revisit if higher-resolution grids are ever used). No structural
   changes in either system's loop body (`ref` writes to own component only) — no
   `CommandBuffer.Synced` needed, avoids the parallel-structural-change slowdown the docs
   warn about.
   **Added** `Kjarni/Nornir.Tests/IrradianceParallelBenchmark.cs` — xunit `[Theory]`, 10 and
   100 synthetic bodies (1220 / 12200 cells), sequential (`query.Chunks` foreach) vs parallel
   (real `IrradianceSystem` via `SystemRoot`) timed with `Stopwatch`, one warm-up tick each.
   Both paths call the same public `IrradianceMath.StellarZenithAngle`/`Insolation` (moved out
   of `IrradianceSystem` into their own static class, colocated in the domain folder on the
   model of `OrbitalMechanics.cs`) so only threading differs. Not a regression test — logs
   timings via `ITestOutputHelper`, no pass/fail threshold.

   **Results** (8 cores):

   | bodies | cells | sequential | parallel | speedup |
   |---|---|---|---|---|
   | 10 | 1220 | 0.965 ms | 0.968 ms | 1.00x |
   | 100 | 12200 | 9.795 ms | 3.152 ms | 3.11x |

   At today's realistic scale (1220 cells) parallel overhead cancels the gain — a wash, not a
   regression. At 10x that scale the parallel path wins —3x, sub-linear on 8 cores rather than
   ~8x, consistent with the per-cell work being dominated by `Entity.GetComponent<T>` reads on
   the shared planet entity (random access, not pure arithmetic) rather than the trig itself.
   Conclusion: the conversion is correct and free at current scale, and pays off if/when cell
   counts grow (higher-resolution grids, more bodies). No action needed unless resolution
   increases.

## Related

`.kanban/2_Doing/adaptive-tick-scheduling.md` — "Future lever: parallel query execution"
note, `docs/concerns.adoc` (Eldr section: Irradiance, Albedo).
