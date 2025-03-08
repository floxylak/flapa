using UnityEngine;

public class Door : Interactable
{
    private bool _draggingDoor;
    private DraggableDoor _draggableDoor;
    
    public AudioSource source;
    public AudioClip openSound;
    public AudioClip closedSound;
    
    private void Awake()
    {
        interactType = InteractType.Persistent;
        isActive = false;
        bailOutRange = 5f;
        
        _draggableDoor = GetComponent<DraggableDoor>();
        if (_draggableDoor == null) Debug.LogError("DraggableObject not found on " + gameObject.name);
    }
    
    private void Update()
    {
        if (_draggingDoor)
        {
            _draggableDoor.DragObject();
        }
    }
    
    public override void Interact()
    {
        base.Interact();
        
        _draggingDoor = !_draggingDoor;
        
        if (_draggingDoor)
        {
            _draggableDoor.StartDragging();
            if (source && openSound) source.PlayOneShot(openSound);
            Debug.Log("Door interaction: Started dragging");
        }
        else
        {
            _draggableDoor.ReleaseObject();
            if (source && closedSound) source.PlayOneShot(closedSound);
            Debug.Log("Door interaction: Released");
        }
    }
}