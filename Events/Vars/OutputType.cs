using System;
using UnityEngine;

namespace Architect.Events.Vars;

public class OutputType(string id, string name, string typeId, Func<GameObject, object> check)
{
    public readonly string Id = id;
    
    public readonly string Name = name;

    public object GetValue(GameObject obj) => check(obj);

    public string GetTypeId() => typeId;
}
