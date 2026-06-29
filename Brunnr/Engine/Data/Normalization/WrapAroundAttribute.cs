namespace Brunnr.Engine.Data.Normalization;

/// <summary>
///     Marks an <see cref="UnitsNet.IQuantity" /> property for cyclic wrap-around into [min, max).
///     The value is extracted and reconstructed in the given unit.
/// </summary>
/// <example>
///     <code>
///         [WrapAround(0, 360, AngleUnit.Degree)]
///         public required Angle AxialPrecessionPhase { get; init; }
///     </code>
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public sealed class WrapAroundAttribute(double min, double max, object unit) : Attribute
{
    public double Min { get; } = min;
    public double Max { get; } = max;
    public Enum Unit { get; } = (Enum) unit;
}
