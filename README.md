# Mimameithr

A civilization-scale planetary simulation engine.
Models physical and civilizational processes over a 10 000-year horizon — tectonics, climate, hydrology, geology,
weather, vegetation, fauna, and the civilizations that emerge from and are constrained by them.

Godot 4 is the rendering layer.
The engine runs headless.
Not solar system bound, engine supports arbitrary planetary systems.

## Bifrost — Interop

| Directory                          | Purpose                                                   |
|------------------------------------|-----------------------------------------------------------|
| [`Kart`](Bifrost/Kart/README.adoc) | C#/Rust bridge to `Rustr.Kart` — geodesic grid operations |

## Kjarni — Simulation core

| Directory                             | Purpose                                                 |
|---------------------------------------|---------------------------------------------------------|
| [`Brunnr`](Kjarni/Brunnr/README.adoc) | Simulation framework — topology, scheduling, validation |
| [`Kvasir`](Kjarni/Kvasir/README.adoc) | Science simulations — stateless pure functions          |
| [`Nornir`](Kjarni/Nornir/README.adoc) | Domain models — tectonics, climate, civilization        |

## Rustr — Native performance

| Directory                          | Purpose                                                  |
|------------------------------------|----------------------------------------------------------|
| [`Kart`](Rustr/Kart/README.adoc)   | Geodesic grid — H3/h3o cell operations exposed via C FFI |
| [`Seidr`](Rustr/Seidr/README.adoc) | Heavy math — numerical and scientific computation        |

## Skald — Rendering

| Directory                            | Purpose                 |
|--------------------------------------|-------------------------|
| [`Bithot`](Skald/Bithot/README.adoc) | Godot 4 rendering layer |

## Völundr — Developer toolbox

| Directory                          | Purpose                            |
|------------------------------------|------------------------------------|
| [`Gler`](Volundr/Gler/README.adoc) | Build-time documentation generator |

## Other

| Directory | Purpose                                          |
|-----------|--------------------------------------------------|
| `docs/`   | Design documents and [kanban](.kanban/README.md) |
