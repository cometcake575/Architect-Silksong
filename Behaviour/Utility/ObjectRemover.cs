using System.Collections;
using System.Collections.Generic;
using Architect.Content.Custom;
using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class ObjectRemover : MonoBehaviour
{
    public string triggerName;
    public string filter;
    public bool all;

    private Disabler[] _toggle = [];

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

public class ObjectEnabler : MonoBehaviour
{
    public string objectPath;
    
    private Enabler _toggle;

    private bool _shouldEnable;
    private bool _setup;
        
    private void OnEnable()
    {
        _shouldEnable = true;
    }

    private void OnDisable()
    {
        if (_toggle) ArchitectPlugin.Instance.StartCoroutine(_toggle.Disable(name));
    }
    
    private void Update()
    {
        if (!_setup)
        {
            _setup = true;
            var o = ObjectUtils.GetGameObjectFromArray(gameObject.scene.GetRootGameObjects(), objectPath);
            if (o)
            {
                if (o.GetComponent<Disabler>()) return;
                _toggle = o.GetOrAddComponent<Enabler>();
            }
        }
        
        if (_shouldEnable)
        {
            _shouldEnable = false;
            DoEnable();
        }
    }

    private void DoEnable()
    {
        if (_toggle) _toggle.Enable(name);
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

public class Enabler : MonoBehaviour
{
    public List<string> enablers = [];

    private bool _disableByDefault;

    private void OnDisable()
    {
        if (enablers.Count > 0)
        {
            _disableByDefault = true;
            gameObject.SetActive(true);
        }
    }

    public void Enable(string enableName)
    {
        enablers.Add(enableName);
        Refresh();
    }

    public IEnumerator Disable(string enableName)
    {
        yield return null;

        if (!this) yield break;
        
        enablers.Remove(enableName);
        Refresh();
    }

    private void Refresh()
    {
        if (enablers.Count == 0)
        {
            if (_disableByDefault) gameObject.SetActive(false);
        }
        else if (!gameObject.activeSelf)
        {
            _disableByDefault = true;
            gameObject.SetActive(true);
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