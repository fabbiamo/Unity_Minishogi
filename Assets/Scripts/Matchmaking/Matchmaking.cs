using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using UnityEngine;
using UnityEngine.Events;
using Assets.Scripts.PlayerData;

namespace Assets.Scripts.Matchmaking {
    public class Matchmaking : MonoBehaviour{

        [SerializeField]
        PhotonManager PhotonManager = default;

        [SerializeField]
        PlayerDataManager PlayerDataManager = default;

        [SerializeField]
        TextController TextController = default;

        [SerializeField]
        UnityEvent OnLogin = default;

        [SerializeField]
        UnityEvent OnMatchmaking = default;

        string PlayFabId { get; set; } = null;

        public void Login() {
            var id = LoadCustomId();
            Login(
                string.IsNullOrEmpty(id) ? CreateNewId() : id,
                string.IsNullOrEmpty(id)
            );
        }

        public void OnClickConnectButton() {
            OnMatchmaking.Invoke();
            CreateTicket();
        }

        public void OnClickLocalButton() {
            UnityEngine.SceneManagement.SceneManager.LoadScene("PlayScene");
        }

        public void OnEditInputField() {
            if (TextController.IsChanged())
                SetPlayerDisplayName(TextController.DisplayName);
        }

        void Awake() {
            Login();
        }

        void Login(string id, bool shouldCreateAccount) {
            PlayFabClientAPI.LoginWithCustomID(
                new LoginWithCustomIDRequest {
                    CustomId = id,
                    CreateAccount = shouldCreateAccount
                },
                result => {
                    Debug.Log($"Login successfully, NewlyCreated = {result.NewlyCreated}");
                    PlayFabId = result.PlayFabId;

                    if (result.NewlyCreated) {
                        SaveCustomId(id);
                        SetPlayerDisplayName("anonymous", true);
                        UpdateUserData(new PlayerData.PlayerData(), true);
                    }
                    else {
                        // DisplayNameの取得
                        GetDisplayName();

                        // UserDataの取得
                        GetUserData();
                    }

                    OnLogin.Invoke();
                },
                error => {
                    Debug.LogError(error.GenerateErrorReport());
                }
            );
        }

        string LoadCustomId() {
            return PlayerPrefs.GetString(KEY);
        }

        void SaveCustomId(string value) {
            PlayerPrefs.SetString(KEY, value);

        }

        void SetPlayerDisplayName(string displayName, bool reget = false) {
            PlayFabClientAPI.UpdateUserTitleDisplayName(
                new UpdateUserTitleDisplayNameRequest {
                    DisplayName = displayName
                },
                result => {
                    Debug.Log("Set display name was succeded");

                    if (reget)
                        GetDisplayName();
                },
                error => {
                    Debug.LogError(error.GenerateErrorReport());
                }
            );
        }

        void GetDisplayName() {
            PlayFabClientAPI.GetPlayerProfile(
                new GetPlayerProfileRequest {
                    PlayFabId = PlayFabId,
                    ProfileConstraints = new PlayerProfileViewConstraints {
                        ShowDisplayName = true
                    }
                },
                result => {
                    if (string.IsNullOrEmpty(result.PlayerProfile.DisplayName)) {
                        SetPlayerDisplayName("anonymous", true);
                        return;
                    }
                    TextController.SetDisplayName(result.PlayerProfile.DisplayName);
                    Debug.Log($"DisplayName : {result.PlayerProfile.DisplayName}");
                },
                error => {
                    Debug.LogError(error.GenerateErrorReport());
                }
            );
        }

        void UpdateUserData(PlayerData.PlayerData playerData, bool reget = false) {
            PlayFabClientAPI.UpdateUserData(
                new UpdateUserDataRequest {
                    Data = new Dictionary<string, string> {
                        { "Rating" , playerData.Rating.ToString() },
                        { "Game"   , playerData.Game.ToString()   },
                        { "Win"    , playerData.Win.ToString()    },
                        { "Lose"   , playerData.Lose.ToString()   },
                    }
                },
                result => {
                    Debug.Log("Update successfully");

                    // 統計情報を送信する
                    SendStatisticUpdate((int)playerData.Rating);

                    if (reget)
                        GetUserData();
                },
                error => {
                    Debug.LogError(error.GenerateErrorReport());
                }
            );
        }

