using System.Collections.Generic;
using System.Linq;
using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Custom;

public class LineObject : MonoBehaviour
{
    private static readonly Dictionary<string, List<LineObject>> Objects = [];
    
    public string id;

    private bool _setup;

    public float r;
    public float g;
    public float b;
    public float a;

    public float width;
    
    private void OnEnable()
    {
        if (!Objects.ContainsKey(id)) Objects[id] = [];
        Objects[id].Add(this);
    }

    private void OnDisable()
    {
        if (!Objects.TryGetValue(id, out var o)) return;
        o.Remove(this);
        if (o.IsNullOrEmpty()) Objects.Remove(id);
    }

    private void Setup()
    {
        _setup = true;
        if (!Objects.TryGetValue(id, out var o)) return;

        var ec = GetComponent<EdgeCollider2D>();

        var points = o.Select(obj =>
        {
            obj._setup = true;
            return obj.transform.position - transform.position;
        }).ToArray();

        var lr = GetComponent<LineRenderer>();
        lr.startColor = lr.endColor = new Color(r, g, b, a);
        lr.startWidth = lr.endWidth = width;
        lr.useWorldSpace = false;

        lr.positionCount = points.Length;
        lr.SetPositions(points);

        ec.points = o.Select(obj =>
        {
            obj._setup = true;
            if (obj.gameObject != gameObject) obj.gameObject.RemoveComponent<EdgeCollider2D>();
            return (Vector2) (obj.transform.position - transform.position);
        }).ToArray();
    }
    
    private void Update()
    {
        if (!_setup) Setup();
    }
}