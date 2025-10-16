namespace Architect.Objects.Groups;

public enum RotationGroup
{
    None, // No rotation
    Vertical, // 180 rotation
    Three, // 0, 90 and 270 rotation
    Four, // 0, 90, 180 and 270 rotation
    Eight, // 0, 45, 90, 135, 180, 225, 270, 315 rotation
    All // Every degree of rotation
}