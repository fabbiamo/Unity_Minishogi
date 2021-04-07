using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.LocalEngineManager
{
    public class LocalEngineManager : MonoBehaviour
    {
        private static GameObject engineCell = null;
        public static GameObject EngineCell
        {
            get
            {
                if (engineCell == null)
                    engineCell = Resources.Load("LocalEngineCell") as GameObject;
                return engineCell;
            }
        }

        void Start()
        {
            foreach (var path in LocalEngineData.Instance.engineList)
                AddEngine(path);
        }


        public void OnClickAddButton()
        {
            var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", "exe", false);
            if (paths.Length > 0)
            {
                var path = paths[0];

                if (string.IsNullOrEmpty(path))
                    Debug.Log("null or empty.");
                else if (!LocalEngineData.Instance.AddData(path))
                    Debug.Log("already exists.");
                else
                    AddEngine(path);
            }
        }

        public void OnClickDeleteButton()
        {
            for (int index = 0; index < transform.childCount; ++index)
            {
                var child = transform.GetChild(index);
                if (child.GetComponent<Toggle>().isOn)
                {
                    LocalEngineData.Instance.RemoveData(index);
                    //panel.RemoveOption(index);
                    Destroy(child.gameObject);
                }
            }
        }

        public void AddEngine(string path)
        {
            var cell = Instantiate(EngineCell, transform);
            cell.GetComponent<Toggle>().group = gameObject.GetComponent<ToggleGroup>();
            cell.GetComponentInChildren<TextMeshProUGUI>().text = System.IO.Path.GetFileName(path);
            //panel.AddOption(System.IO.Path.GetFileNameWithoutExtension(path));
        }
    }
}
