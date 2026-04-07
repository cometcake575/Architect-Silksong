using System;
using System.Collections.Generic;
using Architect.Placements;
using Architect.Utils;
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

    public static void Init()
    {
        typeof(HealthManager).Hook(nameof(HealthManager.IsImmuneTo),
            (Func<HealthManager, HitInstance, bool, bool> orig, 
                HealthManager self,
                HitInstance hit,
                bool full) =>
            {
                var ext = self.GetComponent<ExtraResistance>();
                if (ext && ext.resistances.Contains(hit.AttackType)) return true;
                return orig(self, hit, full);
            });
    }

    public List<AttackTypes> extraImmunities = [];

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
        var hm = target.GetComponentInChildren<HealthManager>();
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

        if (!extraImmunities.IsNullOrEmpty())
            hm.gameObject.AddComponent<ExtraResistance>().resistances = extraImmunities;
    }

    public class ExtraResistance : MonoBehaviour
    {
        public List<AttackTypes> resistances;
    }

    private void Update()
    {
        if (!_shielded) Shield();
    }
}