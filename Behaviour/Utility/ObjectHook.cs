using Architect.Utils;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class ObjectHook : MonoBehaviour
{
    public string path;
    public GameObject o;
    public int start;
    public int index;

    public void FindObject()
    {
        if (!o) o = ObjectUtils.FindGameObject(path, index);
    }

    private void Start()
    {
        FindObject();
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