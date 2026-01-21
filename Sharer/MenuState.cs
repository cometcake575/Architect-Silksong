using UnityEngine;

namespace Architect.Sharer;

public abstract class MenuState : MonoBehaviour
{
    public virtual MenuState ReturnState => null;

    public bool started;
    
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
        
        if (!started)
        {
            started = true;
            OnStart();
        }
        OnOpen();
    }
    
    public virtual void OnOpen() { }
    public virtual void OnClose() { }
    public virtual void OnStart() { }
}