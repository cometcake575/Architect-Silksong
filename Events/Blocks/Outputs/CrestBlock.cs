using System.Collections;
using System.Collections.Generic;
using Architect.Content.Preloads;
using Architect.Utils;
using UnityEngine;

namespace Architect.Events.Blocks.Outputs;

public class CrestBlock : ScriptBlock
{
    private static ToolCrestUIMsg _prefab;
    
    public static void Init()
    {
        PreloadManager.RegisterPreload(new BasicPreload(
            "prompts_assets_all", 
            "Assets/Prefabs/UI/Messages/Tool Crest UI Msg.prefab", 
            o =>
            {
                _prefab = o.GetComponent<ToolCrestUIMsg>();
            }, notSceneBundle: true));

        ToolItemManager.OnEquippedStateChanged += () =>
        {
            References.RemoveAll(r => !r);
            foreach (var r in References) r.Block.Event("OnUpdate");
        };
    }
    
    protected override IEnumerable<string> Inputs => ["Set", "Unlock", "SilentUnlock"];
    protected override IEnumerable<string> Outputs => ["OnUpdate"];
    protected override IEnumerable<(string, string)> OutputVars => [
        ("Equipped", "Boolean"),
        ("Unlocked", "Boolean")
    ];
    
    protected override string Name => "Crest Control";

    private static readonly List<CrestBlockRef> References = [];
    
    public override void SetupReference()
    {
        var cbr = new GameObject("[Architect] Crest Block Ref").AddComponent<CrestBlockRef>();
        cbr.Block = this;
        References.Add(cbr);
    }

    public class CrestBlockRef : MonoBehaviour
    {
        public CrestBlock Block;

        private void OnEnable() => References.Add(this);
        
        private void OnDisable() => References.Remove(this);
    }

    public override void Reset()
    {
        CrestName = "";
    }

    public string CrestName;

    public override object GetValue(string id)
    {
        return PlayerData.instance.CurrentCrestID == CrestName;
    }

    protected override void Trigger(string trigger)
    {
        var crest = ToolItemManager.GetCrestByName(CrestName);
        if (!crest) return;
        switch (trigger)
        {
            case "Set":
                ToolItemManager.AutoEquip(
                    crest,
                    false,
                    false
                );
                break;
            case "Unlock":
                ArchitectPlugin.Instance.StartCoroutine(Unlock(crest));
                break;
            default:
                crest.Unlock();
                break;
        }
    }

    private static IEnumerator Unlock(ToolCrest crest)
    {
        yield return HeroController.instance.FreeControl();
        HeroController.instance.RelinquishControl();
        
        _prefab.Setup(crest);
        ToolCrestUIMsg.Spawn(crest, _prefab, () =>
        {
            crest.Unlock();
            ToolItemManager.AutoEquip(
                crest,
                false,
                false
            );
            HeroController.instance.RegainControl();
        });
    }
}