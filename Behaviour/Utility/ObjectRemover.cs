using System.Collections;
using System.Collections.Generic;
using Architect.Content.Custom;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class ObjectRemover : MonoBehaviour
{
    public string triggerName;

    private Disabler[] _toggle;

    private bool _shouldEnable;
        
    private void OnEnable()
    {
        _shouldEnable = true;
    }

    private void OnDisable()
    {
        foreach (var obj in _toggle) if (obj) ArchitectPlugin.Instance.StartCoroutine(obj.Enable(name));
    }
    
    private void Update()
    {
        if (_shouldEnable)
        {
            _shouldEnable = false;
            DoEnable();
        }
    }

    private void DoEnable()
    {
        _toggle = UtilityObjects.GetObjects(this);
        foreach (var obj in _toggle) if (obj) obj.Disable(name);
    }
}

public class Disabler : MonoBehaviour
{
    public List<string> disablers = [];

    private bool _enableByDefault;

    private void OnEnable()
    {
        if (disablers.Count > 0)
        {
            _enableByDefault = true;
            gameObject.SetActive(false);
        }
    }

    public void Disable(string enableName)
    {
        disablers.Add(enableName);
        Refresh();
    }

    public IEnumerator Enable(string enableName)
    {
        yield return null;

        if (!this) yield break;
        
        disablers.Remove(enableName);
        Refresh();
    }

    private void Refresh()
    {
        if (disablers.Count == 0)
        {
            if (_enableByDefault) gameObject.SetActive(true);
        }
        else if (gameObject.activeSelf)
        {
            _enableByDefault = true;
            gameObject.SetActive(false);
        }
    }
}

public class RoomClearerConfig : MonoBehaviour
{
    public bool removeTransitions;

    public bool removeBenches;

    public bool removeBlur;

    public bool removeMusic;

    public bool removeOther;
}

public class ObjectRemoverConfig : MonoBehaviour
{
    public string objectPath = "";
}