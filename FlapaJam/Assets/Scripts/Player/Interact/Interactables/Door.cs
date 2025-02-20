using UnityEngine;

namespace Player.Interact.Interactables
{
    public class Door : Interactable
    {
        [SerializeField]
        private GameObject door;

        private bool doorOpen;
        
        protected override void Interact()
        {
            doorOpen = !doorOpen;
            door.GetComponent<Animator>().SetBool("isOpen", doorOpen);
            Debug.Log("Interacting with " + gameObject.name);

            promptMessage = doorOpen ? "Close Door" : "Open Door";
        }
    }
}