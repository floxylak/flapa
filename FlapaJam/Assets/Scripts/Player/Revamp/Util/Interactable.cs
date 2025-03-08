using UnityEngine;

namespace Core
{
    public abstract class Interactable : MonoBehaviour
    {

        public string promptAction;
        public bool isHold;
        
        public void BaseInteract()
        {
            Interact();
        }

        protected virtual void Interact()
        {
            
        }
    }
}