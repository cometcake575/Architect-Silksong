using System.Collections;
using UnityEngine;

namespace Architect.Behaviour.Utility;

public class RoarEffect : MonoBehaviour
{
    public float time;
    public bool small;
    
    private GameObject _roar;
    
    private void Start()
    {
        _roar = Instantiate(GameManager.instance.gameCams.gameObject.transform
            .Find(small ? "Roar Wave Emitter Small" : "Roar Wave Emitter").gameObject);
        _roar.transform.position = transform.position;
    }
    
    public void DoRoar()
    {
        StartCoroutine(Roar());
    }

    public IEnumerator Roar()
    {
        FSMUtility.SendEventToGameObject(_roar, "START");
        yield return new WaitForSeconds(time);
        FSMUtility.SendEventToGameObject(_roar, "END");
    }
}