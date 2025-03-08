using UnityEngine.Events;

public class Useable : Interactable
{
    public UnityEvent useEvent;
    public int uses = 1;
    
    public override void Interact()
    {
        base.Interact();
        Use();
    }
    
    public virtual void Use()
    {
        useEvent?.Invoke();
        if (uses < 0) return;

        uses -= 1;
        if (uses == 0)
            Destroy(gameObject);
        
    }
}