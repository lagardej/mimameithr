---
status: done
area: Skald/Bithot
---

# Use real body geometry for rendering

`BootTest.SphereRadius` (`Skald/Bithot/BootTest.cs`) is a hardcoded
`const float = 1f`, disconnected from the actual body created in
`EngineBootstrap.CreateTestBody` (`BodyRadius = 400`, via
`SetGeometry`). Sphere, camera, equator guide, and axis guide all
scale off the fake constant instead of `GeometryC.Radius` on the
body entity.

Read `GeometryC` back from the engine after `CreateTestBody` and
drive all `Configure(...)` calls from it.
