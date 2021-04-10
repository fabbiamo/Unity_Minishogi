using System.Linq;
using Assets.Scripts.LocalEngine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SColor = Assets.Scripts.Shogi.Color;

namespace Assets.Scripts.GameServer {
    public class LocalGamePanel : MonoBehaviour {
        [SerializeField]
        ToggleGroup SenteGroup = default;

        [SerializeField]
        ToggleGroup GoteGroup = default;

        [SerializeField]
        TMP_Dropdown SenteDropdown = default;

        [SerializeField]
        TMP_Dropdown GoteDropdown = default;

        GameServer gameServer;

        void Awake() {
            gameServer = GetComponentInParent<GameServer>();
        }

        public void OnEnable() {
            SenteDropdown.ClearOptions();
            GoteDropdown.ClearOptions();
            foreach (var path in LocalEngineData.Instance.EngineList)
                AddOption(System.IO.Path.GetFileNameWithoutExtension(path));
            Refresh();
        }

        public void OnClickStartButton() {
            var st = SenteGroup.ActiveToggles().FirstOrDefault();
            var gt = GoteGroup.ActiveToggles().FirstOrDefault();

            if (st.name == "Human" && gt.name == "Human") {
                gameObject.SetActive(false);
                gameServer.StartGame(SColor.NB);
            }
            else if (SenteDropdown.options.Count > 0) {
                gameObject.SetActive(false);
                SColor us = st.name == "Human" ? SColor.BLACK : SColor.WHITE;
                var path = LocalEngineData.Instance.EngineList[us == SColor.WHITE ? SenteDropdown.value : GoteDropdown.value];
                gameServer.StartGame(us, path);
            }
            else {
                // warning
            }
        }

        public void AddOption(string engineName) {
            SenteDropdown.options.Add(new TMP_Dropdown.OptionData { text = engineName });
            GoteDropdown.options.Add(new TMP_Dropdown.OptionData { text = engineName });
            //Refresh();
        }

        public void RemoveOption(int index) {
            SenteDropdown.options.RemoveAt(index);
            GoteDropdown.options.RemoveAt(index);
            //Refresh();
        }

        public void Refresh() {
            SenteDropdown.RefreshShownValue();
            GoteDropdown.RefreshShownValue();
            SenteGroup.GetComponentInChildren<Toggle>().interactable = SenteDropdown.options.Count > 0;
            GoteGroup.GetComponentInChildren<Toggle>().interactable = GoteDropdown.options.Count > 0;
        }
    }
}
