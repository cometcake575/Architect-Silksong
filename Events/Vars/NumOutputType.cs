using System;
using UnityEngine;

namespace Architect.Events.Vars;

public class NumOutputType(string id, string name, Func<GameObject, float> check) : OutputType<float>(id, name, check)
{
    public override object GetDefaultValue()
    {
        return 0;
    }

    public override string GetTypeId() => "Number";
}