using Architect.Behaviour.Custom;
using Architect.Objects.Categories;
using Architect.Objects.Placeable;
using Architect.Utils;
using UnityEngine;

namespace Architect.Content.Custom;

public static class CollectableObjects
{
    public static void Init()
    {
        Categories.Abilities.Add(MakeCherry("Cherry", "Cherries.Normal.normal", 1, false, false));
        Categories.Abilities.Add(MakeCherry("Winged Cherry", "Cherries.Normal.wing", 9, true, false));
        Categories.Abilities.Add(MakeCherry("Golden Cherry", "Cherries.Ding.normal", 1, false, true));
        Categories.Abilities.Add(MakeCherry("Golden Winged Cherry", "Cherries.Ding.wing", 9, true, true));
    }

    private static PlaceableObject MakeCherry(string name, string path, int amount, bool wing, bool ding)
    {
        var obj = new GameObject("Cherry");
        obj.SetActive(false);
        Object.DontDestroyOnLoad(obj);
        
        obj.transform.SetPositionZ(0.006f);

        var cherry = new GameObject("Sprite")
        {
            transform =
            {
                parent = obj.transform,
                localPosition = Vector3.zero
            }
        };

        var sprites = new Sprite[amount];
        for (var i = 0; i < amount; i++)
        {
            sprites[i] = ResourceUtils.LoadSpriteResource($"{path}{i+1}", FilterMode.Point, ppu: 15);
        }
        
        cherry.AddComponent<SpriteRenderer>().sprite = sprites[0];
        if (sprites.Length > 1) cherry.AddComponent<CherryAnim>().sprites = sprites;
        else
        {
            cherry.AddComponent<FloatAnim>();
        }

        return new CustomObject(name, $"cherry_{ding}_{wing}", obj);
    }

    public class CherryAnim : MonoBehaviour
    {
        public Sprite[] sprites;
        public int spriteIndex;

        private float _time;
        private SpriteRenderer _sr;

        private void Start()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            _time += Time.deltaTime * 10;
            while (_time > 1)
            {
                _time -= 1;
                spriteIndex++;
                spriteIndex %= sprites.Length;
                _sr.sprite = sprites[spriteIndex];
            }

            transform.SetLocalPositionY((1 - Mathf.Sin(Mathf.Deg2Rad * 360 * (spriteIndex + _time - 8) / 9)) / 3);
        }
    }
}