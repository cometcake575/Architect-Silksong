using System.Collections.Generic;
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
        ("Rotation", "Number")
    ];
    
    protected override Color Color => ObjectBlock.ValidColor;
    protected override string Name => "Move Object";

    protected override void Trigger(string trigger)
    {
        var obj = GetVariable<GameObject>("Target");
        if (!obj) return;
        var rot = GetVariable<float>("Angle");
        var x = GetVariable<float>("X");
        var y = GetVariable<float>("Y");
        var z = GetVariable<float>("Z");
        var rb2d = obj.GetComponent<Rigidbody2D>();
        switch (trigger)
        {
            case "MoveTo":
                var tx = GetVariable<float>("X", obj.transform.GetPositionX());
                var ty = GetVariable<float>("Y", obj.transform.GetPositionY());
                var tz = GetVariable<float>("Z", obj.transform.GetPositionZ());
                obj.transform.position = new Vector3(tx, ty, tz);
                break;
            case "MoveBy":
                obj.transform.position += new Vector3(x, y, z);
                break;
            case "AddForce":
                if (!rb2d) return;
                rb2d.linearVelocityX += x;
                rb2d.linearVelocityY += y;
                break;
            case "SetVelocity":
                if (!rb2d) return;
                var vx = GetVariable<float>("X", rb2d.linearVelocityX);
                var vy = GetVariable<float>("Y", rb2d.linearVelocityY);
                rb2d.linearVelocityX = vx;
                rb2d.linearVelocityY = vy;
                break;
            case "SetRotation":
                obj.transform.SetRotation2D(rot);
                break;
            case "AddRotation":
                obj.transform.SetRotation2D(obj.transform.GetRotation2D() + rot);
                break;
            case "PointAt":
                obj.transform.SetRotation2D(
                    Mathf.Atan((y - obj.transform.GetPositionY()) / (x - obj.transform.GetPositionX())) * Mathf.Rad2Deg
                    + rot
                );
                break;
        }
    }

    protected override object GetValue(string id)
    {
        var obj = GetVariable<GameObject>("Target");
        if (!obj) return 0;
        return id switch
        {
            "Pos X" => obj.transform.GetPositionX(),
            "Pos Y" => obj.transform.GetPositionY(),
            "Pos Z" => obj.transform.GetPositionZ(),
            "Angle" => obj.transform.GetRotation2D(),
            _ => 0
        };
    }
}