        void GetUserData() {
            PlayFabClientAPI.GetUserData(
                new GetUserDataRequest {
                    PlayFabId = PlayFabId,
                },
                result => {
                    if (!result.Data.ContainsKey("Rating") ||
                        !result.Data.ContainsKey("Game") ||
                        !result.Data.ContainsKey("Win") ||
                        !result.Data.ContainsKey("Lose")) {
                        // データ損失
                        UpdateUserData(new PlayerData.PlayerData(), true);
                        return;
                    }

                    var playerData = new PlayerData.PlayerData {
                        Rating = double.Parse(result.Data["Rating"].Value),
                        Game = int.Parse(result.Data["Game"].Value),
                        Win = int.Parse(result.Data["Win"].Value),
                        Lose = int.Parse(result.Data["Lose"].Value),
                    };

                    // データをセットする
                    PlayerDataManager.SetPlayerData(playerData);
                    TextController.DisplayPlayerData(playerData.Rating, playerData.Game, playerData.Win, playerData.Lose);
                    Debug.Log("Get user data successfully");
                    UpdateUserData(PlayerDataManager.PlayerData);
                },
                error => {
                    Debug.LogError(error.GenerateErrorReport());
                }
            );
        }

        void SendStatisticUpdate(int value) {

            var statisticUpdates = new List<StatisticUpdate> {
                new StatisticUpdate {
                    StatisticName = STATISTIC_NAME,
                    Value = value,
                }
            };

            PlayFabClientAPI.UpdatePlayerStatistics(
                new UpdatePlayerStatisticsRequest {
                    Statistics = statisticUpdates,
                },
                result => {
                    Debug.Log("Send score was succeeded");
                },
                error => {
                    Debug.LogError(error.GenerateErrorReport());
                }
            );
        }

        string CreateNewId() {
            return System.Guid.NewGuid().ToString();
        }

        void CreateTicket() {
            var matchingplayer = new MatchmakingPlayer {
                Entity = new PlayFab.MultiplayerModels.EntityKey {
                    Id = PlayFabSettings.staticPlayer.EntityId,
                    Type = PlayFabSettings.staticPlayer.EntityType,
                },
                Attributes = new MatchmakingPlayerAttributes {
                    DataObject = new { },
                }
            };

            PlayFabMultiplayerAPI.CreateMatchmakingTicket(
                new CreateMatchmakingTicketRequest {
                    Creator = matchingplayer,
                    GiveUpAfterSeconds = 12, // 最大600秒
                    QueueName = QUEUE_NAME,
                },
                result => {
                    StartCoroutine(Polling(result.TicketId));
                },
                error => {
                    Debug.LogError(error.GenerateErrorReport());
                }
            );
        }

        IEnumerator Polling(string ticketId) {

            var seconds = 6f; // 6秒間隔
            bool matchedOrCanceled = false;

            while (true) {
                if (matchedOrCanceled) {
                    //OnLogin.Invoke();
                    yield break;
                }

                PlayFabMultiplayerAPI.GetMatchmakingTicket(
                    new GetMatchmakingTicketRequest {
                        TicketId = ticketId,
                        QueueName = QUEUE_NAME
                    },
                    OnGetMatchingTicket,
                    error => {
                        matchedOrCanceled = true;
                        Debug.LogError(error.GenerateErrorReport());
                    }
                );
                yield return new WaitForSeconds(seconds);
            }

            void OnGetMatchingTicket(GetMatchmakingTicketResult result) {
                switch (result.Status) {
                case "Matched":
                    matchedOrCanceled = true;
                    TextController.Output("対戦相手が見つかりました！");
                    GetMatch(result.MatchId);
                    return;

                case "Canceled":
                    TextController.Output("キャンセルしました");
                    OnLogin.Invoke();
                    matchedOrCanceled = true;
                    return;

                default:
                    TextController.Output("対戦相手を探しています");
                    return;
                }
            }
        }

        void GetMatch(string matchId) {
            PlayFabMultiplayerAPI.GetMatch(
                new GetMatchRequest {
                    MatchId = matchId,
                    ReturnMemberAttributes = true,
                    QueueName = QUEUE_NAME,
                },
                result => {
                    PhotonManager.SetProperties(TextController.DisplayName, PlayerDataManager.PlayerData.Rating);
                    PhotonManager.JoinOrCreateRoom(result.MatchId);
                },
                error => {
                    Debug.LogError(error.GenerateErrorReport());
                }
            );
        }

#if false
            // 統計情報の取得
            PlayFabMultiplayerAPI.GetQueueStatistics(
                new GetQueueStatisticsRequest {
                    QueueName = QUEUE_NAME,
                },
                result => {
                    Debug.Log(result.NumberOfPlayersMatching);
                },
                error => {
                    Debug.LogError(error.GenerateErrorReport());
                }
            );
#endif

        private static readonly string KEY = "CUSTOM_ID";
        private static readonly string STATISTIC_NAME = "RATING";
        private static readonly string QUEUE_NAME = "1vs1";
    }
}
