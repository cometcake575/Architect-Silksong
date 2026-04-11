using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class ObjectHook : MonoBehaviour
{
    public string path;
    public GameObject o;
    public int start;
    public int index;

    private void Start()
    {
        o = ObjectUtils.FindGameObject(path, index);
        switch (start)
        {
            case 1:
                o.SetActive(false);
                break;
            case 2:
                o.SetActive(true);
                break;
        }
    }
}