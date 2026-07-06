using Kjarni.Kvasir.Formal.Maths.Geometry;

namespace Kjarni.Kvasir.Natural.Physical.Geodesy;

/// <summary>Latitude/longitude coordinate in degrees.</summary>
public readonly record struct LatLng(double Lat, double Lng)
{
    /// <summary>Outward unit vector on a sphere at this coordinate.</summary>
    public Vector3D ToUnitVector()
    {
        var lat = Lat * Math.PI / 180.0;
        var lng = Lng * Math.PI / 180.0;
        return new Vector3D(
            Math.Cos(lat) * Math.Cos(lng),
            Math.Cos(lat) * Math.Sin(lng),
            Math.Sin(lat));
    }
}
