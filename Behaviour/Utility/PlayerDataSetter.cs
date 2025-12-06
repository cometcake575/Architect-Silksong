using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class PlayerDataSetter : MonoBehaviour
{
    public string dataName;
    public bool value;

    public void SetValue()
    {
        PlayerData.instance.SetBool(dataName, value);
    }

    public void Relay()
    {
        if (PlayerData.instance.GetBool(dataName) == value) gameObject.BroadcastEvent("OnCall");
    }
}