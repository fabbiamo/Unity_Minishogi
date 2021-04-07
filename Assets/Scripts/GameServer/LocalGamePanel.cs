using System.Linq;
using Assets.Scripts.LocalEngineManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SColor = Assets.Scripts.Shogi.Color;

namespace Assets.Scripts.GameServer
{
    public class LocalGamePanel : MonoBehaviour
    {
        [SerializeField]
        ToggleGroup senteGroup = default;

        [SerializeField]
        ToggleGroup goteGroup = default;

        [SerializeField]
        TMP_Dropdown senteDropdown = default;

        [SerializeField]
        TMP_Dropdown goteDropdown = default;

        GameServer gameServer;

        void Awake()
        {
            gameServer = GetComponentInParent<GameServer>();
        }

        public void OnEnable()
        {
            senteDropdown.ClearOptions();
            goteDropdown.ClearOptions();
            foreach (var path in LocalEngineData.Instance.engineList)
                AddOption(System.IO.Path.GetFileNameWithoutExtension(path));
            Refresh();
        }

        public void OnClickStartButton()
        {
            var st = senteGroup.ActiveToggles().FirstOrDefault();
            var gt = goteGroup.ActiveToggles().FirstOrDefault();

            if (st.name == "Human" && gt.name == "Human")
            {
                gameObject.SetActive(false);
                gameServer.StartGame(SColor.NB);
            }
            else if (senteDropdown.options.Count > 0)
            {
                gameObject.SetActive(false);
                SColor us = st.name == "Human" ? SColor.BLACK : SColor.WHITE;
                var path = LocalEngineData.Instance.engineList[us == SColor.WHITE ? senteDropdown.value : goteDropdown.value];
                gameServer.StartGame(us, path);
            }
            else
            {
                // warning
            }
        }

        public void AddOption(string engineName)
        {
            senteDropdown.options.Add(new TMP_Dropdown.OptionData { text = engineName });
            goteDropdown.options.Add(new TMP_Dropdown.OptionData { text = engineName });
            //Refresh();
        }

        public void RemoveOption(int index)
        {
            senteDropdown.options.RemoveAt(index);
            goteDropdown.options.RemoveAt(index);
            //Refresh();
        }

        public void Refresh()
        {
            senteDropdown.RefreshShownValue();
            goteDropdown.RefreshShownValue();
            senteGroup.GetComponentInChildren<Toggle>().interactable = senteDropdown.options.Count > 0;
            goteGroup.GetComponentInChildren<Toggle>().interactable = goteDropdown.options.Count > 0;
        }
    }
}
