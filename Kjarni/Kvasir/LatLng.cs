using System.Numerics;

namespace Kvasir;

/// <summary>Latitude/longitude coordinate in degrees.</summary>
public readonly record struct LatLng(double Lat, double Lng)
{
    /// <summary>Outward unit vector on a sphere at this coordinate.</summary>
    public Vector3 ToUnitVector()
    {
        var lat = Lat * Math.PI / 180.0;
        var lng = Lng * Math.PI / 180.0;

        return new Vector3(
            (float) (Math.Cos(lat) * Math.Cos(lng)),
            (float) (Math.Cos(lat) * Math.Sin(lng)),
            (float) Math.Sin(lat));
    }
}
