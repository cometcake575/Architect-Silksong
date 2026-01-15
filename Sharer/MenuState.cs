using UnityEngine;

namespace Architect.Sharer;

public abstract class MenuState : MonoBehaviour
{
    public virtual MenuState ReturnState => null;
    
    private void Start()
    {
        OnStart();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        OnClose();
    }

    public void Open()
    {
        gameObject.SetActive(true);
        
        SharerManager.ReturnBtn.SetActive(ReturnState);
        SharerManager.OpenSharerBtn.SetActive(!ReturnState);
        
        OnOpen();
    }
    
    public virtual void OnOpen() { }
    public virtual void OnClose() { }
    public virtual void OnStart() { }
}