using Architect.Placements;
using Architect.Utils;
using BepInEx;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class ObjectHook : PreviewableBehaviour
{
    public string path;
    public string prefabPath;
    public string childPath;
    public GameObject o;
    public int start;
    public int index;

    private bool _targetingCustom;

    public void FindObject()
    {
        if (path.IsNullOrWhiteSpace()) return;
        if (!o)
        {
            if (PlacementManager.TryGetValue(prefabPath, out o)) _targetingCustom = true;
            else o = ObjectUtils.FindGameObject(path, index);
            if (!o || childPath.IsNullOrWhiteSpace()) return;
            var splitPath = childPath.Split("/");
            foreach (var s in splitPath)
            {
                var child = o.transform.Find(s);
                if (!child) return;
                o = child.gameObject;
            }
        }
    }

    private void Start()
    {
        FindObject();
        if (!o) return;
        if (isAPreview && _targetingCustom) return;
        
        switch (start)
        {
            case 1:
                o.SetActive(false);
                break;
            case 2:
                o.SetActive(true);
                break;
        }
    }
}