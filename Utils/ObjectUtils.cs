using System;
using System.Linq;
using Architect.Events;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Architect.Utils;

public static class ObjectUtils
{
    public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
    {
        var comp = obj.GetComponent<T>();
        return comp ? comp : obj.AddComponent<T>();
    }
    
    public static void RemoveComponent<T>(this GameObject obj) where T : Component
    {
        var comp = obj.GetComponent<T>();
        if (comp) Object.Destroy(comp);
    }
    
    public static T ReplaceComponent<T>(this GameObject obj) where T : Component
    {
        obj.RemoveComponent<T>();
        return obj.AddComponent<T>();
    }
    
    public static void BroadcastEvent(this GameObject obj, string triggerName)
    {
        EventManager.BroadcastEvent(obj, triggerName);
    }
    
    public static void RemoveComponentsInChildren<T>(this GameObject obj) where T : Component
    {
        var comps = obj.GetComponentsInChildren<T>(true);
        foreach (var comp in comps) Object.Destroy(comp);
    }
    
    public static string GetPath(this Transform current) {
        if (!current.parent) return current.name;
        return current.parent.GetPath() + "/" + current.name;
    }

    public static GameObject FindGameObject(string path)
    {
        for (var i = 0; i < SceneManager.loadedSceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            var obj = GetGameObjectFromArray(scene.GetRootGameObjects(), path);
            if (obj) return obj;
        }

        return null;
    }
    
    internal static GameObject GetGameObjectFromArray(GameObject[] objects, string objName)
    {
        // Split object name into root and child names based on '/'
        string rootName;
        string childName = null;

        var slashIndex = objName.IndexOf('/');
        if (slashIndex == -1)
        {
            rootName = objName;
        }
        else if (slashIndex == 0 || slashIndex == objName.Length - 1)
        {
            throw new ArgumentException("Invalid GameObject path");
        }
        else
        {
            rootName = objName[..slashIndex];
            childName = objName[(slashIndex + 1)..];
        }

        // Get root object
        var obj = objects.FirstOrDefault(o => o.name == rootName);
        if (!obj) return null;

        // Get child object
        if (childName != null)
        {
            var t = obj.transform.Find(childName);
            return !t ? null : t.gameObject;
        }

        return obj;
    }
}