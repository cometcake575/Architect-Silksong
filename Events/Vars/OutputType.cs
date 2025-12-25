using System;
using UnityEngine;

namespace Architect.Events.Vars;

public abstract class OutputType(string id, string name)
{
    public readonly string Id = id;
    
    public readonly string Name = name;

    public abstract object GetDefaultValue();
    
    public abstract object GetValue(GameObject obj);

    public abstract string GetTypeId();
}

public abstract class OutputType<T>(string id, string name, Func<GameObject, T> check) : OutputType(id, name)
{
    public readonly Func<GameObject, T> Check = check;

    public override object GetValue(GameObject obj)
    {
        return Check(obj);
    }
}
