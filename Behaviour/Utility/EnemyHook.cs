using System.Linq;
using Architect.Events.Blocks.Objects;
using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class EnemyHook : MonoBehaviour
{
    public string path;
    public HealthManager hm;
    
    private void Start()
    {
        DoCheck();
    }

    public void Set(HealthManager manager)
    {
        if (!manager) return;
        hm = manager;
        Setup();
    }

    public void DoCheck()
    {
        var obj = ObjectUtils.FindGameObject(path);
        if (obj) hm = obj.GetComponent<HealthManager>();

        if (!hm) return;
        Setup();
    }

    private void Setup()
    {
        foreach (var block in GetComponents<ObjectBlock.ObjectBlockReference>())
        {
            if (hm.GetComponents<ObjectBlock.ObjectBlockReference>()
                .FirstOrDefault(o => o.Block == block.Block)) continue;
            hm.gameObject.AddComponent<ObjectBlock.ObjectBlockReference>().Block = block.Block;
            block.Spawns.Add(hm.gameObject);
        }
    }
}