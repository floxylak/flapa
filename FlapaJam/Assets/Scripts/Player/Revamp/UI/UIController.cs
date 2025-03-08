using TMPro;
using UnityEngine;

namespace Player
{
    public class UIController : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI promptText;

        public void UpdatePromptText(string promptMessage)
        {
            promptText.text = promptMessage;
        }
    }
}