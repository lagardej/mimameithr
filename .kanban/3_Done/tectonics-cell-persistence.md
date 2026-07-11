---
status: done
area: Hlothyn/Tectonics
---

# Cell entity persistence for MobileLid tectonics

`Simulation.Result` returns domain objects: `Plate` (covers many R0 cells
via `Plate.Cells`), `Boundary` (one per R2 cell, dense — every cell incl.
`None`).

`SetTectonicsMobileLidHandler` creates `CellIdentityC`/`CellParentRefC`
entities on demand (no pre-seeding at `SetGeometry` time), decides what
to persist:

- all R0 cells get `TectonicsPlateC` (dense, 122/122 confirmed)
- only non-`None` R2 cells get `TectonicsBoundaryC` (sparse, ~51%
  confirmed — matches H3 res-2 cell count)

Filtering `None` is handler's call, not simulation's.
