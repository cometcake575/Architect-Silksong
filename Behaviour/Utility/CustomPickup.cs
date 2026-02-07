using System;
using Architect.Utils;
using GlobalSettings;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class CustomPickup : MonoBehaviour
{
    public string item = "";
    public bool ignoreObtained;
    public bool touch;
    public int persistence;

    private CollectableItemPickup _itemPickup;

    public static void Init()
    {
        typeof(CollectableItemPickup).Hook(nameof(CollectableItemPickup.CheckActivation),
            (Action<CollectableItemPickup> orig, CollectableItemPickup self) =>
            {
                var comp = self.GetComponentInParent<CustomPickup>();
                if (comp && comp.ignoreObtained) if (!self.activatedRead) return;
                orig(self);
            });
        
        typeof(CollectableItemPickup).Hook(nameof(CollectableItemPickup.DoPickupAction),
            (Func<CollectableItemPickup, bool, bool> orig, CollectableItemPickup self, bool breakIfAtMax) =>
            {
                var pickup = self.GetComponentInParent<CustomPickup>();
                if (pickup)
                {
                    breakIfAtMax = true;
                    pickup.gameObject.BroadcastEvent("BeforePickup");
                }
                return orig(self, breakIfAtMax);
            });
    }

    private void Start()
    {
        _itemPickup = Instantiate(
            touch ? Gameplay.CollectableItemPickupInstantPrefab : Gameplay.CollectableItemPickupPrefab, 
            gameObject.transform);
        _itemPickup.transform.SetLocalPosition2D(Vector2.zero);
        _itemPickup.name = name + " Pickup";
        switch (persistence)
        {
            case 0:
                _itemPickup.gameObject.RemoveComponent<PersistentBoolItem>();
                break;
            case 1:
                _itemPickup.GetComponent<PersistentBoolItem>().itemData.IsSemiPersistent = true;
                break;
        }

        var savedItem = MiscUtils.GetSavedItem(item);
        if (!savedItem) return;

        _itemPickup.item = savedItem;
    }
}