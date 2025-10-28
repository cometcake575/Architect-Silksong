using Architect.Behaviour.Fixers;
using Architect.Events;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class TriggerZone : MonoBehaviour
{
    public int mode;
    public int layer;

    private void OnTriggerEnter2D(Collider2D other)
    {
        switch (mode)
        {
            case 0:
                if (!other.gameObject.GetComponent<HeroController>()) return;
                break;
            case 1:
                if (!other.gameObject.GetComponent<NailSlash>()) return;
                break;
            case 2:
                if (!other.gameObject.GetComponent<HealthManager>()) return;
                break;
            case 3:
                var tz = other.gameObject.GetComponent<TriggerZone>();
                if (!tz || tz.layer != layer) return;
                break;
            case 4:
                var kr = other.gameObject.GetComponent<MiscFixers.Kratt>();
                if (!kr || kr.layer != layer) return;
                break;
            case 5:
                var bb = other.gameObject.GetComponent<MiscFixers.BellBaby>();
                if (!bb || bb.layer != layer) return;
                break;
        }

        EventManager.BroadcastEvent(gameObject, "ZoneEnter");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        switch (mode)
        {
            case 0:
                if (!other.gameObject.GetComponent<HeroController>()) return;
                break;
            case 1:
                if (!other.gameObject.GetComponent<NailSlash>()) return;
                break;
            case 2:
                if (!other.gameObject.GetComponent<HealthManager>()) return;
                break;
            case 3:
                if (!other.gameObject.GetComponent<TriggerZone>()) return;
                break;
            case 4:
                if (!other.gameObject.GetComponent<MiscFixers.Kratt>()) return;
                break;
            case 5:
                if (!other.gameObject.GetComponent<MiscFixers.BellBaby>()) return;
                break;
        }

        EventManager.BroadcastEvent(gameObject, "ZoneExit");
    }
}