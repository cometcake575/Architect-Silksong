using GlobalEnums;
using UnityEngine;

namespace Architect.Behaviour.Custom;

public class CustomDamager : MonoBehaviour
{
    public int damageAmount = 1;
    public DamagePropertyFlags flags = DamagePropertyFlags.None;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var controller = other.gameObject.GetComponent<HeroController>();
        if (!controller) return;
        controller.TakeDamage(
            gameObject,
            CollisionSide.other,
            damageAmount,
            HazardType.SPIKES,
            flags
        );
    }
}