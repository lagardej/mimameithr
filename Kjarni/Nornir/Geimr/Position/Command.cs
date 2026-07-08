using Kjarni.Brunnr.Command;

namespace Kjarni.Nornir.Geimr.Position;

/// <summary>Command to configure the position and velocity of a planetary body.</summary>
/// <param name="Id">The entity id.</param>
/// <param name="X">Cartesian X coordinate, in kilometres, relative to the system origin.</param>
/// <param name="Y">Cartesian Y coordinate, in kilometres, relative to the system origin.</param>
/// <param name="Z">Cartesian Z coordinate, in kilometres, relative to the system origin.</param>
/// <param name="VelocityX">Velocity component along X, in kilometres per second.</param>
/// <param name="VelocityY">Velocity component along Y, in kilometres per second.</param>
/// <param name="VelocityZ">Velocity component along Z, in kilometres per second.</param>
public record SetPosition(
    int Id,
    double X,
    double Y,
    double Z,
    double VelocityX,
    double VelocityY,
    double VelocityZ
) : ICommand;
