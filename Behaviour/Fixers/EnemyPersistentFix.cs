using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Architect.Behaviour.Fixers;

public class EnemyPersistentFix : MonoBehaviour
{
    public bool dead;
    public bool semiPersistent;
    public PersistentBoolItem item;
    public HealthManager hm;

    private void Awake()
    {
        item = gameObject.AddComponent<PersistentBoolItem>();

        item.OnSetSaveState += SetSaveState;
        item.OnGetSaveState += GetSaveState;
    }

    private void OnDestroy()
    {
        item.OnSetSaveState -= SetSaveState;
        item.OnGetSaveState -= GetSaveState;
    }

    private void SetSaveState(bool value)
    {
        ArchitectPlugin.Logger.LogInfo("Setting dead as " + value);
        dead = value;
        hm.isDead = true;
        hm.gameObject.SetActive(false);
    }

    private void GetSaveState(out bool value)
    {
        ArchitectPlugin.Logger.LogInfo("Getting dead as " + dead);
        value = dead;
    }

    private void OnDisable()
    {
        if (dead) GameManager.instance.sceneData.persistentBools.SetValue(item.itemData);
    }

    private void Start()
    {
        item.ItemData.IsSemiPersistent = semiPersistent;
        item.ItemData.SceneName = "UNIVERSAL";
        item.ItemData.ID = name;
    }
}

public class EnemyPersistentFixLink : MonoBehaviour
{
    public EnemyPersistentFix epf;
}
