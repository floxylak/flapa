using UnityEngine;

public class Pickup : Useable
{
    public PickupSO SO;
    public AudioSource thud;
    public Rigidbody rb;
    
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Hit");
        // thud?.Play();
    }
    
    public override void Interact()
    {
        base.Interact();
        // PickUp();
    }
    
    // public virtual void PickUp()
    // {
    //     // Give pickup to player
    //     PlayerSingleton.instance.interact.EquipPickup(SO);
    //     Destroy(gameObject);
    // }
}