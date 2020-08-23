using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class PlayerNameInputField : MonoBehaviour
{
    UsersData usersData;

    const string playerNamePrefKey = "PlayerName";
    void Start()
    {
        string defaultName = "UserName";
        InputField _inputField = this.GetComponent<InputField>();
        if (_inputField != null)
        {
            if (PlayerPrefs.HasKey(playerNamePrefKey))
            {
                defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                _inputField.text = defaultName;
            }
        }

        usersData.UserName = defaultName;
    }
    public void SetPlayerName(string value)
    {
#if true
        if (string.IsNullOrEmpty(value))
        {
            Debug.LogError("Player Name is null or empty");
            return;
        }
#endif
        usersData.UserName = value;

        PlayerPrefs.SetString(playerNamePrefKey, value);
    }
}