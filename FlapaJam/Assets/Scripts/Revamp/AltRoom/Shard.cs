using UnityEngine;

public class Shard : Pickup
{
    public AudioSource audioSource;
    public AudioClip flickerSound;
    public ParticleSystem flickerEffect;

    public override void Interact()
    {
        base.Interact();
        RoomManager.Instance.OnShardInteracted();
        audioSource.PlayOneShot(flickerSound);
        flickerEffect.Play();
    }
}