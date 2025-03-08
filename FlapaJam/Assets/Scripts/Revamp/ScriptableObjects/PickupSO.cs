using UnityEngine;

[CreateAssetMenu(fileName = "Pickup", menuName = "Interaction/Pickup Item")]
public class PickupSO : ScriptableObject
{
    public GameObject PickupObject;  // World object (on ground)
    public GameObject EquipModel;    // Held item model
}