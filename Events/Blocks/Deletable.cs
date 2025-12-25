using Architect.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Architect.Events.Blocks;

public abstract class Deletable : MonoBehaviour, IPointerClickHandler
{
    public static GameObject DeleteButton;
    
    private static Deletable _target;
    
    public static void Init(GameObject scriptUI)
    {
        var (btn, img, _) = UIUtils.MakeButtonWithImage(
            "Delete Button",
            scriptUI,
            Vector2.zero,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            96,
            48,
            false);
        DeleteButton = btn.gameObject;
        img.sprite = ResourceUtils.LoadSpriteResource("delete_button");
        
        btn.onClick.AddListener(() =>
        {
            if (_target)
            {
                _target.Delete();
                _target = null;
                DeleteButton.SetActive(false);
            }
        });
    }
    
    public abstract void Delete();
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right)
        {
            DeleteButton.SetActive(false);
            return;
        }
        _target = this;
        DeleteButton.SetActive(true);
        DeleteButton.transform.SetAsLastSibling();
        DeleteButton.transform.position = eventData.position + new Vector2(-20, 20);
    }
}