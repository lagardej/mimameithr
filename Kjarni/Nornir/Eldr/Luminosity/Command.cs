using Kjarni.Brunnr.Command;
using System.ComponentModel.DataAnnotations;

namespace Kjarni.Nornir.Eldr.Luminosity;

/// <summary>Command to configure the luminosity of a star.</summary>
/// <param name="Id">The entity id.</param>
/// <param name="Luminosity">Luminosity on a 1–1000 scale mapping 10⁻³ L☉ to 10³ L☉ exponentially.</param>
/// <remarks>
/// <para>Luminosity scale reference</para>
/// Scale | Luminosity | Description
/// ----- | ---------- | -----------
///     1 |      0.001 | Extreme low-luminosity red dwarf
///   100 |      0.005 | Faint red dwarf
///   200 |      0.031 | Typical red dwarf
///   300 |      0.175 | Bright red dwarf / dim orange dwarf
///   400 |      1     | Sun-equivalent luminosity
///   500 |      2.15  | Bright solar-type star
///   600 |      4.64  | Late F-type / early A-type main-seq star
///   700 |     10     | Bright main-sequence star
///   800 |     46.4   | Giant star or luminous B-type star
///   900 |    215     | Bright giant / supergiant
///  1000 |   1000     | Highly luminous supergiant
/// </remarks>
public record SetLuminosity(
    int Id,
    [Range(1u, 1000u)] uint Luminosity
) : ICommand;
