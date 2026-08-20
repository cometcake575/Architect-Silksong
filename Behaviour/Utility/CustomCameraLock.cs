using UnityEngine;

namespace Architect.Behaviour.Utility;

public class CustomCameraLock : PreviewableBehaviour
{
    public Vector2 lockZone = Vector2.one;
    public Vector2 lockOffset;
    public Vector2 boxZone = Vector2.one;
    public Vector2 boxOffset;

    public GameObject lockPreview;
    public GameObject boxPreview;
    public SpriteRenderer renderer;
    public SpriteRenderer lockPreviewRenderer;
    public SpriteRenderer boxPreviewRenderer;

    public BoxCollider2D collider;
    public CameraLockArea cameraLockArea;

    private void Start() => Setup();
    
    public void Setup()
    {
        if (isAPreview)
        {
            lockPreview.transform.localScale = lockZone;
            boxPreview.transform.localScale = boxZone;
            lockPreview.transform.SetLocalPosition2D(lockOffset);
            boxPreview.transform.SetLocalPosition2D(boxOffset);

            lockPreviewRenderer.enabled = true;
            boxPreviewRenderer.enabled = true;
            renderer.enabled = true;
            collider.enabled = false;
        }
        else
        {
            lockPreview.SetActive(false);
            boxPreview.SetActive(false);
            collider.offset = boxOffset;
            collider.size = boxZone;

            cameraLockArea.cameraXMin = transform.GetPositionX() - lockZone.x / 2 + lockOffset.x;
            cameraLockArea.cameraXMax = transform.GetPositionX() + lockZone.x / 2 + lockOffset.x;
            cameraLockArea.cameraYMin = transform.GetPositionY() - lockZone.y / 2 + lockOffset.y;
            cameraLockArea.cameraYMax = transform.GetPositionY() + lockZone.y / 2 + lockOffset.y;
            cameraLockArea.enabled = true;
        }
    }
}