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

0. **Bugfix (prerequisite) — DONE**
   - `RotationSystem`/`PositionSystem` used `Tick.time` (absolute sim-clock time) instead of
     `Tick.deltaTime` when advancing state, while feeding the previous tick's already-advanced
     value back in as the "epoch" input — compounded error every fire (call 2 at t=20s produced
     `θ0+rate·30` instead of `θ0+rate·20`). Masked today by the single 10s tier; would compound
     differently per tier once #2 lands, so fixed first.
   - Fixed: both systems now use `Tick.deltaTime`. Renamed misleading `epochAngle`/
     `initialMeanAnomaly` params to `previousAngle`/`previousMeanAnomaly` in
     `RotationSystem.CurrentAngle` and `OrbitalMechanics.CurrentMeanAnomaly` — docs now match
     actual incremental (not epoch-relative) semantics.
   - No existing test covered this (`Nornir.Tests` has no multi-tick `RotationSystem`/
     `PositionSystem` coverage) — worth a follow-up test asserting angle correctness across
     multiple non-uniform-interval `Advance()` calls, not filed as separate ticket yet.

0b. **Bugfix: `StaggeredSystemGroup` first-fire delay — DONE**
   - `initialAccumulator = -_offset * _members.Count` made each member wait `interval + offset·i`
     for its *first* fire (10s, 11s, 12s with the default 10s/1s config) instead of firing quickly
     at `offset·i` (0s, 1s, 2s) and settling into the `interval` cadence afterward. Entities sat on
     stale generation-phase state for up to a full interval after boot.
   - Fixed: per-member `Threshold` field, starts at `offset·index`, switches to `_interval` after
     the first fire. Test: `Kjarni.Brunnr.Tests.StaggeredSystemGroupTests` — tick step (0.5s) must
     be finer than the offset (1s) or successive members' thresholds collapse onto the same tick
     and the stagger can't be observed (first test draft hit exactly this, red on `A[0]`).
   - `ThrottledSystemGroup` has the same shape (`_accumulator = -offset`) — same bug, likely, but
     it's currently unused anywhere in the codebase. Not fixed here; fix if/when it's adopted.

1. **Hardcap / decouple sim from render — DONE**
   - `project.godot`: added `[physics] common/physics_ticks_per_second=30`.
   - `BootTest.cs`: `_nornir.Advance` moved to `_PhysicsProcess` (fixed 1/30s delta);
     `_bithot.Advance()` stays in `_Process` at display rate.
   - Chose `_PhysicsProcess` split over `Engine.MaxFps` stopgap — fixed-step sim,
     independent of vsync/render jitter.

2. **Tiered staggering (bucket by period magnitude) — DONE, pending test run**
   - 3 tags: `FastTierTag`/`MediumTierTag`/`SlowTierTag` (`Kjarni.Brunnr.System.UpdateTier`).
   - `UpdateTiering.TagFor(Duration)`: <60s fast, <1day medium, else slow. Interval per tier:
     0.5s / 10s / 600s. Thresholds/intervals are guesses, tunable.
   - `SetRotationHandler`/`SetPositionHandler` tag entity at generation time, from rotation
     period / derived `OrbitC.OrbitalPeriod`.
   - `RotationSystem`/`PositionSystem` gained `(Tags tier)` ctor overload — `Filter.AnyTags(tier)`.
     Parameterless ctor kept (unused now, harmless).
   - `VerðandiSystems.Build`: 3 `StaggeredSystemGroup`s (Fast/Medium/Slow), each with tagged
     Rotation+Position. `IrradianceSystem` stays untagged in Medium tier only — it queries cells,
     not the tiered planet components, not tiered itself.
   - Same tag types shared across Rotation+Position domains, no conflict — `AnyTags` filter, entity
     can carry both a fast rotation tag and a slow orbit tag simultaneously, each system only cares
     about its own tag being present.
   - Known wart: `StaggerOffset=1f` shared across all 3 tiers — in Fast tier (interval 0.5s),
     `PositionSystem`'s first-fire threshold (offset·1=1s) exceeds the interval itself, delaying
     its first fire slightly past where Rotation's (threshold 0) fires. Cosmetic, not filed.
   - Tests: `Kjarni.Brunnr.Tests.UpdateTieringTests` (threshold boundaries). Existing
     `RotationSystemTests`/`StaggeredSystemGroupTests` still valid, re-verified by hand that
     correctness holds regardless of which tier's interval an entity lands in.
   - Not run yet on your end — `dotnet test` when ready.

