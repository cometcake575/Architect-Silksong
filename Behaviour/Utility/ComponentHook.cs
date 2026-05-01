using System.Linq;
using System.Reflection;
using Architect.Placements;
using Architect.Utils;
using Mono.WebBrowser;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class ComponentHook : MonoBehaviour
{
    public bool recursive;
    public string id;
    public string componentName = string.Empty;
    public int mode;

    public string fieldName = string.Empty;
    private FieldInfo _fieldInfo;
    
    private bool _done;

    private UnityEngine.Behaviour[] _components;

    public void Setup()
    {
        if (!PlacementManager.TryGetValue(id, out var target))
        {
            target = ObjectUtils.FindGameObject(id);
            if (!target) return;
        }
        
        _components = (recursive ?
            target.GetComponentsInChildren<UnityEngine.Behaviour>() : 
            target.GetComponents<UnityEngine.Behaviour>()).Where(c => c.GetType().Name == componentName)
            .ToArray();
        
        foreach (var c in _components)
        {
            if (mode != 3)
            {
                if (mode == 0) Destroy(c);
                else c.enabled = mode == 2;
            }

            if (_fieldInfo == null)
            {
                _fieldInfo = c.GetType().GetField(fieldName,
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            }
        }
    }

    public void SetValue(object data)
    {
        if (_fieldInfo == null) return;
        if (data is float f)
        {
            if (_fieldInfo.FieldType == typeof(int)) data = (int)f;
            else if (_fieldInfo.FieldType == typeof(double)) data = (double)f;
        }
        if (data.GetType() != _fieldInfo.FieldType) return;
        try
        {
            foreach (var c in _components)
            {
                if (c) _fieldInfo.SetValue(c, data);
            }
        }
        catch (Exception) { }
    }

    private void Update()
    {
        if (_done) return;
        _done = true;
        Setup();
    }
}