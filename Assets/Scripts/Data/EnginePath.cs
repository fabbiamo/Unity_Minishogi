using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets.Scripts.Data
{
    public class EnginePath : MonoBehaviour
    {
        public TextMeshProUGUI engineName;

        Color red = new Color(255f / 255f, 79f / 255f, 79f / 255f, 208f / 255f);
        Color white = new Color(255f / 255f, 255f / 255f, 255f / 255f, 0f / 255f);

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        public void ChangeColor()
        {
            GetComponent<Image>().color = GetComponent<Toggle>().isOn ? red : white;
        }
    }
}
