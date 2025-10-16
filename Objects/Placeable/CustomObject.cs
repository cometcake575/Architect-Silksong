using System;
using UnityEngine;

namespace Architect.Objects.Placeable;

public sealed class CustomObject : PlaceableObject
{
    public CustomObject(string name,
        string id,
        GameObject prefab,
        string description = null,
        Action<GameObject> postSpawnAction = null,
        bool preview = false,
        Sprite sprite = null,
        Sprite uiSprite = null) : base(name,
        id,
        description,
        postSpawnAction,
        preview,
        sprite,
        uiSprite) => FinishSetup(prefab);
}