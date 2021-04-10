using UnityEngine;
using TMPro;

namespace Assets.Scripts.Matchmaking {
    class TextController : MonoBehaviour {
        [SerializeField]
        TMP_InputField InputField = default;

        [SerializeField]
        TextMeshProUGUI MessageText = default;

        [SerializeField]
        TextMeshProUGUI RatingText = default;

        [SerializeField]
        TextMeshProUGUI WinText = default;

        [SerializeField]
        TextMeshProUGUI LoseText = default;

        string DisplayNameBefore { get; set; }

        public string displayName;

        public string DisplayName {
            get { return displayName; }
            private set {
                DisplayNameBefore = displayName;
                displayName = value;
            }
        }

        public void SetDisplayName(string name) {
            DisplayName = name;
            InputField.text = name;
        }

        public bool IsChanged() {
            DisplayName = InputField.text;
            return DisplayNameBefore != DisplayName;
        }

        public void Output(string message) {
            MessageText.text = message;
        }

        public void DisplayPlayerData(double rating, int game, int win, int lose) {
            RatingText.text = ((int)rating).ToString();
            WinText.text = win.ToString();
            LoseText.text = lose.ToString();
        }
    }
}
