using System.Collections.Generic;
using Architect.Events;
using Architect.Multiplayer.Ssmp;
using Architect.Placements;
using UnityEngine;

namespace Architect.Multiplayer;

public abstract class CoopManager
{
    public static CoopManager Instance;
    
    public static void Init()
    {
        if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("ssmp")) SsmpManagerInitializer.Init();
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
    
    public abstract void ShareScene(string room);
    
    public abstract void RefreshRoom();
    
    public abstract void ShareEvent(string room, string name);
}