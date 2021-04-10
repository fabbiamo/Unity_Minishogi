using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.LocalEngine {
    public class LocalEngineEntry : MonoBehaviour {

        public void ChangeColor() {
            GetComponent<Image>().color = GetComponent<Toggle>().isOn ? Red : White;
        }

        private readonly Color Red = new Color(255f / 255f, 79f / 255f, 79f / 255f, 208f / 255f);
        private readonly Color White = new Color(255f / 255f, 255f / 255f, 255f / 255f, 0f / 255f);
    }
}
