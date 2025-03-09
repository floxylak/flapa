using UnityEngine;

public class Shard : Pickup
{
    private Transform playerTransform;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
        else
            Debug.LogError("Player not found for Shard!");
    }

    public override void Interact()
    {
        base.Interact();
        if (playerTransform != null)
        {
            RoomManager.Instance.OnShardInteracted(playerTransform.position);
            Debug.Log("Shard interacted at player position: " + playerTransform.position);

            // Destroy
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("Shard cannot find player position!");
        }
    }
}