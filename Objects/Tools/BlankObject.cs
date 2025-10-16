using UnityEngine;

namespace Architect.Objects.Tools;

public class BlankObject() : ToolObject("blank", Storage.Settings.Blank, -8)
{
    public static readonly BlankObject Instance = new();
    
    public override string GetName() => "";

    public override string GetDescription() => "";

    public override void Click(Vector3 mousePosition, bool first) { }
}