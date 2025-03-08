using UnityEngine;


public class PromptSender : MonoBehaviour
{
    [TextArea]
    public string prompt;
    public float duration;
    
    public void SendPrompt()
    {
        PromptManager.instance.ShowPrompt(prompt, duration);
    }
}