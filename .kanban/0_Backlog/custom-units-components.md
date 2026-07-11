---
area: Nornir/Components
priority: medium
---

# Custom domain units for typed component properties

Replace raw `double`/`float` properties in Nornir components with typed UnitsNet quantities.

## Phase 1 (Highest ROI)

1. **GeologicalTime** (million years)
   - Used in: `OrogenyC.OrogenicAge`, `MobileLidC.CrustAge`
   - Semantics: Geological timescale (My), distinct from generic `Duration`

2. **PlateVelocityMmPerYear** (mm/year)
   - Used in: `MobileLidC.VerticalDisplacementRate`
   - Semantics: Tectonics standard unit, not generic `Speed`

3. **Mantle HeatFlux** (verify W/m² semantics)
   - Used in: `MantlePhysics.AsthenosphericHeatFlux()`
   - Status: Already `HeatFlux` from UnitsNet; confirm correctness

## Phase 2

4. **SolarIrradiance** or **SolarFlux** for Eldr quantities
   - Used in: Irradiance/Luminosity (currently raw `double`)

5. **Orbital elements** (Geimr context)
   - SemiMajorAxis, Eccentricity, orbital mechanics
   - Used in: `OrbitC`, `PositionC` (coordinates)

## Acceptance Criteria

- All component properties are UnitsNet quantities (no bare `double`/`float`)
- Compiler prevents unit confusion (e.g., `Length` vs `Duration`)
- Serialization/deserialization updated (JSON, Rust interop)
- Tests pass for all conversions and edge cases
- Backwards-compatible migration path for existing data

## Related

See `docs/architecture.adoc` for component conventions.
