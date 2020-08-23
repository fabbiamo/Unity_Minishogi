using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets.Scripts.Game
{
    public class LocalGameSetting : MonoBehaviour
    {
        [SerializeField]
        ToggleGroup blackGroup = default, whiteGroup = default;

        [SerializeField]
        TMP_Dropdown dropdownBlack = default, dropdownWhite = default;

        public void OnClickStartButton()
        {
            //Get the label in activated toggles
            string blackLabel = blackGroup.ActiveToggles()
                .First().GetComponentsInChildren<Text>()
                .First(t => t.name == "Label").text;

            string whiteLabel = whiteGroup.ActiveToggles()
                .First().GetComponentsInChildren<Text>()
                .First(t => t.name == "Label").text;
            GameFormat gameFormat =
                blackLabel == "Human" && whiteLabel == "Human" ? GameFormat.LocalHumanHuman :
                blackLabel == "Human" && whiteLabel == "Cpu"   ? GameFormat.LocalHumanCpu :
                blackLabel == "Cpu"   && whiteLabel == "Human" ? GameFormat.LocalCpuHuman : GameFormat.None;

            GetComponentInParent<LocalGameServer>().NewGame(gameFormat, dropdownBlack.value, dropdownWhite.value);
        }

        public void AddOption(string engineName)
        {
            // Dropdown
            dropdownBlack.options.Add(new TMP_Dropdown.OptionData { text = engineName });
            dropdownWhite.options.Add(new TMP_Dropdown.OptionData { text = engineName });

            // Toggle Group
            blackGroup.transform.GetChild(1).GetComponent<Toggle>().interactable = true;
            whiteGroup.transform.GetChild(1).GetComponent<Toggle>().interactable = true;
        }

        public void RemoveOption(int index)
        {
            dropdownBlack.options.RemoveAt(index);
            dropdownWhite.options.RemoveAt(index);

            if (dropdownBlack.options.Count <= 0)
            {
                // Dropdown
                dropdownBlack.GetComponentInChildren<TextMeshProUGUI>().text = "";
                dropdownWhite.GetComponentInChildren<TextMeshProUGUI>().text = "";

                // Toggle Group
                blackGroup.transform.GetChild(0).GetComponent<Toggle>().isOn = true;
                whiteGroup.transform.GetChild(0).GetComponent<Toggle>().isOn = true;
                blackGroup.transform.GetChild(1).GetComponent<Toggle>().interactable = false;
                whiteGroup.transform.GetChild(1).GetComponent<Toggle>().interactable = false;
            }
            else
            {
                // Dropdown
                dropdownBlack.GetComponentInChildren<TextMeshProUGUI>().text = dropdownBlack.options[0].text;
                dropdownWhite.GetComponentInChildren<TextMeshProUGUI>().text = dropdownBlack.options[0].text;
            }
        }
    }
}
