using Nornir.Engine.Domain;

namespace Nornir.System.Primosphere.Irradiance.Sphere;

public static class IrradianceSphereFields
{
    /// <summary>Stellar energy flux received at a surface cell, in W/m².</summary>
    public static readonly FieldId Irradiance = new(IrradianceSphereSystem.Key, 0);
}
