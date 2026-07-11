---
status: todo
area: Skald/Bithot
---

# Bithot render-side ECS store

Nornir stays headless — `Urðr` "must stay operable without Godot"
(`AGENTS.md`). No render components inside `Kjarni.Nornir`. Bithot
needs its own component store for derived render state (visual
radius, per-cell plate colour/motion vector, boundary outline style)
instead of scattering ad-hoc records/`Configure(float)` params as
tectonics rendering grows.

- Second `Friflo.Engine.ECS.EntityStore` owned by Bithot, separate
  from Nornir's.
- Bootstrap step reads Nornir (`GetComponent`/`Query`) after
  generation, creates matching render entities, writes Bithot-only
  components (`VisualRadiusC`, later `PlateColorC`,
  `PlateMotionVectorC`, `BoundaryOutlineC`).
- Join key back to source: `int` body id / `CellId` per cell, as an
  `IIndexedComponent<>` on the render entity for O(1) lookup.
- Render nodes query the render store only, never touch Nornir
  directly.

`VisualScale.ToVisualRadius` (`Skald/Bithot/Render/VisualScale.cs`)
becomes the source for the first component (`VisualRadiusC`) once
this lands.

Prerequisite for `bithot-render-tectonics` (per-cell plate/boundary
data needs the same store).
