using System.Collections.Generic;
using System.Linq;
using Architect.Objects.Categories;
using Architect.Objects.Groups;
using Architect.Objects.Placeable;
using Architect.Utils;
using TeamCherry.Splines;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Architect.Content.Custom;

public static class SplineObjects
{
    public static void Init()
    {
        Categories.Utility.Add(CreateStartNode());
        Categories.Utility.Add(CreateNode());
    }

    private static PlaceableObject CreateStartNode()
    {
        var node = new GameObject("Start Node");
        node.SetActive(false);
        Object.DontDestroyOnLoad(node);

        node.AddComponent<Spline>();

        return new CustomObject("Track Start Point", "start_node",
                node,
                preview: true,
                sprite: ResourceUtils.LoadSpriteResource("track_start", FilterMode.Point, ppu:15),
                description: "The start of a track.\n" +
                             "Place Track Points with the same Track ID to link together and form a track.\n\n" +
                             "Setting a track point as the parent of an Object Anchor will cause it to\n" +
                             "follow the track, starting at that point.")
            .WithConfigGroup(ConfigGroup.TrackStartPoint);
    }

    private static PlaceableObject CreateNode()
    {
        var node = new GameObject("Node");
        node.SetActive(false);
        Object.DontDestroyOnLoad(node);

        node.AddComponent<SplinePoint>();

        return new CustomObject("Track Point", "node",
                node,
                preview: true,
                sprite: ResourceUtils.LoadSpriteResource("track_node", FilterMode.Point, ppu:15),
                description: "A point on a track.\n" +
                             "Use the Track Start Point to start a track.")
            .WithConfigGroup(ConfigGroup.TrackPoint)
            .WithReceiverGroup([]);
    }

    private static readonly Dictionary<string, Spline> Splines = [];

    public class SplinePoint : MonoBehaviour
    {
        public string id;
        public bool hasSetup;

        public Spline spline;

        private Transform _point;
        
        protected virtual void Setup()
        {
            spline = Splines[id];
            if (!spline) return;

            hasSetup = true;
            var point = new GameObject("Point") { transform =
            {
                parent = spline.transform,
                position = transform.position
            } };
            var ssp = point.AddComponent<SpawnedSplinePoint>();
            ssp.source = this;

            _point = point.transform;
            
            spline.points.Add(_point);
            spline.splines.Add(this);
        }

        private void Update()
        {
            if (!hasSetup && Splines.ContainsKey(id)) Setup();
        }

        private void OnDisable()
        {
            if (!Splines.TryGetValue(id, out var spl)) return;

            if (!spl) return;
            
            hasSetup = false;
            
            if (spl.points.Count < 3) spl.Deactivate();
            
            spl.points.Remove(_point);
            spl.splines.Remove(this);
            
            Destroy(_point.gameObject);
        }
    }

    public class SpawnedSplinePoint : MonoBehaviour
    {
        public SplinePoint source;

        private void Update()
        {
            transform.position = source.transform.position.Where(z: source.spline.transform.GetPositionZ());
        }

        private void OnDestroy()
        {
            source.hasSetup = false;
        }
    }

    public class Spline : SplinePoint
    {
        public float r = 1;
        public float g = 1;
        public float b = 1;
        public float a = 1;

        public float speed;
        
        public HermiteSpline actualSpline;
        
        public List<SplinePoint> splines = [];
        public List<Transform> points = [];

        private Transform _startPoint;

        private void Start()
        {
            spline = this;
            
            _startPoint = new GameObject("Start Point")
            {
                transform =
                {
                    parent = transform,
                    localPosition = Vector3.zero
                }
            }.transform;
            
            points.Add(_startPoint);
        }

        protected override void Setup()
        {
            if (points.Count < 2) return;

            points = points.OrderBy(o => o != _startPoint).ToList();

            hasSetup = true;
            actualSpline = gameObject.AddComponent<HermiteSpline>();
            actualSpline.preventCulling = true;
            actualSpline.controlPoints = points;
            actualSpline.InternalPoints = [];
            actualSpline.subdivisions = 25;

            var material = MiscObjects.LineMaterial;
            var color = new Color(r, g, b, a);
            if (color != Color.white)
            {
                material = new Material(material) { color = color };
            }

            GetComponent<MeshRenderer>().material = material;
        }

        public void Deactivate()
        {
            hasSetup = false;
            Destroy(actualSpline);
        }

        private void OnEnable()
        {
            if (Splines.ContainsKey(id)) return;
            Splines.Add(id, this);
        }

        private void OnDisable()
        {
            Deactivate();
            Splines.Remove(id);
        }
    }
}