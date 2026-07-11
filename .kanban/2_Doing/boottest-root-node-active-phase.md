---
area: Skald/Bithot
---

# BootTest as active-phase root node

`BootTest._Ready` currently only runs the generation phase (seed,
`CreateSolarSystem`, one `Bithot.Advance()`, `Bithot.AttachTo`) — no
`_Process` override, so `Nornir`/`Verðandi` never ticks after boot.
Promote `BootTest` to drive the active phase too:

```csharp
public override void _Process(double delta)
{
    _nornir.Advance((float)delta);
    _bithot.Advance();
}
```

Order matters: domain state (`PositionSystem`, `RotationSystem`)
must tick before Bithot re-derives render components from it.

## Gap found while scoping this

`PositionSystem` (`Kjarni/Nornir/Geimr/Position/System.cs`) and
`RotationSystem` (`Kjarni/Nornir/Geimr/Rotation/System.cs`) already
correctly update `PositionC`/`RotationC` each tick. Nothing reads
them back into Godot — `BodyRenderer.AttachTo` sets each body's
`Node3D` transform once, keeps no reference afterward. Wiring
`_Process` alone ticks orbits/rotation correctly in the domain layer
but produces no visible movement.

Needed alongside the `_Process` wiring:

- A way to look up "the `Node3D` for entity X" from a Bithot system.
  Options: store node refs in a Bithot-side lookup, or add a
  component (e.g. `BodyNodeC { Node3D Node }`) set by `BodyRenderer`
  and queried by the new systems — latter matches existing
  `GeometrySystem` pattern (render state lives as components on the
  shared store).
- New Bithot-side system(s) mirroring `GeometrySystem`'s
  event-driven sync: read `PositionC`/`RotationC` off changed
  entities, convert `Length`→world-scale float (need an orbital-
  distance equivalent of `VisualScale`, distinct from body radius
  scale) and `Angle`→radians, write to the looked-up `Node3D`.
  Register in `BithotSystemRegistry.Build`.

Without this, `_Process` wiring alone is misleading — ticks the sim,
shows nothing changing on screen.

## Sub-tasks

- [ ] Position render sync — orbital-distance scale (`VisualScale`
      equivalent for `Length`→world float), `PositionC` → `Node3D.Position`
- [ ] Rotation render sync — `RotationC.CurrentAngle` (`Angle`) →
      `Node3D.Rotation`, radians conversion
