using Kjarni.Brunnr.Command;

namespace Kjarni.Nornir.Geimr.Position;

/// <summary>Command to configure the position and velocity of a planetary body.</summary>
/// <param name="Id">The entity id.</param>
/// <param name="X">Cartesian X coordinate, in kilometres, relative to the system origin.</param>
/// <param name="Y">Cartesian Y coordinate, in kilometres, relative to the system origin.</param>
/// <param name="Z">Cartesian Z coordinate, in kilometres, relative to the system origin.</param>
/// <param name="VelocityX">
///     Velocity component along X, in kilometres per second, relative to <paramref name="ParentId" />
///     (or absolute if none).
/// </param>
/// <param name="VelocityY">
///     Velocity component along Y, in kilometres per second, relative to <paramref name="ParentId" />
///     (or absolute if none).
/// </param>
/// <param name="VelocityZ">
///     Velocity component along Z, in kilometres per second, relative to <paramref name="ParentId" />
///     (or absolute if none).
/// </param>
/// <param name="ParentId">
///     The entity this body orbits. When set, an <see cref="OrbitC" /> is derived from the given state
///     vector and propagated each tick; <see cref="OrbitParentC" /> is attached linking to this entity.
///     May be null for a body that does not orbit anything (e.g. a lone star).
/// </param>
public record SetPosition(
    int Id,
    double X = 0,
    double Y = 0,
    double Z = 0,
    double VelocityX = 0,
    double VelocityY = 0,
    double VelocityZ = 0,
    int? ParentId = null
) : ICommand;
