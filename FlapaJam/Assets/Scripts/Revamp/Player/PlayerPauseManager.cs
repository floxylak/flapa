using UnityEngine;

public class PlayerPauseManager : MonoBehaviour
{
    public delegate void Action();
    public event Action Pause;
    public event Action Unpause;

    private bool isPaused = false;

    public void Freeze()
    {
        if (isPaused) return;
        isPaused = true;

        // Prevents errors if no methods are subscribed
        Pause?.Invoke();

        // Disable player movement & input
        if (Player.PlayerSingleton.instance != null)
        {
            Player.PlayerSingleton.instance.movement.enabled = false;
            Player.PlayerSingleton.instance.cam.enabled = false;
            Player.PlayerSingleton.instance.input.enabled = false;
        }

        // Unlock cursor so player can interact with menus
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Unfreeze()
    {
        if (!isPaused) return;
        isPaused = false;

        // Prevents errors if no methods are subscribed
        Unpause?.Invoke();

        // Re-enable player movement & input
        if (Player.PlayerSingleton.instance != null)
        {
            Player.PlayerSingleton.instance.movement.enabled = true;
            Player.PlayerSingleton.instance.cam.enabled = true;
            Player.PlayerSingleton.instance.input.enabled = true;
        }

        // Lock cursor back when resuming gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
