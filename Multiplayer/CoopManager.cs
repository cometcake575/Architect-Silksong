using System.Collections.Generic;
using Architect.Editor;
using Architect.Multiplayer.Ssmp;
using Architect.Placements;
using UnityEngine;

namespace Architect.Multiplayer;

public abstract class CoopManager
{
    public static CoopManager Instance;
    
    public static void Init()
    {
        if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("ssmp"))
        {
            SsmpManagerInitializer.Init();
            
            EditManager.ToggleChecks.Add(() =>
            {
                switch (Instance.LockType)
                {
                    case EditorLockType.Unknown:
                        Instance.ShowMessage("Cannot toggle edit mode yet, waiting for server info");
                        return false;
                    case EditorLockType.Locked:
                        Instance.ShowMessage("You do not have permission to toggle edit mode in this server");
                        return false;
                    case EditorLockType.Unlocked:
                    case EditorLockType.AdminsOnly:
                    default:
                        return true;
                }
            });
        }
        else Instance = new DummyManager();
    }

    public abstract string Name { get; }
    
    public abstract bool IsActive();
    
    public abstract void ResetRoom(string room);
    
    public abstract void MoveObjects(string room, List<(string, Vector3)> movements);
    
    public abstract void EraseObjects(string room, List<string> ids);
    
    public abstract void ToggleTiles(string room, List<(int, int)> tiles, bool empty);
    
    public abstract void ToggleLock(string room, string id);
    
    public abstract void PlaceObjects(string room, List<ObjectPlacement> placements);
    
    public abstract void ShareScene(string room, bool scriptOnly, LevelData data);
    
    public abstract void RefreshRoom();
    
    public abstract void ShowMessage(string message);

    public EditorLockType LockType = EditorLockType.Unlocked;
    
    public abstract void ShareEvent(string room, string name);
}