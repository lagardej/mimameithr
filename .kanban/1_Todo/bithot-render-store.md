---
status: todo
area: Skald/Bithot
---

# Shared-store render sync

Nornir stays headless — `Urðr` "must stay operable without Godot"
(`AGENTS.md`). No render components inside `Kjarni.Nornir`, no
Godot/Bithot reference from it. But render state (visual radius,
per-cell plate colour/motion vector, boundary outline style) needs
*some* store — decided against a second mirrored `EntityStore` (join
key back to Nornir entities, sync-on-demand, drift risk). Share the
one store instead:

## Shape

- New `Kjarni/Brunnr/Engine/BrunnrEngine.cs` — owns the
  `EntityStore`, nothing else.
- `Nornir` ctor takes an injected store instead of `new`-ing its own:
  `public Nornir(EntityStore store)`. Keep a parameterless overload
  (`: this(new EntityStore())`) so `Nornir.Tests` and any headless
  use keeps working unchanged.
- Bithot owns a render engine (`Skald.Bithot.Render`) constructed
  with the *same* store. Render-only component types
  (`VisualRadiusC`, later `PlateColorC`, `PlateMotionVectorC`,
  `BoundaryOutlineC`) live there, added directly onto the same cell
  entities Nornir already created (`CellIdentityC` /
  `CellParentRefC`). No join key needed — same `Entity`/`Id`.
- `BootTest` becomes the composition root: builds the shared store,
  constructs `Nornir` and the render engine on it.

## Sync: event-driven, not re-derive-after-command

Forcings (`Kjarni.Brunnr.Autodoc.ForcingAttribute`) will mutate
individual components at runtime, mid-tick, via `Verðandi`'s systems
— not through discrete top-level commands. There's no single call
site to hang a "re-derive everything" step on once state changes
continuously during `Advance()`. Re-deriving after every command
does not scale to that and was rejected.

Instead:

- Bithot subscribes to `store.OnComponentChanged`, filtered to the
  component types it renders.
- Handler only marks the entity dirty (`HashSet<Entity>`) — no work
  inline. A mutation can fire many times per tick; recomputing
  visuals synchronously on every one just moves the cost problem to
  a finer grain.
- Bithot's own per-frame step (Godot `_Process`, separate cadence
  from `Verðandi.Advance`) drains the dirty set once, recomputes
  render components for exactly those entities.
- This buys the decoupling needed once `Compression` lets simulation
  run faster than real time: many sim ticks / forcing applications
  coalesce into one dirty flag between frames, no redundant
  recompute, nothing dropped either — the event captures every
  mutation, the drain just batches *when* it's acted on.

## Prerequisite for

`bithot-render-tectonics` — per-cell plate/boundary rendering needs
this store and sync model in place first.
