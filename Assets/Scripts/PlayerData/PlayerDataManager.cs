using System;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.PlayerData
{
    public class PlayerDataManager : MonoBehaviour
    {
        [SerializeField] TMP_InputField nameInputField = default;

        [SerializeField] TextMeshProUGUI ratingText = default;

        [SerializeField] TextMeshProUGUI winText = default;

        [SerializeField] TextMeshProUGUI loseText = default;

        public PlayerData playerData { get; private set; }

        public void GetData()
        {
            if (playerData == null)
                playerData = new PlayerData();

            var request = new GetUserDataRequest();
            PlayFabClientAPI.GetUserData(request, OnSuccess, OnError);

            void OnSuccess(GetUserDataResult result)
            {
                playerData.SetData(result.Data);
                UpdateField();
            }

            void OnError(PlayFabError error)
            {
                Debug.LogError("failed");
                Debug.LogError(error.GenerateErrorReport());
            }
        }

        public void CreateData()
        {
            var request = new UpdateUserDataRequest()
            {
                Data = new Dictionary<string, string>
                {
                    {"name", "anonymous" },
                    {"rating", "1500"},
                    {"game", "0"},
                    {"win", "0"},
                    {"lose", "0"},
                }
            };

            PlayFabClientAPI.UpdateUserData(request, OnSuccess, OnError);

            void OnSuccess(UpdateUserDataResult result)
            {
                Debug.Log("create success");
                GetData();
            }

            void OnError(PlayFabError error)
            {
                Debug.Log(error.GenerateErrorReport());
            }
        }

        public void UpdateField()
        {
            nameInputField.text = playerData.Name;
            ratingText.text = Math.Round(playerData.Rating, MidpointRounding.AwayFromZero).ToString();
            winText.text = playerData.Win.ToString();
            loseText.text = playerData.Lose.ToString();
        }

        public void UpdateData()
        {
            var request = new UpdateUserDataRequest()
            {
                Data = new Dictionary<string, string>
                {
                    {"name"  , playerData.Name},
                    {"rating", playerData.Rating.ToString()},
                    {"game"  , playerData.Game.ToString()},
                    {"win"   , playerData.Win.ToString()},
                    {"lose"  , playerData.Lose.ToString()},
                }
            };

            PlayFabClientAPI.UpdateUserData(request, OnSuccess, OnError);

            void OnSuccess(UpdateUserDataResult result)
            {
                Debug.Log("update success!");
            }

            void OnError(PlayFabError error)
            {
                Debug.LogError(error.GenerateErrorReport());
            }
        }

        public void UpdateData(double diff, bool isWin)
        {
            playerData.UpdateData(diff, isWin);
            //Debug.LogFormat($"{oldRating} -> {Rating}");
            UpdateData();
        }

        public void OnClickExitButton()
        {
            if (nameInputField.text != playerData.Name)
            {
                if (!string.IsNullOrWhiteSpace(nameInputField.text))
                {
                    playerData.SetName(nameInputField.text);
                    UpdateData();
                }
            }
            gameObject.SetActive(false);
        }
    }
}
