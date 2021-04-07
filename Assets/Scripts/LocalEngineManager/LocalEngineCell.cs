using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.LocalEngineManager
{
    public class LocalEngineCell : MonoBehaviour
    {
        private readonly Color red = new Color(255f / 255f, 79f / 255f, 79f / 255f, 208f / 255f);
        private readonly Color white = new Color(255f / 255f, 255f / 255f, 255f / 255f, 0f / 255f);

        public void ChangeColor()
        {
            GetComponent<Image>().color = GetComponent<Toggle>().isOn ? red : white;
        }
    }
}
