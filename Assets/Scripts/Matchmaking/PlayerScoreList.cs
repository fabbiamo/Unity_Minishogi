using System;
using System.Linq;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Matchmaking {
    public class PlayerScoreList : MonoBehaviour {

        [SerializeField]
        TextMeshProUGUI ClockText = default;

        private GameObject playerScoreEntry = null;
        public GameObject PlayerScoreEntry {
            get {
                if (playerScoreEntry == null) {
                    playerScoreEntry = Resources.Load("prefab/PlayerScoreEntry") as GameObject;
                }
                return playerScoreEntry;
            }
        }

        public void LoadLeaderboard() {
            PlayFabClientAPI.GetLeaderboard(
                new GetLeaderboardRequest {
                    StatisticName = STATISTIC_NAME,
                    StartPosition = 0,
                    MaxResultsCount = 10,
                },
                result => {
                    SetCurrentTime();
                    Debug.Log($"{result.Leaderboard.Count} players");
                    foreach (var item in result.Leaderboard.Select((v, i) => new { v, i })) {
                        AddScore(item.i + 1, item.v.DisplayName, item.v.StatValue);
                    }
                },
                error => {
                    Debug.LogError(error.GenerateErrorReport());
                }
            );
        }

        private void OnEnable() {
            foreach (Transform child in transform)
                Destroy(child.gameObject);
        }

        void AddScore(int number, string name, int score) {
            var entry = Instantiate(PlayerScoreEntry, transform);
            entry.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = number.ToString();
            entry.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = name;
            entry.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = score.ToString();
        }

        void SetCurrentTime() {
            ClockText.text = DateTime.Now.ToLongTimeString();
        }

        private static readonly string STATISTIC_NAME = "RATING";
    }
}
