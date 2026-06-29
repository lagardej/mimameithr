namespace Brunnr.Engine.Data.Validation;

/// <summary>Specifies which bounds of a quantity range are inclusive or exclusive.</summary>
public enum RangeBounds
{
    /// <summary>Both bounds inclusive: [min, max].</summary>
    Inclusive,

    /// <summary>Min inclusive, max exclusive: [min, max).</summary>
    ExclusiveMax,

    /// <summary>Min exclusive, max inclusive: (min, max].</summary>
    ExclusiveMin,

    /// <summary>Both bounds exclusive: (min, max).</summary>
    Exclusive
}
