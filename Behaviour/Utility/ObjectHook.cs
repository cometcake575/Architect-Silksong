using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class ObjectHook : MonoBehaviour
{
    public string path;
    public GameObject o;

    private void Start()
    {
        o = ObjectUtils.FindGameObject(path);
    }
}