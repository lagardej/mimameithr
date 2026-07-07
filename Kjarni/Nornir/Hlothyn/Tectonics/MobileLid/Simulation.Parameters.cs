using Kjarni.Kvasir.Foundation.Grid;
using UnitsNet;

namespace Kjarni.Nornir.Hlothyn.Tectonics.MobileLid;

/// <summary>All inputs to the tectonics simulation.</summary>
public sealed record Parameters
{
    /// <summary>Total number of major and minor tectonic plates.</summary>
    /// <remarks>Earth: ~15.</remarks>
    public required int PlateCount { get; init; }

    /// <summary>Plate size variance. Low: plates roughly equal in size. High: a few dominant plates and many small fragments.</summary>
    /// <remarks>Earth: ~0.78.</remarks>
    public required Ratio PlateFragmentation { get; init; }

    /// <summary>Rate at which plates reorganize: split, merge, or change direction over time.</summary>
    /// <remarks>Earth: ~0.11.</remarks>
    public required Ratio PlateStability { get; init; }

    /// <summary>Degree to which activity concentrates along plate boundaries vs spreading diffusely across the interior.</summary>
    /// <remarks>Earth: ~0.67.</remarks>
    public required Ratio BoundaryFocus { get; init; }

    /// <summary>Bias toward convergent boundaries. Drives mountain-building and subduction zones.</summary>
    /// <remarks>Earth: ~0.44.</remarks>
    public required Ratio CollisionDominance { get; init; }

    /// <summary>Number and intensity of intraplate volcanic plumes.</summary>
    /// <remarks>Earth: ~0.22.</remarks>
    public required Ratio HotSpotDensity { get; init; }

    /// <summary>Random seed for reproducible world generation.</summary>
    public required uint Seed { get; init; }

    /// <summary>Grid to operate on.</summary>
    public required IGrid Grid { get; init; }

    /// <summary>Mean radius of the body. Used to compute surface area for heat flux derivation.</summary>
    public required Length BodyRadius { get; init; }

    /// <summary>Gravitational acceleration at the body's surface. Scales plate displacement rates.</summary>
    public required Acceleration BodySurfaceGravity { get; init; }

    /// <summary>Total mass of the body. Scales mantle heat flux and radiogenic heat inventory.</summary>
    public required Mass BodyMass { get; init; }

    /// <summary>Age of the body. Drives radiogenic heat decay.</summary>
    public required Duration BodyAge { get; init; }
}
