using Kvasir.Natural.Physical.Geodesy;
using UnitsNet;

namespace Kvasir.Natural.Physical.Geology.Lithosphere.Tectonics;

/// <summary>All inputs to the tectonics simulation.</summary>
public sealed record TectonicsParameters
{
    /// <summary>Designer-facing world-gen knobs.</summary>
    public required TectonicsSettings Settings { get; init; }

    /// <summary>Physical properties derived from the parent body.</summary>
    public required BoundaryConditions BoundaryConditions { get; init; }
}

/// <summary>
///     World-generation input parameters for the tectonics simulation.
///     All integer knobs use a 1–10 scale. Ratio knobs use 1-100.
/// </summary>
public sealed record TectonicsSettings
{
    #region plate.geometry

    /// <summary>
    ///     <para>Number of tectonic plates seeded at world gen.</para>
    ///     <para>
    ///         <b>Range:</b> 1–10.<br />
    ///         <b>Low:</b> a few large, continent-spanning plates.<br />
    ///         <b>High:</b> a fragmented mosaic of smaller plates.
    ///     </para>
    /// </summary>
    /// <remarks>Earth: 5.</remarks>
    public required Setting10 PlateCount { get; init; }

    /// <summary>
    ///     <para>Plate fragmentation.</para>
    ///     <para>
    ///         <b>Range:</b> 1–10.<br />
    ///         <b>Low:</b> all plates roughly equal in size.<br />
    ///         <b>High:</b> a few dominant plates and many small fragments.
    ///     </para>
    /// </summary>
    /// <remarks>Earth: 8.</remarks>
    public required Setting10 PlateFragmentation { get; init; }

    #endregion

    #region plate.regime

    /// <summary>
    ///     <para>Rate at which plates reorganize: split, merge, or change direction over time.</para>
    ///     <para>
    ///         <b>Range:</b> 1–10.<br />
    ///         <b>Low:</b> stable cratons, predictable elevation drift.<br />
    ///         <b>High:</b> constant reshuffling, unstable terrain, clustered event bursts.
    ///     </para>
    /// </summary>
    /// <remarks>Earth: 2.</remarks>
    public required Setting10 PlateStability { get; init; }

    /// <summary>
    ///     <para>Degree to which activity concentrates along plate boundaries vs spreading diffusely across the interior.</para>
    ///     <para>
    ///         <b>Range:</b> 1–10.<br />
    ///         <b>Low:</b> activity is evenly distributed across the surface.<br />
    ///         <b>High:</b> activity is tightly focused on boundary zones.
    ///     </para>
    /// </summary>
    /// <remarks>Earth: 7.</remarks>
    public required Setting10 BoundaryFocus { get; init; }

    #endregion

    #region boundary.regime

    /// <summary>
    ///     <para>Bias toward convergent boundaries. Drives mountain-building and subduction zones.</para>
    ///     <para>
    ///         <b>Range:</b> 1–10.<br />
    ///         <b>Low:</b> rift-dominated world — island arcs, inland seas, low relief.<br />
    ///         <b>High:</b> collision-dominated world — tall mountain chains, deep trenches.
    ///     </para>
    /// </summary>
    /// <remarks>Earth: 5.</remarks>
    public required Setting10 CollisionDominance { get; init; }

    /// <summary>
    ///     <para>Number and intensity of intraplate volcanic plumes.</para>
    ///     <para>
    ///         <b>Range:</b> 1–10. <i>Earth: 3.</i><br />
    ///         <b>Low:</b> volcanic activity confined to plate boundaries.<br />
    ///         <b>High:</b> hot spots scattered across the interior, Hawaii-style island chains.
    ///     </para>
    /// </summary>
    /// <remarks>Earth: 3.</remarks>
    public required Setting10 HotSpotDensity { get; init; }

    #endregion
}

/// <summary>Physical properties of the parent body, passed into the tectonics simulation.</summary>
public sealed record BoundaryConditions
{
    /// <summary>Random seed for reproducible world generation.</summary>
    public required int Seed { get; init; }

    /// <summary>Geodesic grid to operate on.</summary>
    public required IGeodesicGrid Grid { get; init; }

    /// <summary>Mean radius of the body. Used to compute surface area for heat flux derivation.</summary>
    public required Length BodyRadius { get; init; }

    /// <summary>Gravitational acceleration at the body's surface. Scales plate displacement rates.</summary>
    public required Acceleration BodySurfaceGravity { get; init; }

    /// <summary>Total mass of the body. Scales mantle heat flux and radiogenic heat inventory.</summary>
    public required Mass BodyMass { get; init; }

    /// <summary>Age of the body. Drives radiogenic heat decay.</summary>
    public required Duration BodyAge { get; init; }
}
