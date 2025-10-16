using UnityEngine;

namespace Architect.Editor;

public class GroupSelectionBox : MonoBehaviour
{
    public float width = 1f;
    public float height = 1f;
    public float lineThickness = 0.06f;

    private LineRenderer _lineRenderer;

    private void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 4;
        _lineRenderer.loop = true;
        _lineRenderer.useWorldSpace = false;
        _lineRenderer.widthMultiplier = lineThickness;
    }

    public void UpdateOutline()
    {
        Vector3[] corners =
        [
            new(0, 0, 0),
            new(width, 0, 0),
            new(width, height, 0),
            new(0, height, 0)
        ];

        _lineRenderer?.SetPositions(corners);
    }
}