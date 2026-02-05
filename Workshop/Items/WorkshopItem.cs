using System.Collections.Generic;
using Architect.Workshop.Types;
using UnityEngine;

namespace Architect.Workshop.Items;

public abstract class WorkshopItem
{
    public abstract void Register();
    
    public abstract void Unregister();

    public string Id;

    public string Type;

    public List<ConfigType>[] Config;
    
    public Dictionary<string, ConfigValue> CurrentConfig;

    public abstract Sprite GetIcon();

    public virtual (string, string)[] FilesToDownload => null;
}