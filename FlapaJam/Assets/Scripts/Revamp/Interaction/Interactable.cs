using Player;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public enum InteractType
    {
        Click,
        Hold,
        Persistent
    }

    [Header("Interaction Settings")]
    public InteractType interactType = InteractType.Click;
    public float bailOutRange = 5f;
    public string interactText = "Interact";
    
    [Header("Item Requirement")]
    public bool requiresItem = false;
    public PickupSO itemRequirement;
    
    [Header("Events")]
    public UnityEvent interactEvent;
    
    [SerializeField] protected bool isActive = false;

    private bool touching;
    // Getters
    private static PlayerInteraction1 PlayerInteraction => PlayerSingleton.instance?.interact;
    
    public virtual void Interact()
    {
        if (requiresItem && PlayerSingleton.instance.interact.PickupInHand != itemRequirement) return;
        interactEvent?.Invoke();
        
        switch (interactType)
        {
            case InteractType.Click:
                isActive = !isActive;
                Debug.Log($"{gameObject.name} clicked, active: {isActive}");
                break;
            case InteractType.Hold:
                Debug.Log($"{gameObject.name} being held");
                break;
            case InteractType.Persistent:
                // Toggle persistent state (e.g., start/stop dragging)
                isActive = !isActive;
                Debug.Log($"{gameObject.name} persistent interaction, active: {isActive}");
                break;
        }
    }

    public virtual void LookingAt()
    {
        touching = true;
    }

    public virtual void NotLookingAt()
    {
        touching = false;
    }

    protected bool PlayerHasRequiredItem()
    {
        if (PlayerInteraction == null) return false;
        return PlayerInteraction.PickupInHand == itemRequirement;
    }
}