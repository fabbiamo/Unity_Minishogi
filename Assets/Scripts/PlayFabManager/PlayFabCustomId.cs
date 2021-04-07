using System.Text;
using UnityEngine;

namespace Assets.Scripts.PlayFabManager
{
    public class PlayFabCustomId
    {
        //IDを保存する時のKEY
        private static readonly string CUSTOM_ID_SAVE_KEY = "CUSTOM_ID_SAVE_KEY";

        public static string LoadCustomId(out bool createAccount)
        {
            var id = PlayerPrefs.GetString(CUSTOM_ID_SAVE_KEY);
            createAccount = string.IsNullOrEmpty(id);
            return createAccount ? GenerateCustomId() : id;
        }

        //IDに使用する文字
        private static readonly string ID_CHARACTERS = "0123456789abcdefghijklmnopqrstuvwxyz";

        public static string GenerateCustomId()
        {
            int idLength = 32;//IDの長さ
            StringBuilder stringBuilder = new StringBuilder(idLength);
            var random = new System.Random();

            //ランダムにIDを生成
            for (int i = 0; i < idLength; i++)
                stringBuilder.Append(ID_CHARACTERS[random.Next(ID_CHARACTERS.Length)]);

            return stringBuilder.ToString();
        }

        public static void SaveCustomId(string CustomId)
        {
            PlayerPrefs.SetString(CUSTOM_ID_SAVE_KEY, CustomId);
        }
    }
}
