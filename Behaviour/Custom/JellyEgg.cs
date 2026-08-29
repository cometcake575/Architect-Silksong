using System.Collections;
using Architect.Behaviour.Fixers;
using Architect.Behaviour.Utility;
using UnityEngine;

namespace Architect.Behaviour.Custom;

public class JellyEgg : PreviewableBehaviour
{
    public GameObject explosion;
    public float regenTime = -1;

    private SpriteRenderer _renderer;
    private Collider2D _col;
    
    private void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _col = GetComponent<Collider2D>();
        _renderer.material = MiscFixers.SpriteMaterial;
    }

    private IEnumerator OnTriggerStay2D(Collider2D other)
    {
        if (isAPreview) yield break;
        if (!_col || !_renderer) yield break;
        if (!other.GetComponentInParent<HeroController>()) yield break;
        
        var expl = Instantiate(explosion);
        expl.transform.position = transform.position;
        expl.SetActive(true);

        _col.enabled = false;
        _renderer.enabled = false;
        
        if (regenTime >= 0)
        {
            yield return new WaitForSeconds(regenTime);
            _col.enabled = true;
            _renderer.enabled = true;
        }
    }
}