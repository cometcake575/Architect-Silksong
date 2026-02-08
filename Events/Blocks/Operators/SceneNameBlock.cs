using System.Collections.Generic;
using UnityEngine;

namespace Architect.Events.Blocks.Operators;

public class SceneNameBlock : TimeBlockType
{
    protected override Color Color => Color.yellow;
    protected override string Name => "Scene Name";

    protected override IEnumerable<(string, string)> OutputVars =>
    [
        ("Name", "Text")
    ];

    protected override object GetValue(string id)
    {
        return GameManager.instance.sceneName;
    }
}