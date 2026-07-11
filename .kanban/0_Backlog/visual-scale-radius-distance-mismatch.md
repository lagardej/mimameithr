---
area: Skald/Bithot
---

# Body radius/distance visual scale mismatch

`VisualScale` (log, 1 km\u20131e9 km \u2192 0.05\u201340 units) and `PositionScale`
(linear, 3,000,000 km/unit) are tuned independently. For the
bootstrap solar system they collide: bodies render nearly as big as
their orbital separation.

## Measured (BootTest, current bootstrap)

```
Body_1 (Sun):   real radius=691,831 km   visual radius=25.973
Body_2 (Earth): real radius=6,302 km     visual radius=16.916
Body_3 (Moon):  real radius=1,730 km     visual radius=14.424

Body_1 pos=(0, 0, 0) km              visual=(0, 0, 0)
Body_2 pos=(150,000,000, 0, 0) km    visual=(50, 0, 0)
Body_3 pos=(150,384,400, 0, 0) km    visual=(50.13, 0, 0)
```

Sun+Earth radii sum to 43 units against a 50-unit separation \u2014
spheres nearly touch on screen despite a real 110\u00d7 radius ratio and
a real ~215\u00d7 (radius vs. distance) separation ratio. `VisualScale`'s
log range is calibrated for its full documented span (asteroid to
supergiant); a modest solar system's bodies all land in the top
slice of that range, compressing them toward similar visual size.

## Options considered, not decided

1. Shrink `VisualScale`'s max visual radius \u2014 less overlap, still
   log-compressed, may make asteroids invisible at the other end.
2. Grow `PositionScale`'s scene-units-per-AU \u2014 spreads bodies out;
   `Orbit.OrbitCamera`'s default view distance (sized "for a
   sun-sized body", see `PositionScale` remarks) would likely need
   re-tuning too.
3. Both, tuned together specifically against this 3-body case.

Not blocking `boottest-root-node-active-phase.md`'s Position
sub-task \u2014 the sync mechanism itself (`PositionSystem`,
`BodyNodeC` lookup) is confirmed correct; this is a separate visual
calibration problem.
