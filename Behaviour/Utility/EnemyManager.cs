using System.Collections;
using System.Collections.Generic;
using Architect.Behaviour.Fixers;
using Architect.Placements;
using Architect.Utils;
using BepInEx;
using Silksong.AssetHelper.ManagedAssets;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class EnemyManager : MonoBehaviour
{
    public string targetId;
    public int index;
    
    public int corpseMode = -1;
    
    public bool overrideBlood;
    public Color bloodColour = Color.white;

    public bool overrideHit;
    public Color hitColour = Color.white;

    public string hitEffectsProfile;

    private GameObject _enemy;

    private bool _setup;

    private static readonly List<HitEffectName> HitEffects =
    [
        "Ant",
        "BigBone",
        "Bone",
        "Coral Armoured",
        "Coral",
        "Dummy",
        "Fire Ignite",
        "Fire Inert",
        "Grey Thread",
        "Lightning",
        "Moss",
        "Poison Inert",
        "Pure Thread",
        new("Regular Hit Effect Cloakless.asset", "Cloakless"),
        new("Regular Hit Effect Silent.asset", "Silent"),
        "Regular",
        "Scarecrow",
        "Servitor",
        "Song Automaton",
        "Song Automaton Large",
        "Song Automaton Tiny",
        "Uninfected",
        new("Unthreaded Coral Hit Effect Grey.asset", "Unthreaded Coral Grey"),
        new("Unthreaded Coral Hit Effect Orange.asset", "Unthreaded Coral Orange"),
        "Unthreaded Coral",
        "Unthreaded Enemy",
        "Unthreaded NPC",
        "Void",
        "Void Thread",
        "Wood",
        "WoodObject",
        "Wraith"
    ];

    public class HitEffectName(string name, string alias)
    {
        public readonly string Name = name;
        public readonly string Alias = alias;
        
        public static implicit operator HitEffectName(string s)
        {
            return new HitEffectName($"{s} Hit Effect.asset", s);
        }
    }

    private static readonly Dictionary<string, ManagedAsset<EnemyHitEffectsProfile>> Profiles = [];

    public static void Init()
    {
        foreach (var k in HitEffects)
        {
            Profiles[k.Alias] = ManagedAsset<EnemyHitEffectsProfile>.FromNonSceneAsset(
                $"Assets/Data Assets/Profiles/Hit Effects/{k.Name}",
                "dataassets_assets_assets/dataassets/profiles"
            );
        }
    }
    
    private void Update()
    {
        if (_setup) return;
        Setup();
    }
    
    private void Setup()
    {
        _setup = true;
        if (!PlacementManager.TryGetValue(targetId, out _enemy))
        {
            _enemy = ObjectUtils.FindGameObject(targetId, index);
        }
        if (!_enemy) return;

        PrepareCorpse();

        var hitEffects = _enemy.GetComponentInChildren<EnemyHitEffectsRegular>(true);
        if (!hitEffects) return;
        
        StartCoroutine(PrepareHit(hitEffects));
        PrepareBlood(hitEffects);
    }

    public void ActivateCorpse()
    {
        if (!_enemy) return;

        var corpse = GetCorpse();
        if (!corpse) return;
        
        corpse.transform.SetParent(null);
        corpse.SetActive(true);
    }

    private void PrepareBlood(EnemyHitEffectsRegular hitEffects)
    {
        if (!overrideBlood) return;

        hitEffects.bloodColorOverride = bloodColour;
        hitEffects.overrideBloodColor = true;
    }
    
    private IEnumerator PrepareHit(EnemyHitEffectsRegular hitEffects)
    {
        if (!hitEffectsProfile.IsNullOrWhiteSpace() &&
            Profiles.TryGetValue(hitEffectsProfile, out var profile))
        {
            var handle = profile.Load();
            yield return handle;
        
            hitEffects.profile = handle.Result;
        }

        if (overrideHit)
        {
            hitEffects.profile = Instantiate(hitEffects.profile);
            
            hitEffects.profile.overrideHitFlashColor = true;
            hitEffects.profile.hitFlashColor = hitColour;
        }
    }

    private void PrepareCorpse()
    {
        if (corpseMode == -1) return;
        
        var corpse = GetCorpse();
        if (!corpse) return;

        if (corpseMode == 3)
        {
            corpse.AddComponent<MiscFixers.KeepInactive>();
            return;
        }

        var ac = corpse.GetComponent<ActiveCorpse>();
        var col = corpse.GetComponent<Collider2D>();
        switch (corpseMode)
        {
            case 0:
            {
                if (ac) ac.bounceAway = false;
                if (col) col.isTrigger = false;
                break;
            }
            case 1:
            {
                if (ac) ac.bounceAway = true;
                if (col) col.isTrigger = false;
                break;
            }
            case 2:
            {
                if (col) col.isTrigger = true;
                break;
            }
        }
    }

    private GameObject GetCorpse()
    {
        var ede = _enemy.GetComponentInChildren<EnemyDeathEffects>(true);
        return ede ? ede.GetInstantiatedCorpse(AttackTypes.Generic) : null;
    }
}