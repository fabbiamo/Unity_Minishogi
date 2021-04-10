using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Assets.Scripts.LocalEngine {
    [Serializable]
    public class LocalEngineData : ISerializationCallbackReceiver {
        static LocalEngineData instance = null;
        public static LocalEngineData Instance {
            get {
                if (instance == null)
                    Load();
                return instance;
            }
        }

        static string JsonText = "";

        public List<string> EngineList = new List<string>() { };

        public void OnAfterDeserialize() {
            //Debug.Log("OnAfterDeserialize");
        }

        public void OnBeforeSerialize() {
            //Debug.Log("OnBeforeSerialize");
        }

        private static string Serialize<T>(T obj) {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();
            binaryFormatter.Serialize(memoryStream, obj);
            return Convert.ToBase64String(memoryStream.GetBuffer());
        }

        private static T Deserialize<T>(string str) {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(str));
            return (T)binaryFormatter.Deserialize(memoryStream);
        }

        public void Reload() {
            JsonUtility.FromJsonOverwrite(GetJson(), this);
        }

        private static void Load() {
            instance = JsonUtility.FromJson<LocalEngineData>(GetJson());
        }

        private static string GetJson() {
            if (!string.IsNullOrEmpty(JsonText))
                return JsonText;

            string filePath = GetSaveFilePath();

            JsonText = File.Exists(filePath)
               ? File.ReadAllText(filePath)
               : JsonUtility.ToJson(new LocalEngineData());

            return JsonText;
        }

        public bool AddData(string enginePath) {
            foreach (var path in EngineList) {
                if (string.Equals(path, enginePath, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            instance.EngineList.Add(enginePath);
            Save();
            return true;
        }

        public void RemoveData(int index) {
            instance.EngineList.RemoveAt(index);
            Save();
        }

        public void Save() {
            JsonText = JsonUtility.ToJson(this);
            File.WriteAllText(GetSaveFilePath(), JsonText);
        }

        public void Delete() {
            JsonText = JsonUtility.ToJson(new LocalEngineData());
            Reload();
        }

        private static string GetSaveFilePath() {
            string filePath = "SaveData";

#if UNITY_EDITOR
            filePath += ".json";
#else
            filePath = Application.persistentDataPath + "/" + filePath;
#endif

            return filePath;
        }
    }
}
