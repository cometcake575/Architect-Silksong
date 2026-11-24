using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Architect.Behaviour.Fixers;
using Architect.Placements;
using Architect.Prefabs;
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
    public string deathEffectsProfile;

    public string overrideJournalRecord;

    public string overrideCorpse = string.Empty;

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

    private static readonly List<DeathEffectName> DeathEffects =
    [
        "Ant",
        "BigEnemy",
        "Bone",
        "Coral",
        "Crystal",
        "Lifeblood",
        "Moss",
        new("Pure Thread Death Effect NoSteam.asset", "Pure Thread NoSteam"),
        "Pure Thread",
        "Regular",
        "Song Automaton",
        "Song Automaton Tiny",
        "Tar",
        "Uninfected",
        new("Unthreaded Coral Death Effect Grey.asset", "Unthreaded Coral Grey"),
        new("Unthreaded Coral Death Effect Orange.asset", "Unthreaded Coral Orange"),
        "Unthreaded Coral",
        "Unthreaded Enemy",
        "Unthreaded NPC",
        "Unthreaded Void",
        "Void",
        "Wood",
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

    public class DeathEffectName(string name, string alias)
    {
        public readonly string Name = name;
        public readonly string Alias = alias;
        
        public static implicit operator DeathEffectName(string s)
        {
            return new DeathEffectName($"{s} Death Effect.asset", s);
        }
    }

    private static readonly Dictionary<string, ManagedAsset<EnemyHitEffectsProfile>> HitProfiles = [];
    private static readonly Dictionary<string, ManagedAsset<EnemyDeathEffectsProfile>> DeathProfiles = [];

    public static void Init()
    {
        foreach (var k in HitEffects)
        {
            HitProfiles[k.Alias] = ManagedAsset<EnemyHitEffectsProfile>.FromNonSceneAsset(
                $"Assets/Data Assets/Profiles/Hit Effects/{k.Name}",
                "dataassets_assets_assets/dataassets/profiles"
            );
        }
        foreach (var k in DeathEffects)
        {
            DeathProfiles[k.Alias] = ManagedAsset<EnemyDeathEffectsProfile>.FromNonSceneAsset(
                $"Assets/Data Assets/Profiles/Death Effects/{k.Name}",
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
        
        var deathEffects = _enemy.GetComponentInChildren<EnemyDeathEffectsRegular>(true);
        if (deathEffects)
        {
            StartCoroutine(PrepareDeath(deathEffects));
            SetJournalRecord(deathEffects);
        }

        var hitEffects = _enemy.GetComponentInChildren<EnemyHitEffectsRegular>(true);
        if (hitEffects)
        {
            StartCoroutine(PrepareHit(hitEffects));
            PrepareBlood(hitEffects);
        }
    }

    public void ActivateCorpse()
    {
        if (!_enemy) return;

        var corpse = GetCorpse();
        if (!corpse) return;
        
        corpse.transform.SetParent(null);
        corpse.SetActive(true);
    }

    private void SetJournalRecord(EnemyDeathEffectsRegular deathEffects)
    {
        if (overrideJournalRecord.IsNullOrWhiteSpace()) return;
        var entry = EnemyJournalManager.Instance.recordList.list
            .FirstOrDefault(o => o.name == overrideJournalRecord);
        deathEffects.journalRecord = entry;
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
            HitProfiles.TryGetValue(hitEffectsProfile, out var hProfile))
        {
            var handle = hProfile.Load();
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
    
    private IEnumerator PrepareDeath(EnemyDeathEffectsRegular deathEffects)
    {
        if (!deathEffectsProfile.IsNullOrWhiteSpace() &&
            DeathProfiles.TryGetValue(deathEffectsProfile, out var dProfile))
        {
            var handle = dProfile.Load();
            yield return handle;
        
            deathEffects.profile = handle.Result;
        }
    }

    private void PrepareCorpse()
    {
        switch (corpseMode)
        {
            case -1:
                return;
            case 4:
            {
                var ede = _enemy.GetComponentInChildren<EnemyDeathEffects>(true);
                if (!ede) return;
                ede.corpsePrefab = null;
                
                var prefabObj = new GameObject($"[Architect] Corpse Prefab Spawner {_enemy.name}")
                {
                    transform = { 
                        parent = _enemy.transform,
                        localPosition = Vector3.zero
                    }
                };
                prefabObj.SetActive(false);
                var prefab = prefabObj.AddComponent<Prefab>();
                prefab.id = overrideCorpse;
                
                ede.instantiatedCorpses = [prefabObj];
                return;
            }
        }

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