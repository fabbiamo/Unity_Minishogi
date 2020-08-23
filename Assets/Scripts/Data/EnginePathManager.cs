using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Game;
using System.Windows.Forms;

namespace Assets.Scripts.Data
{
    public class EnginePathManager : MonoBehaviour
    {
        ToggleGroup engineToggleGroup;
        GameObject enginePrefab;

        [SerializeField]
        GameObject contents = default;

        [SerializeField]
        UnityEngine.UI.Button deleteButton = default;

        void Start()
        {
            // Prefab
            enginePrefab = Resources.Load("EnginePath") as GameObject;

            // Toggle Group
            engineToggleGroup = gameObject.AddComponent<ToggleGroup>();
            engineToggleGroup.allowSwitchOff = true;

            foreach (var enginePath in SaveData.Instance.EnginePathList)
                AddEngine(enginePath);
        }

        void Update()
        {
            deleteButton.interactable = engineToggleGroup.AnyTogglesOn();
        }

        public void OnClickAddButton()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // .exeを開くことを指定する
            openFileDialog.Filter = "Exe Files|*.exe";

            // ファイルが存在しない場合は警告を出す(true)
            openFileDialog.CheckFileExists = false;

            // ダイアログを開く
            openFileDialog.ShowDialog();

            if (string.IsNullOrEmpty(openFileDialog.FileName))
            {
                Debug.Log("null or empty.");
            }
            else if (!SaveData.Instance.AddData(openFileDialog.FileName))
            {
                Debug.Log("already exists.");
            }
            else
            {
                AddEngine(openFileDialog.FileName);
            }
        }

        public void OnClickDeleteButton()
        {
            int cnt = contents.transform.childCount;
            if (cnt != 0)
            {
                for(int index = 0; index < cnt; ++index)
                {
                    var engineTransform = contents.transform.GetChild(index);
                    if (engineTransform.GetComponent<Toggle>().isOn)
                    {
                        transform.parent.GetComponentInChildren<LocalGameSetting>().RemoveOption(index);
                        SaveData.Instance.RemoveData(index);
                        Destroy(engineTransform.gameObject);
                    }
                }
            }
        }


        public void AddEngine(string engineName)
        {
            var engine = Instantiate(enginePrefab, contents.transform);
            engine.GetComponent<Toggle>().group = engineToggleGroup;
            engine.GetComponent<EnginePath>().engineName.text = System.IO.Path.GetFileName(engineName);
            
            // update dropdown
            transform.parent.GetComponentInChildren<LocalGameSetting>().AddOption(System.IO.Path.GetFileNameWithoutExtension(engineName));
        }
    }
}
