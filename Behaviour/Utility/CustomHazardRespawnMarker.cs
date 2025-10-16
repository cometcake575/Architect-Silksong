namespace Architect.Behaviour.Utility;

public class CustomHazardRespawnMarker : HazardRespawnMarker
{
    private void Start()
    {
        respawnFacingDirection = transform.GetScaleX() < 0 ? FacingDirection.Right : FacingDirection.Left;
    }
}