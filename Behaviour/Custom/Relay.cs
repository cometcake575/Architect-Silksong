using Architect.Events;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Architect.Behaviour.Custom;

public class Relay : MonoBehaviour
{
    public bool canCall = true;

    public bool startActivated = true;
    public bool semiPersistent;
    [CanBeNull] public string id;
    public float relayChance = 1;
    public bool multiplayerBroadcast;
    public float delay;
    public bool broadcastImmediately;

    private PersistentRelayItem _item;
    private float _schedule = -1;
    private bool _shouldRelay;

    private void Awake()
    {
        _shouldRelay = startActivated;
        if (string.IsNullOrEmpty(id)) return;

        _item = gameObject.AddComponent<PersistentRelayItem>();
        _item.defaultValue = startActivated;

        _item.OnSetSaveState += value =>
        {
            _shouldRelay = value;
            if (value && broadcastImmediately) DoRelay();
        };

        _item.OnGetSaveState += (out bool value) =>
        {
            value = _shouldRelay;
        };
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(id)) return;
        
        _item.ItemData.IsSemiPersistent = semiPersistent;
        _item.ItemData.SceneName = "Universal";
        _item.ItemData.ID = id;
    }

    private void Update()
    {
        if (broadcastImmediately && string.IsNullOrEmpty(id))
        {
            DoRelay();
            broadcastImmediately = false;
        }
        
        canCall = true;
        if (_schedule > 0)
        {
            _schedule -= Time.deltaTime;
            if (_schedule <= 0) EventManager.BroadcastEvent(gameObject, "OnCall", multiplayerBroadcast);
        }
    }

    public bool ShouldRelay()
    {
        if (Random.value > relayChance) return false;
        return canCall && _shouldRelay;
    }

    public void DoRelay()
    {
        if (!ShouldRelay()) return;
        canCall = false;
        if (delay <= 0) EventManager.BroadcastEvent(gameObject, "OnCall", multiplayerBroadcast);
        else _schedule = delay;
    }

    public void EnableRelay()
    {
        _shouldRelay = true;
    }

    public void DisableRelay()
    {
        _shouldRelay = false;
    }
}

public class PersistentRelayItem : PersistentBoolItem
{
    public override bool DefaultValue => defaultValue;

    public bool defaultValue;
}
