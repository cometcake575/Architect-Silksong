using System.Collections.Generic;
using System.Linq;
using Architect.Prefabs;
using UnityEngine;

namespace Architect.Events.Blocks.Objects;

public class ObjectMoverBlock : ScriptBlock
{
    protected override IEnumerable<string> Inputs => [
        "MoveTo", 
        "MoveBy", 
        "AddForce", 
        "SetVelocity", 
        "SetRotation", 
        "AddRotation",
        "RotateAround",
        "PointAt"
    ];

    protected override IEnumerable<(string, string)> InputVars =>
    [
        Space,
        Space,
        ("Target", "Object"),
        ("X", "Number"),
        ("Y", "Number"),
        ("Z", "Number"),
        ("Angle", "Number")
    ];
    
    protected override IEnumerable<(string, string)> OutputVars =>
    [
        ("Pos X", "Number"),
        ("Pos Y", "Number"),
        ("Pos Z", "Number"),
        ("Rotation", "Number"),
        ("Force X", "Number"),
        ("Force Y", "Number")
    ];
    
    public override Color Color => ObjectBlock.ValidColor;
    protected override string Name => "Move Object";

    protected override void Trigger(string trigger)
    {
        var obj = GetVariable<GameObject>("Target");
        if (!obj) return;
        var rot = GetVariable<float>("Angle");
        var x = GetVariable<float>("X");
        var y = GetVariable<float>("Y");
        var z = GetVariable<float>("Z");
        
        IEnumerable<Rigidbody2D> rb2ds = [obj.GetComponentInChildren<Rigidbody2D>()];

        var prefab = obj.GetComponent<Prefab>();

        switch (trigger)
        {
            case "MoveTo":
                var tx = GetVariable<float>("X", obj.transform.GetPositionX());
                var ty = GetVariable<float>("Y", obj.transform.GetPositionY());
                var tz = GetVariable<float>("Z", obj.transform.GetPositionZ());

                var pos = new Vector3(tx, ty, tz);
                if (prefab) prefab.Move(pos);
                else obj.transform.position = pos;
                
                break;
            case "MoveBy":
                var target = obj.transform.position + new Vector3(x, y, z);

                if (prefab) prefab.Move(target);
                else obj.transform.position = target;

                break;
            case "AddForce":
                if (prefab) rb2ds = prefab.spawns.SelectMany(o => o.GetComponentsInChildren<Rigidbody2D>());
                foreach (var rb2d in rb2ds)
                {
                    rb2d.linearVelocityX += x;
                    rb2d.linearVelocityY += y;
                }

                break;
            case "SetVelocity":
                if (prefab) rb2ds = prefab.spawns.SelectMany(o => o.GetComponentsInChildren<Rigidbody2D>());
                foreach (var rb2d in rb2ds)
                {
                    var vx = GetVariable<float>("X", rb2d.linearVelocityX);
                    var vy = GetVariable<float>("Y", rb2d.linearVelocityY);
                    rb2d.linearVelocityX = vx;
                    rb2d.linearVelocityY = vy;
                }

                break;
            case "SetRotation":
                if (prefab) prefab.SetRotation(rot);
                else obj.transform.SetRotation2D(rot);

                break;
            case "AddRotation":
                if (prefab) prefab.SetRotation(prefab.rot + rot);
                else obj.transform.SetRotation2D(obj.transform.GetRotation2D() + rot);

                break;
            case "PointAt":
                var newRot = Mathf.Atan((y - obj.transform.GetPositionY()) / (x - obj.transform.GetPositionX())) *
                    Mathf.Rad2Deg + rot;
                if (x - obj.transform.GetPositionX() < 0) newRot += 180;
                if (prefab) prefab.SetRotation(newRot);
                else obj.transform.SetRotation2D(newRot);
                
                break;
        }
    }

    public override object GetValue(string id)
    {
        var obj = GetVariable<GameObject>("Target");
        if (!obj) return 0;
        switch (id)
        {
            case "Force X":
                var xrb2d = obj.GetComponent<Rigidbody2D>();
                if (!xrb2d) return 0;
                return xrb2d.linearVelocityX;
            case "Force Y":
                var yrb2d = obj.GetComponent<Rigidbody2D>();
                if (!yrb2d) return 0;
                return yrb2d.linearVelocityY;
        }
        
        return id switch
        {
            "Pos X" => obj.transform.GetPositionX(),
            "Pos Y" => obj.transform.GetPositionY(),
            "Pos Z" => obj.transform.GetPositionZ(),
            "Rotation" => obj.transform.GetRotation2D(),
            _ => 0
        };
    }
}