3. **Per-entity adaptive scheduling (alternative/extension to #2)**
   - `NextDueAtC` component: sim-time of next required update, derived from entity's own
     period (e.g. period/360 for ~1° angular step).
   - Query filters entities where `NextDueAt <= clock.ElapsedSeconds`; recompute due time
     after update.
   - No tier-boundary artifacts, true per-entity resolution. Costs a query+compare per tick
     instead of blanket group toggle — benchmark against #2 before committing.
   - Weakness: full scan every tick just to check the branch, even for entities nowhere near
     due — tiered's group-level `Enabled = false` skips the entire query invocation for
     inactive tiers (zero touch); this touches every entity every tick, just cheaper per-touch.

3b. **Per-entity scheduling via priority queue — considered, not recommended over #3b-index**
   - Prior engine iteration: each system schedules a job per entity with a `DueAt`; a central
     scheduler sorts by `DueAt` (`PriorityQueue<T,TPriority>`, .NET min-heap) and dispatches
     only entities actually due — `O(log n)` pop per due entity instead of `O(n)` scan every tick.
   - Rejected: bypasses Friflo's `QuerySystem`/archetype batching entirely. Popping scattered
     entity IDs off a heap and doing `store.GetEntityById(id).GetComponent<T>()` per job is
     random access, one at a time — loses the cache-locality/SIMD value Friflo's contiguous
     component arrays provide. Would sit beside `SystemRoot` as a second, parallel dispatch
     path, not inside it.
   - Also needs manual lazy-invalidation for forcings (no decrease-key in `PriorityQueue`):
     job carries the due-time it expected; on pop, compare against the component's current
     due-time, skip if stale (superseded by a forcing rescheduling sooner). Workable but adds
     a footgun the index-based approach below doesn't have.

3c. **Per-entity scheduling via Friflo Component Index — preferred over 3b if #3 is pursued**
   - Friflo has this natively: `IIndexedComponent<TValue>` + `store.Query().ValueInRange<T,TValue>(min, max)`.
     Returns a real batched query result (`ForEachEntity`), not scattered lookups — keeps the
     ECS's cache-locality model intact, unlike 3b.
   - Shape: one indexed component per domain (`RotationDueAtC`, `PositionDueAtC` — not one
     shared type, keeps dispatch simple, mirrors existing per-domain system separation), each
     with its own `BaseSystem` (needs direct `Store` access, not `QuerySystem`) running
     `Store.Query().ValueInRange<RotationDueAtC, double>(double.NegativeInfinity, now)` and
     `ForEachEntity` over the result.
   - Forcings (#5) become close to free: `entity.AddComponent(new RotationDueAtC { DueAt = now })`
     reschedules immediately — the index *is* the live schedule, no stale-job invalidation
     needed (unlike 3b).
   - Two real costs, not hand-waved:
     1. Updating the indexed value **must** go through `entity.AddComponent<>()`, not `ref` —
        docs are explicit that `ref` mutation silently skips the index update. ~10x the cost of
        a plain component write, and a different mutation pattern than `RotationSystem`/
        `PositionSystem` use today (easy to get wrong by habit / during review).
     2. Duplicate-value pileup: insert/remove is `O(N)` in the number of entities sharing the
        *exact same* indexed value; docs recommend staying under ~100 duplicates. Entities all
        initialized to `DueAt = 0` at boot is the obvious trap — needs per-entity jitter
        (e.g. `+ (entityId % 1000) * 1e-6`) to spread ties.
   - `ValueInRange` is `O(N log N)` in unique due-values *within range*, not total entity
     count — genuinely sparse dispatch, addresses #3's "full scan every tick" weakness,
     assuming duplicate pileup (above) is avoided.
   - Still: benchmark against #2 before committing, per #3's own caveat. #2 already ships and
     covers the motivating case (star vs 30yr rotator); only build this if profiling shows #2's
     coarse tier buckets cost something real at target entity counts.

4. **Large sim-delta safety at high `TimeCompression` — DONE (audit only, no fix needed)**
   - `simDelta = deltaTime * compression` — at `Day` compression + 30Hz real tick,
     ~2880 sim-seconds/tick. Single-tier/all-entity-due-every-tick problem gets *worse*
     under compression, not better — mitigated by #2 (tiered staggering), already shipped.
   - Audited `RotationSystem`/`OrbitalMechanics.CurrentMeanAnomaly` for large-angle wrap
     safety: both use `(previousValue + rate·elapsed) % 360.0` with `previousValue` always
     fed back already wrapped to `[0°,360°)`. Only the current tick's contribution can be
     large (~1e5° max at Day compression/30Hz), nowhere near double-precision limits (~15-17
     significant digits) — no accumulating error, no overflow, no small-angle assumption.
     Already correct by construction, no change made.
   - `CurrentMeanAnomaly`'s wrapped output also bounds the input to `MeanToTrueAnomaly`'s
     Kepler solver regardless of compression.
   - Found but out of scope: `MeanToTrueAnomaly` runs a fixed 10 Newton-Raphson iterations,
     no convergence check — fine for low/moderate eccentricity, may under-converge for
     `e` close to 1. Independent of `TimeCompression` (same risk at any tick rate/compression),
     not filed as a ticket yet.

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

## Future lever: parallel query execution (not needed yet)

Friflo's `QueryJob` + `ParallelJobRunner` (multi-core query execution, see
[Query Optimization](https://friflo.gitbook.io/friflo.engine.ecs/documentation/query-optimization))
is a good fit for `RotationSystem`/`PositionSystem` specifically — docs recommend it for
"only arithmetic computations like `* / + - sin(), cos()`", which is exactly what
`CurrentAngle`/Kepler-solve/trig are. Orthogonal to #2: tiering cuts *how often* a tier's
query runs, parallelizing cuts *how long* one run takes — they compose.

Not worth adding now — per-tier entity counts likely don't justify thread-dispatch overhead
yet, and `QueryJob` needs to be reused across ticks to avoid GC allocation. Revisit if a tier's
entity count grows large (asteroid fields, particle-like bodies), gated on profiling — same
"benchmark before committing" posture as #3/#3c.

Strike against 3c (Component Index) specifically if that's ever picked up: updating an indexed
field requires `entity.AddComponent<>()` (a structural change), which under parallel execution
needs `CommandBuffer.Synced` — and the docs' own caveat is that structural changes under
parallel execution tend to be *slower* single-threaded due to cache contention on random memory
access. #2's plain `ref` writes parallelize cleanly; 3c's indexed writes don't.

