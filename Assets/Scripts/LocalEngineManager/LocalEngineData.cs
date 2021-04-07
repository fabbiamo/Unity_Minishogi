using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Assets.Scripts.LocalEngineManager
{
    [Serializable]
    public class LocalEngineData : ISerializationCallbackReceiver
    {
        private static LocalEngineData instance = null;
        public static LocalEngineData Instance
        {
            get
            {
                if (instance == null)
                    Load();
                return instance;
            }
        }

        [SerializeField]
        private static string jsonText_ = "";
        
        public List<string> engineList = new List<string>() {};

        public void OnAfterDeserialize()
        {
            //Debug.Log("OnAfterDeserialize");
        }

        public void OnBeforeSerialize()
        {
            //Debug.Log("OnBeforeSerialize");
        }

        private static string Serialize<T>(T obj)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();
            binaryFormatter.Serialize(memoryStream, obj);
            return Convert.ToBase64String(memoryStream.GetBuffer());
        }

        private static T Deserialize<T>(string str)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(str));
            return (T)binaryFormatter.Deserialize(memoryStream);
        }

        public void Reload()
        {
            JsonUtility.FromJsonOverwrite(GetJson(), this);
        }

        private static void Load()
        {
            instance = JsonUtility.FromJson<LocalEngineData>(GetJson());
        }

        private static string GetJson()
        {
            if (!string.IsNullOrEmpty(jsonText_))
                return jsonText_;

            string filePath = GetSaveFilePath();

             jsonText_ = File.Exists(filePath)
                ? File.ReadAllText(filePath)
                : JsonUtility.ToJson(new LocalEngineData());

            return jsonText_;
        }

        public bool AddData(string enginePath)
        {
            foreach (var path in engineList)
            {
                if (string.Equals(path, enginePath, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            instance.engineList.Add(enginePath);
            Save();
            return true;
        }

        public void RemoveData(int index)
        {
            instance.engineList.RemoveAt(index);
            Save();
        }

        public void Save()
        {
            jsonText_ = JsonUtility.ToJson(this);
            File.WriteAllText(GetSaveFilePath(), jsonText_);
        }

        public void Delete()
        {
            jsonText_ = JsonUtility.ToJson(new LocalEngineData());
            Reload();
        }

        private static string GetSaveFilePath()
        {
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
