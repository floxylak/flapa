using 
    
    UnityEngine;

using TMPro;

namespace Player
{
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI promptText;

        public void UpdatePromptText(string promptMessage)
        {
            promptText.text = promptMessage;
        }
    }
}
