using Nornir.Engine.Domain;

namespace Nornir.Component.Primosphere.Motion.Dynacore.Fixed;

/// <summary>Store fields produced by the fixed motion dynacore. All values in SI units.</summary>
public static class MotionFixedFields
{
    /// <summary>X position relative to the Primosphere origin, in metres.</summary>
    public static readonly FieldId X = new(MotionFixedDynacore.Key, 0);

    /// <summary>Y position relative to the Primosphere origin, in metres.</summary>
    public static readonly FieldId Y = new(MotionFixedDynacore.Key, 1);

    /// <summary>Z position relative to the Primosphere origin, in metres.</summary>
    public static readonly FieldId Z = new(MotionFixedDynacore.Key, 2);
}
