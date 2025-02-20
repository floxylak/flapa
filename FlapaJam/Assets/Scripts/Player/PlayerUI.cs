using UnityEngine;

using TMPro;

namespace Player
{
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _promptText;

        public void UpdateText(string promptMessage)
        {
            _promptText.text = promptMessage;
        }
    }
}
