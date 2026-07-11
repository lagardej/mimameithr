---
area: Hlothyn/Tectonics
blocks: ilinkrelation-orogeny-belt-cell
---

# Per-plate-pair boundary classification

Currently per-cell — cell's boundary type doesn't know which two plates
it's between. Prerequisite for Orogeny belt coherence.

Known limitation caused by this: Orogeny belt grouping is spatial-only
(flood fill over convergent cells) — merges cells from unrelated plate
pairs into one belt. Fixed once this lands.
