using Kjarni.Brunnr.Command;
using System.ComponentModel.DataAnnotations;

namespace Kjarni.Nornir.Geimr.Rotation;

/// <summary>Command to configure the rotational properties of a planetary body.</summary>
/// <param name="Id">The entity id.</param>
/// <param name="InitialAngle">Initial angle of rotation, in degrees. Range: [1, 360].</param>
/// <param name="RotationPeriod">Duration of one full rotation on a 1-1000 scale mapping 10⁹ to 10 s exponentially.</param>
/// <remarks>
///     <para>Rotation period scale reference</para>
///     <code>
/// Scale |               Period (s) | Description
/// ----- | ------------------------ | -----------
///     1 | 1,000,000,000 (31.7 y)   | Very slow rotator
///   100 |   101,746,340 (3.22 y)   | Very slow rotator
///   200 |    10,116,086 (117 d)    | Slow rotator
///   300 |     1,005,788 (11.6 d)   | Slow rotator
///   400 |       100,000 (27.8 h)   | Earth-like range
///   500 |        21,544 (5.98 h)   | Fast planetary rotator
///   600 |         4,642 (1.29 h)   | Very fast planetary rotator
///   700 |         1,000 (16.7 min) | Extreme rotator
///   800 |           215 (3.59 min) | Extreme rotator
///   900 |          46.4            | Near breakup regime
///  1000 |            10            | Ultra-fast rotator
/// </code>
/// </remarks>
public record SetRotation(
    int Id,
    [Range(1u, 360u)] uint InitialAngle,
    [Range(1u, 1000u)] uint RotationPeriod
) : ICommand;
