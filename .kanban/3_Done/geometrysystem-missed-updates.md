---
area: Skald/Bithot
---

# GeometrySystem misses forcing-driven updates

`GeometrySystem` (`Skald/Bithot/Geimr/Geometry/System.cs`) detects
dirty entities via `EntityStore.EventRecorder` +
`query.EventFilter.ComponentAdded<GeometryC>()` /
`ComponentRemoved<GeometryC>()` — structural events only.

Friflo distinguishes three component-change actions (confirmed via
Friflo docs, `Events` page): `Add`, `Update` (value changed on an
already-present component), `Remove`. `EventFilter` only exposes
add/remove filtering. `Update` is only observable via the separate
push-based `entity.OnComponentChanged` / `store.OnComponentChanged`
handler subscription.

Once forcings are materialized as commands (confirmed direction —
see `boottest-root-node-active-phase.md`), a forcing that alters an
existing body's `GeometryC` (e.g. radius) will call
`entity.AddComponent(new GeometryC {...})` on an entity that already
has the component — an `Update`, not an `Add`. `GeometrySystem`
won't see it. `VisualRadiusC` silently goes stale, permanently, for
any body whose geometry changes after generation.

## Root cause note

`bithot-render-store.md` (Done) originally specced
`store.OnComponentChanged` (push-based, correctly covers `Update`)
for exactly this sync problem. The implemented `GeometrySystem` used
`EventRecorder`/`EventFilter` instead (poll-based, add/remove only)
— implementation drifted from its own design doc; the drift is the
bug.

## Open question — resolved

Two hypotheses were proposed:

1. `EventFilter.ComponentAdded<T>()` only sees genuine first-attach
   — `AddComponent` on an already-present component fires `Update`,
   invisible to it. Gap is real.
2. `AddComponent` fires `Add` structurally regardless of whether the
   component already existed — the `Add`/`Update`/`Remove` action
   distinction only applies to the separate `OnComponentChanged`
   push subscription, not to what `EventFilter` tracks. No gap;
   `GeometrySystem` already catches forcing-driven overwrites.

Confirmed by test (`Kjarni/Brunnr.Tests/EventFilterTests.cs`,
`ComponentAdded_FiresOnAddOverExisting`): add component, clear
events, `AddComponent` again on the same entity (an `Update`),
assert `query.HasEvent()` — passes green. **Hypothesis (2) holds.**
`EventFilter.ComponentAdded<T>()` fires on `AddComponent` regardless
of prior presence. `GeometrySystem` already catches forcing-driven
geometry overwrites. No gap.

## Fix

Not needed — closing as no-bug. `GeometrySystem` stays on
`EventRecorder`/`EventFilter` as implemented. No
`OnComponentChanged` migration required.

Not needed for `Skald.Bithot.Geimr.Position.PositionSystem` — that
one always full-resyncs every entity every `Advance()` call, no
event filter involved, so it has no equivalent gap. Any *new*
Bithot render system should default to that always-resync shape
unless there's a proven cost reason to add event filtering — and if
it does, it must use `OnComponentChanged`, not `EventFilter`.
