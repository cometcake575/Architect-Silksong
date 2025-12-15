using UnityEngine;

namespace Architect.Behaviour.Fixers;

public class SemiPersistentBool : MonoBehaviour
{
    public bool semiPersistent;
    
    private void Start()
    {
        var item1 = GetComponent<PersistentBoolItem>();
        var item2 = GetComponent<PersistentIntItem>();
        if (item1) item1.ItemData.IsSemiPersistent = semiPersistent;
        if (item2) item2.ItemData.IsSemiPersistent = semiPersistent;
    }
}