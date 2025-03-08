using UnityEngine;
using UnityEngine.Events;

public class Flashlight : Pickup
{
    private Light flashlightLight;
    private bool isOn = false;
    
    [SerializeField] private bool startOn = false;
    
    [Header("Flashlight Events")]
    public UnityEvent OnToggle; 

    private void Awake()
    {
        flashlightLight = GetComponentInChildren<Light>();
        if (flashlightLight == null)
        {
            Debug.LogError($"{gameObject.name} is missing a Light component in its children!");
            return;
        }
        
        flashlightLight.enabled = startOn;
        isOn = startOn;
        
        uses = -1;
        
        if (SO == null)
            Debug.LogWarning($"{gameObject.name} is missing a PickupSO reference!");
        
    }

    public override void Use()
    {
        base.Use();
        if (flashlightLight == null)
        {
            Debug.LogError($"Flashlight.Use() failed: flashlightLight is null on {gameObject.name}");
            return;
        }
        isOn = !isOn;
        flashlightLight.enabled = isOn;
        OnToggle?.Invoke();
        Debug.Log($"Flashlight toggled to {isOn} on {gameObject.name}");
    }
    
    public override void Interact()
    {
        base.Interact(); // Handles Interactable.Interact() logic
        // If you want the flashlight to toggle when interacted with on the ground, uncomment below
        // Use();
    }
}