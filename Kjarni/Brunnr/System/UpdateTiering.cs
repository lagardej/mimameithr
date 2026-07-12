using Friflo.Engine.ECS;
using UnitsNet;

namespace Kjarni.Brunnr.System;

/// <summary>
///     Maps a physical period to one of three update-cadence tiers. A short period (e.g. a fast
///     rotator) needs frequent updates to stay visually smooth; a long period (e.g. a slow rotator)
///     can be updated rarely without visible difference — bucketing by period avoids paying the
///     same update cost for both.
/// </summary>
public static class UpdateTiering
{
    /// <summary>Simulated-time update interval for <see cref="FastTierTag" /> entities, in seconds.</summary>
    public const float FastInterval = 0.5f;

    /// <summary>Simulated-time update interval for <see cref="MediumTierTag" /> entities, in seconds.</summary>
    public const float MediumInterval = 10f;

    /// <summary>Simulated-time update interval for <see cref="SlowTierTag" /> entities, in seconds.</summary>
    public const float SlowInterval = 600f;

    /// <summary>Periods below this threshold are tagged <see cref="FastTierTag" />.</summary>
    public static readonly Duration FastThreshold = Duration.FromMinutes(1);

    /// <summary>
    ///     Periods at or above this threshold are tagged <see cref="SlowTierTag" />; periods between
    ///     <see cref="FastThreshold" /> and this are tagged <see cref="MediumTierTag" />.
    /// </summary>
    public static readonly Duration SlowThreshold = Duration.FromDays(1);

    /// <summary>Returns the tier tag matching the given physical <paramref name="period" />.</summary>
    public static Tags TagFor(Duration period) =>
        period < FastThreshold ? Tags.Get<FastTierTag>()
        : period < SlowThreshold ? Tags.Get<MediumTierTag>()
        : Tags.Get<SlowTierTag>();
}
