using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameMenuUI : MonoBehaviour
{
    private Button newGameBtn;
    private Button optionsBtn;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        newGameBtn = root.Q<Button>("new-game-btn");
        optionsBtn = root.Q<Button>("options-btn");

        newGameBtn.clicked += StartGame;
        optionsBtn.clicked += OpenOptions;
    }

    void StartGame()
    {
        SceneManager.LoadScene("bunkerscene"); // Load the game scene
    }

    void OpenOptions()
    {
        Debug.Log("Options Menu Placeholder");
    }
}
