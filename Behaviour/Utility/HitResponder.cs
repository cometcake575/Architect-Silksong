using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class HitResponder : MonoBehaviour, IHitResponder
{
    public static readonly Sprite SquareZone =
        ResourceUtils.LoadSpriteResource("hit_responder", FilterMode.Point, ppu: 10);
    public static readonly Sprite CircleZone =
        ResourceUtils.LoadSpriteResource("hit_responder_circle", FilterMode.Point, ppu: 10);

    public IHitResponder.HitResponse Hit(HitInstance damageInstance)
    {
        gameObject.BroadcastEvent("OnHit");
        return IHitResponder.HitResponse.Default;
    }
}