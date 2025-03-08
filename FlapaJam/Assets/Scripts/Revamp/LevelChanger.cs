/*using UnityEngine.SceneManagement;
using UnityEngine;
using Player;

public class LevelChanger : Interactable
{
    public Object sceneToChangeTo;
    public Transform positionToTeleportTo;
    public override void Interact()
    {
        Debug.Log("Test");
        SceneManager.LoadScene(sceneToChangeTo.name);
        if(positionToTeleportTo != null)
        {
            PlayerSingleton.instance.transform.position = positionToTeleportTo.position;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public override void Update()
    {
        
    }
}*/