---
area: Eldr/Irradiance
priority: low
---

# Parallel query execution for Irradiance/Albedo

Both are per-cell, embarrassingly parallel: no neighbour reads, output written only to the
entity's own component. `IrradianceSystem.OnUpdate` (commented-out body, see blocker below)
only reads its own cell's `CellParentRefC`-linked planet state + the cell's own geographic
position, writes only its own `IrradianceC` — confirmed by reading the current code, not
assumed. Same shape expected for the not-yet-built `AlbedoSystem`. Textbook fit for Friflo's
`QueryJob`/`ParallelJobRunner` — docs recommend it specifically for "only arithmetic
computations like `* / + - sin(), cos()`", which is exactly `StellarZenithAngle`/`Insolation`
(trig, matrix transforms, no branching on other entities).

Logged from the resource-optimization conversation (`adaptive-tick-scheduling.md`) as a
distinct task — different lever (parallelize one query's execution) than that task's tiering
work (change how often a query runs).

## Blocker

`IrradianceSystem.OnUpdate` is currently **entirely commented out** — blocked on a
cell-id → `LatLng` lookup that doesn't exist yet (`IGrid` has no such method; the `GeoGrid`
referenced in the old code pre-refactor is gone). Per the existing code comment, this is
already flagged as a separate ticket. Nothing to parallelize until that lands and the system
actually does work.

`AlbedoC` exists as a component only — no `AlbedoSystem` yet at all.

## Sub-tasks

1. Wire up the cell-id → `LatLng` lookup (tracked elsewhere, not here) and un-comment
   `IrradianceSystem.OnUpdate` — prerequisite, not part of this task's scope.
2. Convert `IrradianceSystem.OnUpdate` from `Query.ForEachEntity` to `Query.ForEach(...)` +
   `QueryJob.RunParallel()`, with a `ParallelJobRunner` assigned to the `EntityStore`
   (shared instance — docs: thread-safe, reusable across all query jobs, should not be
   created per-call). Reuse the returned `QueryJob` across ticks per docs' recommendation
   (avoids GC allocation on repeated `RunParallel()` calls).
3. Design `AlbedoSystem` (when built) with the same parallel shape from the start, once its
   actual read-set is known (`Reads: _Nothing, external driver for now_` per `concerns.adoc`
   — currently no computation to parallelize either).
4. Benchmark before/after at realistic cell counts (R0 = 122 cells/body × up to ~10 bodies
   ≈ 1220 cells today; revisit if higher-resolution grids are ever used). No structural
   changes in either system's loop body (`ref` writes to own component only) — no
   `CommandBuffer.Synced` needed, avoids the parallel-structural-change slowdown the docs
   warn about.

## Related

`.kanban/2_Doing/adaptive-tick-scheduling.md` — "Future lever: parallel query execution"
note, `docs/concerns.adoc` (Eldr section: Irradiance, Albedo).
