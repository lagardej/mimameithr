---
area: Brunnr/System
priority: medium
---

# Adaptive tick scheduling — decouple sim rate from render rate, per-entity update frequency

`BootTest._Process` drives `_nornir.Advance` + `_bithot.Advance` every render frame, uncapped.
Single `StaggeredSystemGroup` (10s sim-interval) batches Rotation/Position/Irradiance for
all entities regardless of period — a 10s-rotation star and a 30yr-rotation body update at
the same cadence. Wasteful at rest, worse under `TimeCompression`.

## Sub-tasks

1. **Hardcap / decouple sim from render — DONE**
   - `project.godot`: added `[physics] common/physics_ticks_per_second=30`.
   - `BootTest.cs`: `_nornir.Advance` moved to `_PhysicsProcess` (fixed 1/30s delta);
     `_bithot.Advance()` stays in `_Process` at display rate.
   - Chose `_PhysicsProcess` split over `Engine.MaxFps` stopgap — fixed-step sim,
     independent of vsync/render jitter. `docs/architecture.adoc` update still open
     (tradeoff not yet written up there).

2. **Tiered staggering (bucket by period magnitude)**
   - Multiple `StaggeredSystemGroup` instances at different sim-intervals (e.g. 1s, 10s, 1h).
   - At generation phase (Urðr), compute period magnitude per entity from `RotationC`
     period / `OrbitC` period, bucket via `PiecewiseExponentialScale`
     (`Kjarni.Kvasir.Foundation.Scaling`).
   - Route entity's Rotation/Position/Irradiance systems into matching tier at
     `VerðandiSystems.Build`.
   - Static per-entity tier assignment — cheap, coarse, tier boundaries are a guess.

3. **Per-entity adaptive scheduling (alternative/extension to #2)**
   - `NextDueAtC` component: sim-time of next required update, derived from entity's own
     period (e.g. period/360 for ~1° angular step).
   - Query filters entities where `NextDueAt <= clock.ElapsedSeconds`; recompute due time
     after update.
   - No tier-boundary artifacts, true per-entity resolution. Costs a query+compare per tick
     instead of blanket group toggle — benchmark against #2 before committing.

4. **Large sim-delta safety at high `TimeCompression`**
   - `simDelta = deltaTime * compression` — at `Day` compression + 30Hz real tick,
     ~2880 sim-seconds/tick. Single-tier/all-entity-due-every-tick problem gets *worse*
     under compression, not better — makes #2/#3 more necessary, not less.
   - Audit `RotationSystem`, `PositionSystem`, `OrbitalMechanics.cs` for correct
     large-angle wrap/modulo handling (no small-angle assumptions).
   - Consider optional sim-delta clamping (sub-step internally if jump exceeds N periods)
     if mid-compression visual fidelity matters, vs only endpoint state.

5. **Forcings must bypass scheduling delay**
   - Forcings are runtime commands issued mid-tick during the active phase (Verðandi),
     not generation-phase-only — see `forcing-asteroid-sun-crash-e2e.md`. Not yet
     materialized as commands (open dependency, same blocker noted there).
   - Implication for #2/#3: a forcing landing on an entity in a slow tier / with a
     far-future `NextDueAt` must not wait for the entity's next scheduled slot — the
     command's effect (e.g. `AddComponent` overwrite on `PositionC`/`RotationC`) needs
     to be visible immediately, or the forcing appears to silently no-op until the tier
     next fires.
   - Fix shape: forcing application resets the target entity's `NextDueAt` to "now"
     (per-entity adaptive, #3), or forces an out-of-band single-entity update outside
     its tier's accumulator (tiered, #2). Needs a concrete mechanism before either
     scheduling option ships — don't let this surface as another silent-staleness bug
     (cf. `geometrysystem-missed-updates.md`, same failure shape: system doesn't see a
     forcing-driven update because it wasn't watching the right signal).

## Architecture options to weigh

No constraint to preserve `StaggeredSystemGroup`/`ThrottledSystemGroup` shape as-is —
both are internal `Kjarni.Brunnr.System` types, no external contract. Breaking changes
(new group types, changed `Add`/`OnUpdateGroup` signatures, `VerðandiSystems.Build`
restructure) allowed if the tiered/adaptive design calls for it. Don't force-fit #2/#3/#5
into the existing two-group shape if a cleaner model exists.

- Tiered (#2) vs per-entity adaptive (#3): tiered is cheap and matches existing
  `StaggeredSystemGroup` pattern (small diff); per-entity is precise but adds a
  due-time component + per-tick query cost. Start tiered, escalate only if entity
  count/period spread demands it.
- Sim/render decoupling (#1): `_PhysicsProcess` split is the "correct" architecture
  (fixed-step determinism, independent of vsync) but touches `BootTest` structure;
  `Engine.MaxFps` is a one-line stopgap.

## Related

See conversation notes on resource optimization, `TimeCompression` (`Kjarni.Brunnr.Engine.Time`),
`StaggeredSystemGroup`/`ThrottledSystemGroup` (`Kjarni.Brunnr.System`).
