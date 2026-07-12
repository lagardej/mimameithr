---
area: Geimr/Orbit
---

# Orbit system rewrite

`PositionC` (`Kjarni/Nornir/Geimr/Position/`) is the current source of
truth. `OrbitC`, `SetOrbit`, `SetOrbitHandler` are `[Obsolete("Use
Position instead")]`, handler body fully commented out. Downstream,
`IrradianceSystem.OnUpdate` is entirely commented out too — it needs
`DistanceFromStar` and orbital angle (true anomaly) that only `OrbitC`
used to provide. This ticket is what unblocks it.

## Ownership stays in Nornir

`Urðr` "must stay operable without Godot" (`AGENTS.md`). Orbit state —
distance from star, orbital angle — feeds `IrradianceSystem`, pure
sim, no render involved. Whatever replaces `OrbitC` stays a Nornir
component, computed by a Nornir system. Bithot reads it from the
shared store (`bithot-render-store`) same as any other component, no
ownership change there.

## Command surface: keep `SetPosition`

`SetPosition` (cartesian X/Y/Z + velocity) stays the way a designer
configures a body's orbit — easier to reason about than typing in six
astronomical scale values. `SetOrbit`/`OrbitC`'s scale-based Keplerian
input (`Eccentricity`, `InitialMeanAnomaly`, `OrbitalPeriod`,
`SemiMajorAxis`) is not coming back as a command surface.

Gap: `SetPosition` currently has no `ParentId`. `OrbitParentC`
(`ILinkComponent`, `geimr-orbit-parent-ref`) already exists as the
link type but nothing sets it since `SetOrbitHandler` was gutted.
Add `ParentId` to `SetPosition` (mirroring what `SetOrbit` used to
take) so `SetPositionHandler` can attach `OrbitParentC`. Needed for
`μ` (combined `G·(Mstar + Mbody)`, via `Kjarni.Kvasir.Geimr.Gravitation`)
and for `IrradianceSystem` to find the star through
`CellParentRefC` → body → `OrbitParentC` → star.

## Derivation: state vector → conserved elements → propagate

Two-body Kepler propagation, not per-tick N-body integration —
cheaper, and it's the same tick-loop shape the old `OrbitSystem`
already had (Kepler's equation solve via Newton-Raphson is reusable
as-is).

1. **On `SetPosition`** (`SetPositionHandler`, once): given initial
   `r₀` (position) and `v₀` (velocity) relative to the parent, and
   `μ` from `Gravitation`, derive the conserved two-body quantities:
   - specific angular momentum `h = r₀ × v₀` → orbital plane normal
   - eccentricity vector `e = (v₀ × h)/μ − r̂₀` → magnitude +
     periapsis direction
   - semi-major axis `a` from vis-viva:
     `1/a = 2/|r₀| − |v₀|²/μ`
   - mean anomaly at epoch, from true anomaly at `t = 0` (angle
     between `e` and `r₀`) via the existing
     `OrbitSystem.MeanToTrueAnomaly` inverse
   Store these on a reinstated `OrbitC` (drop `[Obsolete]`), plus
   `OrbitalPeriod` from `a` and `μ` (`T = 2π√(a³/μ)`).
2. **Each tick** (`OrbitSystem.OnUpdate`, as before): advance mean
   anomaly by elapsed time, solve Kepler's equation for true anomaly,
   compute `DistanceFromStar` — all unchanged from the current
   (commented-out-adjacent) implementation.
3. **New step**: reconstruct `PositionC.{X,Y,Z}` each tick from
   `DistanceFromStar`, `OrbitalAngle`, and the orbital-plane basis
   (periapsis direction from `e`, normal from `h`). `PositionC`
   becomes a derived/output component after the initial `SetPosition`
   call seeds it — same role `VisualRadiusC` plays for render state
   in `bithot-render-store`, but this one stays in Nornir since sim
   (`IrradianceSystem`) reads it too, not just render.

## Rejected

- Reviving `SetOrbit`'s Keplerian-scale command surface — designer
  UX regression, no reason to reintroduce it now that state-vector
  input works.
- Per-tick numerical integration (RK4 etc.) of `PositionC` from
  velocity directly — drifts without energy/angular-momentum
  correction, more expensive per tick than analytic Kepler
  propagation for a two-body system.
- Moving `PositionC`/orbit state ownership to `Skald.Bithot` —
  breaks the headless requirement; `IrradianceSystem` needs this data
  with no Godot involved.

## Unblocks

`IrradianceSystem.OnUpdate` — currently a fully commented-out no-op,
waiting on `DistanceFromStar` and orbital angle to exist again.
