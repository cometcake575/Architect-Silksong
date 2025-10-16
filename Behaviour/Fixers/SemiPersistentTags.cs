using UnityEngine;

namespace Architect.Behaviour.Fixers;

public class SemiPersistentBool : MonoBehaviour
{
    private void Start()
    {
        GetComponent<PersistentBoolItem>().ItemData.IsSemiPersistent = true;
    }
}