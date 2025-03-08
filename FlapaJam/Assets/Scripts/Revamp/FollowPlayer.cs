using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Vector3 offset = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        transform.position = Player.PlayerSingleton.instance.transform.position + offset;
    }
}