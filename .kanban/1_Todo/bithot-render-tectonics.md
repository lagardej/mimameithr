---
area: Skald/Bithot
---

# Render tectonics data

No visualization of `TectonicsPlateC`/`TectonicsBoundaryC` state yet
(`Kjarni/Nornir/Hlothyn/Tectonics/MobileLid/Component.cs`). Depends on
`bithot-real-body-geometry` for correct scale/positioning and
`bithot-render-store` for per-cell render component storage.

- Plates: colour each R0 cell (e.g. by `PlateSeedCellId` or
  `CrustComposition`), draw `PlateAngularVelocity`-derived linear
  motion vector per cell.
- Boundaries: outline R2 boundary/hot-spot cells, styled per
  `BoundaryType` (Convergent/Divergent/Transform/HotSpot).
