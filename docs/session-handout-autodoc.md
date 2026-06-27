# Session Handout — Autodoc Next Steps

## Context

Autodoc generates per-component `README.adoc` files and a global `Nornir/README.adoc` index
from reflected attributes in `Brunnr.Autodoc`.

The global index is functional. Per-component READMEs are empty.

---

## 1. Fix per-component README generation

`BuildParameterRows` and `BuildStateRows` in `Volundr/Autodoc/Program.cs` look for separate
holder types annotated with `[Settings]` / `[States]` in the component namespace.

Attributes are now declared directly on component struct fields:

```csharp
[Setting("m", "Mean radius of the body.")]
public Length Radius;

[State("m/s²", "Gravitational acceleration at the body's surface.")]
public Acceleration SurfaceGravity;
```

The generator must be updated to:
- Scan fields and properties on the `[Component]`-annotated struct itself.
- Collect `[Setting]` fields for the Parameters section.
- Collect `[State]` fields for the States section.
- Remove the `[Settings]` / `[States]` holder type lookup (no longer used).

