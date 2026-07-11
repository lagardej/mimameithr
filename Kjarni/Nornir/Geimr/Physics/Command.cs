using Kjarni.Brunnr.Command;
using Kjarni.Kvasir.Foundation;
using System.ComponentModel.DataAnnotations;

namespace Kjarni.Nornir.Geimr.Physics;

/// <summary>Command to configure the physical properties of a planetary body.</summary>
/// <param name="Id">The entity id.</param>
/// <param name="Age">Age of the body, on a 1-1000 scale mapping 10⁶ to 10¹³ years exponentially.</param>
/// <param name="Mass">Mass of the body on a 1-1000 scale mapping 10¹² kg to 10³² kg exponentially.</param>
/// <remarks>
///     <para>Age scale reference</para>
///     <code>
/// Scale | Age (years) | Description
/// ----- | ----------- | ------------
///     1 |  1    ×10⁶  | Newly formed system
///   100 |  1    ×10⁷  | Young stars, active protoplanetary aftermath
///   200 |  1    ×10⁸  | Mature young stellar systems
///   300 |  1    ×10⁹  | Typical Galactic-disk stars
///   400 |  1    ×10¹⁰ | Present cosmic epoch
///   500 |  2.15 ×10¹⁰ | Late Stelliferous Era
///   600 |  4.64 ×10¹⁰ | Ageing stellar population
///   700 |  1    ×10¹¹ | Star formation greatly diminished
///   800 |  4.64 ×10¹¹ | Final generations of red dwarfs dominate
///   900 |  2.15 ×10¹² | Near end of Stelliferous Era
///  1000 |  1    ×10¹³ | Stelliferous Era ending
/// </code>
///     <para>Mass scale reference</para>
///     <code>
/// Scale | Mass (kg) | Description
/// ----- | --------- | -----------
///     1 |    1×10¹² | Small asteroid
///   100 |    3×10¹⁵ | Asteroid
///   200 |    1×10¹⁹ | Large asteroid
///   300 |    3×10²² | Dwarf planet / medium moon
///   400 |    1×10²⁶ | Neptune-class planet
///   500 |    1×10²⁷ | Jupiter-class planet
///   600 |    1×10²⁸ | Super-Jupiter / brown dwarf regime
///   700 |    1×10²⁹ | Lowest-mass stars
///   800 |    1×10³⁰ | Sun-like stars
///   900 |    1×10³¹ | Massive stars
///  1000 |    1×10³² | Most massive stars
/// </code>
/// </remarks>
public record SetPhysics(
    int Id,
    [Range(1u, 1000u)] uint Age,
    [Range(1u, 1000u)] uint Mass
) : ICommand;

/// <summary>The scales used by the command properties.</summary>
public static class SetPhysicsScale
{
    /// <summary>Age scale: 10⁶ to 10¹³ years exponentially.</summary>
    public static readonly PiecewiseExponentialScale AgeScale =
        new(Scaling.Range1000, [6, 10, 11, 13], [400, 700, 1000]);

    /// <summary>Mass scale: 10¹² to 10³² kg exponentially.</summary>
    public static readonly PiecewiseExponentialScale MassScale =
        new(Scaling.Range1000, [12, 26, 29, 32], [400, 700, 1000]);
}
