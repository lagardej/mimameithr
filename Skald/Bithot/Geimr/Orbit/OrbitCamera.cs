using Godot;

namespace Skald.Bithot.Geimr.Orbit;

/// <summary>Camera3D with right-click-drag orbit and scroll-wheel zoom around the world origin.</summary>
public partial class OrbitCamera : Camera3D
{
    private const float OrbitSensitivity = 0.01f;
    private const float ZoomStep = 0.1f;
    private const float MaxPitch = Mathf.Pi / 2f - 0.05f;
    private float _distance;
    private float _maxDistance;

    private float _minDistance;
    private bool _orbiting;
    private float _pitch = -0.3f;
    private float _yaw;

    /// <summary>Sets orbit distance bounds and resets to a default distance, relative to the orbited target's radius.</summary>
    public void Configure(float targetRadius, float minDistanceFactor = 1.2f, float maxDistanceFactor = 20f)
    {
        _minDistance = targetRadius * minDistanceFactor;
        _maxDistance = targetRadius * maxDistanceFactor;
        _distance = targetRadius * 4f;
        UpdateTransform();
    }

    public override void _Ready()
    {
        Current = true;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        switch (@event)
        {
            case InputEventMouseButton { ButtonIndex: MouseButton.Right } mb:
                _orbiting = mb.Pressed;
                break;
            case InputEventMouseButton { ButtonIndex: MouseButton.WheelUp, Pressed: true }:
                Zoom(-ZoomStep);
                break;
            case InputEventMouseButton { ButtonIndex: MouseButton.WheelDown, Pressed: true }:
                Zoom(ZoomStep);
                break;
            case InputEventMouseMotion motion when _orbiting:
                _yaw -= motion.Relative.X * OrbitSensitivity;
                _pitch = Mathf.Clamp(_pitch - motion.Relative.Y * OrbitSensitivity, -MaxPitch, MaxPitch);
                UpdateTransform();
                break;
        }
    }

    private void Zoom(float factorDelta)
    {
        _distance = Mathf.Clamp(_distance * (1f + factorDelta), _minDistance, _maxDistance);
        UpdateTransform();
    }

    private void UpdateTransform()
    {
        var offset = new Vector3(
            _distance * Mathf.Cos(_pitch) * Mathf.Sin(_yaw),
            _distance * Mathf.Sin(_pitch),
            _distance * Mathf.Cos(_pitch) * Mathf.Cos(_yaw));
        LookAtFromPosition(offset, Vector3.Zero, Vector3.Up);
    }
}