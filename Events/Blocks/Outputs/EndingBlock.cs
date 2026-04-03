using System;
using Architect.Storage;
using Architect.Utils;
using UnityEngine.UI;

namespace Architect.Events.Blocks.Outputs;

using System.Collections.Generic;
using UnityEngine;

public class EndingBlock : LocalBlock
{
    protected override IEnumerable<string> Inputs => ["GrantEnding", "PlayCredits"];

    private static readonly Color DefaultColor = new(0.2f, 0.4f, 0.8f);
    protected override Color Color => DefaultColor;
    protected override string Name => "Ending Control";

    public string CustomSprite = string.Empty;
    public bool Point;

    public static void Init()
    {
        typeof(SaveSlotCompletionIcons).Hook(nameof(SaveSlotCompletionIcons.Awake),
            (Action<SaveSlotCompletionIcons> orig, SaveSlotCompletionIcons self) =>
            {
                orig(self);
                
                var newIcon =
                    Object.Instantiate(self.completionIcons[0].icon, 
                    self.transform, 
                    true);
                newIcon.name = "Custom Icon";
                var img = newIcon.GetComponent<Image>();
                img.sprite = ArchitectPlugin.BlankSprite;
                self.gameObject.AddComponent<CustomSaveArtData>().img = img;
            });
        
        typeof(SaveSlotCompletionIcons).Hook(nameof(SaveSlotCompletionIcons.SetCompletionIconState),
            (Action<SaveSlotCompletionIcons, SaveStats> orig, SaveSlotCompletionIcons self, SaveStats stats) =>
            {
                orig(self, stats);
                var btn = self.GetComponentInParent<SaveSlotButton>();
                if (!btn) return;
                var sad = self.GetComponent<CustomSaveArtData>();
                if (GlobalArchitectData.Instance.CustomSaveArt.TryGetValue(btn.SaveSlotIndex,
                        out var art))
                {
                    self.gameObject.SetActive(true);
                    var img = sad.img;
                    img.sprite = ArchitectPlugin.BlankSprite;
                    CustomAssetManager.DoLoadSprite(art.Item1, 
                        art.Item2, 100, 1, 1, 
                        s => img.sprite = s[0]);
                    foreach (var icon in self.completionIcons) icon.icon.SetActive(false);
                    sad.img.gameObject.SetActive(true);
                } else sad.img.gameObject.SetActive(false);
            });
    }
    
    protected override void Trigger(string id)
    {
        if (id == "GrantEnding")
        {
            var index = GameManager.instance.profileID;
            GlobalArchitectData.Instance.CustomSaveArt[index] = (CustomSprite, Point);
        } 
        else GameManager.instance.ChangeToScene("End_Credits", "", 0);
    }

    public class CustomSaveArtData : MonoBehaviour
    {
        public Image img;
    }
}
