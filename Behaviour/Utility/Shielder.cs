using Architect.Placements;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class Shielder : MonoBehaviour
{
    public string id;
    
    public bool immuneToBeams;
    public bool immuneToCoal;
    public bool immuneToExplosions;
    public bool immuneToHunterWeapon;
    public bool immuneToLava;
    public bool immuneToNailAttacks;
    public bool immuneToSpikes;
    public bool immuneToTraps;
    public bool immuneToWater;

    private bool _shielded;

    public void Shield()
    {
        if (_shielded) return;

        _shielded = true;
        
        if (!PlacementManager.Objects.TryGetValue(id, out var target)) return;
        
        var dupe = target.GetComponent<ObjectDuplicator>();
        if (dupe) dupe.shielder = this;
        else Shield(target);
    }

    public void Shield(GameObject target)
    {
        var hm = target.GetComponent<HealthManager>();
        if (!hm) return;

        hm.immuneToBeams = immuneToBeams;
        hm.immuneToCoal = immuneToCoal;
        hm.immuneToExplosions = immuneToExplosions;
        hm.immuneToHunterWeapon = immuneToHunterWeapon;
        hm.immuneToLava = immuneToLava;
        hm.immuneToNailAttacks = immuneToNailAttacks;
        hm.immuneToSpikes = immuneToSpikes;
        hm.immuneToTraps = immuneToTraps;
        hm.immuneToWater = immuneToWater;
    }

    private void Update()
    {
        if (!_shielded) Shield();
    }
}