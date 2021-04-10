using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.LocalEngine {
    public class LocalEngineManager : MonoBehaviour {
        private GameObject localengineEntry = null;
        public GameObject LocalEngineEntry {
            get {
                if (localengineEntry == null)
                    localengineEntry = Resources.Load("prefab/LocalEngineEntry") as GameObject;
                return localengineEntry;
            }
        }

        void Start() {
            foreach (var path in LocalEngineData.Instance.EngineList)
                AddEngine(path);
        }


        public void OnClickAddButton() {
            var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", "exe", false);
            if (paths.Length > 0) {
                var path = paths[0];

                if (string.IsNullOrEmpty(path))
                    Debug.Log("null or empty.");
                else if (!LocalEngineData.Instance.AddData(path))
                    Debug.Log("already exists.");
                else
                    AddEngine(path);
            }
        }

        public void OnClickDeleteButton() {
            for (int index = 0; index < transform.childCount; ++index) {
                var child = transform.GetChild(index);
                if (child.GetComponent<Toggle>().isOn) {
                    LocalEngineData.Instance.RemoveData(index);
                    //panel.RemoveOption(index);
                    Destroy(child.gameObject);
                }
            }
        }

        public void AddEngine(string path) {
            var entry = Instantiate(LocalEngineEntry, transform);
            entry.GetComponent<Toggle>().group = gameObject.GetComponent<ToggleGroup>();
            entry.GetComponentInChildren<TextMeshProUGUI>().text = System.IO.Path.GetFileName(path);
            //panel.AddOption(System.IO.Path.GetFileNameWithoutExtension(path));
        }
    }
}
