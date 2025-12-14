using UnityEngine;

namespace Architect.Behaviour.Fixers;

public class SemiPersistentBool : MonoBehaviour
{
    public bool semiPersistent;
    
    private void Start()
    {
        GetComponent<PersistentBoolItem>().ItemData.IsSemiPersistent = semiPersistent;
    }
}