using UnityEngine;

public class Shard : Pickup
{
    private Transform playerTransform; // Reference to player's transform

    private void Start()
    {
        // Assuming the player has a tag "Player", adjust as needed
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
        }
        else
        {
            Debug.LogWarning("Shard cannot find player position!");
        }
    }
}