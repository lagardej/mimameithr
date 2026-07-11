using UnitsNet;

namespace Kjarni.Kvasir.Geimr;

/// <summary>
///     Provides Newtonian gravitation and orbital mechanics calculations.
/// </summary>
public static class Gravitation
{
    /// <summary>
    ///     Newtonian constant of gravitation in m³ kg⁻¹ s⁻².
    /// </summary>
    private const double G = 6.67430e-11;

    /// <summary>
    ///     Computes gravitational acceleration at specific radius from centre of mass.
    /// </summary>
    public static Acceleration Acceleration(Mass mass, Length radius) =>
        UnitsNet.Acceleration.FromMetersPerSecondSquared(G * mass.Kilograms / (radius.Meters * radius.Meters));

    /// <summary>
    ///     Computes attractive force between two masses.
    /// </summary>
    public static Force Force(Mass mass1, Mass mass2, Length distance) =>
        UnitsNet.Force.FromNewtons(G * mass1.Kilograms * mass2.Kilograms / (distance.Meters * distance.Meters));

    /// <summary>
    ///     Computes speed required to escape gravitational pull of body.
    /// </summary>
    public static Speed EscapeVelocity(Mass mass, Length radius) =>
        Speed.FromMetersPerSecond(Math.Sqrt(2 * G * mass.Kilograms / radius.Meters));

    /// <summary>
    ///     Computes speed of stable circular orbit at specific radius.
    /// </summary>
    public static Speed OrbitalVelocity(Mass centralMass, Length orbitalRadius) =>
        Speed.FromMetersPerSecond(Math.Sqrt(G * centralMass.Kilograms / orbitalRadius.Meters));

    /// <summary>
    ///     Computes the standard gravitational parameter (μ = G·M) for a two-body system, in m³ s⁻².
    ///     No dedicated <c>UnitsNet</c> quantity exists for this, so the result is a raw SI double.
    /// </summary>
    public static double StandardGravitationalParameter(Mass mass1, Mass mass2) =>
        G * (mass1.Kilograms + mass2.Kilograms);
}
