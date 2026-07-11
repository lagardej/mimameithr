---
area: Skald/Bithot
---

# E2E: Forcing disrupts asteroid orbit, asteroid crashes into Sun

Godot-shell e2e (`BootTest`-style), not a unit test. Boot a solar
system with Sun, Earth, Moon, and an asteroid on a stable orbit.
Apply a Forcing that disrupts the asteroid's orbit (e.g. drops
periapsis below Sun's radius or kills orbital velocity). Advance the
simulation and assert the asteroid eventually collides with the Sun.

## Assertion

Asteroid's `PositionC` distance to Sun's `PositionC` reaches ≤ Sun's
`GeometryC.Radius` within the run.

## Dependencies / blockers

- Forcings aren't materialized as commands yet (see direction noted
  in `geometrysystem-missed-updates.md`, now in `3_Done`) — this
  scenario needs that first, to have a `SetPosition`-style call
  Verðandi can issue mid-tick on the asteroid.
- No collision/proximity detection exists yet. Need either a system
  that checks body-to-body distance against combined radii, or the
  test polls `PositionC` directly each tick from outside the engine.
- `OrbitalMechanics.MeanToTrueAnomaly` assumes elliptical orbits
  (`e < 1`, Newton-Raphson loop). A hard velocity-kill forcing
  should keep `e < 1` (radial infall), so this scenario doesn't by
  itself require hyperbolic-orbit support — but note the gap exists
  if a future forcing scenario needs escape trajectories.
- Real-time run at physical timescales is impractical for a crash
  test; use `Kjarni.Brunnr.Engine.Time.TimeCompression` to
  accelerate, or pick forcing parameters (near-zero angular
  momentum) that crash fast.
