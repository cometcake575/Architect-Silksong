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
}