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
        var obj = ObjectUtils.GetGameObjectFromArray(gameObject.scene.GetRootGameObjects(), path);
        if (obj) hm = obj.GetComponent<HealthManager>();

        if (!hm) return;
        foreach (var block in GetComponents<ObjectBlock.ObjectBlockReference>())
        {
            obj.AddComponent<ObjectBlock.ObjectBlockReference>().Block = block.Block;
            block.Spawns.Add(obj);
        }
    }
}