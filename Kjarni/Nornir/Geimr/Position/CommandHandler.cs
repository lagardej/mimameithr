using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using Kjarni.Brunnr.System;
using Kjarni.Kvasir.Geimr;
using Kjarni.Nornir.Geimr.Physics;
using System.Numerics;
using UnitsNet;

namespace Kjarni.Nornir.Geimr.Position;

/// <summary>Handles <see cref="SetPosition" /> commands against the entity store.</summary>
public class SetPositionHandler(EntityStore store) : ICommandHandler<SetPosition>
{
    /// <summary>
    ///     Eccentricity below which the periapsis direction is undefined; the epoch position is used as the angle-zero
    ///     reference instead.
    /// </summary>
    private const double EccentricityEpsilon = 1e-8;

    /// <inheritdoc />
    public void Handle(SetPosition command)
    {
        var entity = store.GetEntityById(command.Id);

        entity.AddComponent(new PositionC
        {
            X = Length.FromKilometers(command.X),
            Y = Length.FromKilometers(command.Y),
            Z = Length.FromKilometers(command.Z),
            VelocityX = Speed.FromKilometersPerSecond(command.VelocityX),
            VelocityY = Speed.FromKilometersPerSecond(command.VelocityY),
            VelocityZ = Speed.FromKilometersPerSecond(command.VelocityZ)
        });

        if (command.ParentId is null)
        {
            return;
        }

        var parentEntity = store.GetEntityById(command.ParentId.Value);
        entity.AddComponent(new OrbitParentC { Parent = parentEntity });
        var orbit = DeriveOrbit(entity, parentEntity);
        entity.AddComponent(orbit);
        entity.AddTags(UpdateTiering.TagFor(orbit.OrbitalPeriod));
    }

    /// <summary>
    ///     Derives conserved two-body orbital elements from the state vector just assigned to
    ///     <paramref name="body" />, relative to <paramref name="parent" />'s current position. Velocity is
    ///     taken as given directly — relative to the parent, not the system origin.
    /// </summary>
    private static OrbitC DeriveOrbit(Entity body, Entity parent)
    {
        if (!parent.HasComponent<PositionC>())
        {
            throw new InvalidOperationException(
                $"Cannot derive orbit for entity {body.Id}: parent entity {parent.Id} has no PositionC. " +
                "Configure the parent's position before setting an orbiting child's.");
        }

        if (!parent.HasComponent<PhysicsC>())
        {
            throw new InvalidOperationException(
                $"Cannot derive orbit for entity {body.Id}: parent entity {parent.Id} has no PhysicsC. " +
                "Configure the parent's mass before setting an orbiting child's position.");
        }

        var position = body.GetComponent<PositionC>();
        var parentPosition = parent.GetComponent<PositionC>();
        var bodyMass = body.HasComponent<PhysicsC>() ? body.GetComponent<PhysicsC>().Mass : Mass.Zero;
        var parentMass = parent.GetComponent<PhysicsC>().Mass;
        var mu = Gravitation.StandardGravitationalParameter(bodyMass, parentMass);

        var rx = (position.X - parentPosition.X).Meters;
        var ry = (position.Y - parentPosition.Y).Meters;
        var rz = (position.Z - parentPosition.Z).Meters;
        var vx = position.VelocityX.MetersPerSecond;
        var vy = position.VelocityY.MetersPerSecond;
        var vz = position.VelocityZ.MetersPerSecond;

        var rMag = Math.Sqrt((rx * rx) + (ry * ry) + (rz * rz));
        if (rMag == 0.0)
        {
            throw new InvalidOperationException(
                $"Cannot derive orbit for entity {body.Id}: position coincides with parent entity {parent.Id}.");
        }

        // Specific angular momentum: h = r × v. Defines the orbital plane.
        var hx = (ry * vz) - (rz * vy);
        var hy = (rz * vx) - (rx * vz);
        var hz = (rx * vy) - (ry * vx);
        var hMag = Math.Sqrt((hx * hx) + (hy * hy) + (hz * hz));

        if (hMag == 0.0)
        {
            throw new InvalidOperationException(
                $"Cannot derive orbit for entity {body.Id}: velocity is purely radial (zero angular momentum) " +
                "— not a supported orbit shape.");
        }

        // Eccentricity vector: e = (v × h) / μ − r̂. Points toward periapsis; magnitude is the eccentricity.
        var vCrossHx = (vy * hz) - (vz * hy);
        var vCrossHy = (vz * hx) - (vx * hz);
        var vCrossHz = (vx * hy) - (vy * hx);
        var ex = (vCrossHx / mu) - (rx / rMag);
        var ey = (vCrossHy / mu) - (ry / rMag);
        var ez = (vCrossHz / mu) - (rz / rMag);
        var eccentricity = Math.Sqrt((ex * ex) + (ey * ey) + (ez * ez));

        // Vis-viva equation: 1/a = 2/|r| − |v|²/μ.
        var vMagSquared = (vx * vx) + (vy * vy) + (vz * vz);
        var inverseSemiMajorAxis = (2.0 / rMag) - (vMagSquared / mu);
        if (inverseSemiMajorAxis <= 0.0)
        {
            throw new InvalidOperationException(
                $"Cannot derive orbit for entity {body.Id}: state vector describes an unbound (parabolic or " +
                "hyperbolic) trajectory, not a supported orbit shape.");
        }

        var semiMajorAxis = 1.0 / inverseSemiMajorAxis;
        var orbitalPeriod = 2.0 * Math.PI * Math.Sqrt(Math.Pow(semiMajorAxis, 3) / mu);
        var normal = new Vector3((float) (hx / hMag), (float) (hy / hMag), (float) (hz / hMag));

        Vector3 periapsisDirection;
        Angle trueAnomaly;
        if (eccentricity < EccentricityEpsilon)
        {
            // Periapsis is undefined for a circular orbit — use the epoch position itself as the angle-zero
            // reference, so the epoch true anomaly is trivially zero.
            periapsisDirection = new Vector3((float) (rx / rMag), (float) (ry / rMag), (float) (rz / rMag));
            trueAnomaly = Angle.Zero;
            eccentricity = 0.0;
        }
        else
        {
            periapsisDirection = new Vector3(
                (float) (ex / eccentricity), (float) (ey / eccentricity), (float) (ez / eccentricity));

            var cosTrueAnomaly = Math.Clamp(
                ((ex * rx) + (ey * ry) + (ez * rz)) / (eccentricity * rMag), -1.0, 1.0);
            var angle = Math.Acos(cosTrueAnomaly);
            var movingAwayFromPeriapsis = (rx * vx) + (ry * vy) + (rz * vz) >= 0.0;
            trueAnomaly = Angle.FromRadians(movingAwayFromPeriapsis ? angle : (2.0 * Math.PI) - angle);
        }

        return new OrbitC
        {
            Eccentricity = eccentricity,
            MeanAnomaly = OrbitalMechanics.TrueToMeanAnomaly(trueAnomaly, eccentricity),
            OrbitalAngle = trueAnomaly,
            OrbitalPeriod = Duration.FromSeconds(orbitalPeriod),
            SemiMajorAxis = Length.FromMeters(semiMajorAxis),
            DistanceFromStar = Length.FromMeters(rMag),
            PeriapsisDirection = periapsisDirection,
            OrbitNormal = normal
        };
    }
}
