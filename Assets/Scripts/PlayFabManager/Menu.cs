using System.Collections;
using Assets.Scripts.PlayerData;
using PlayFab;
using PlayFab.MultiplayerModels;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.PlayFabManager
{
    public class Menu : MonoBehaviour
    {
        [SerializeField]
        TextMeshProUGUI outputText = default;
        
        [SerializeField]
        Button connectButton = default;

        [SerializeField]
        Button disconnectButton = default;

        [SerializeField]
        Button localButton = default;

        [SerializeField]
        Button accountButton = default;

        PhotonManager photonManager;
        PlayerDataManager playerDataManager;
        string ticketId;
        string queueName = "1vs1";
        bool matchedOrCanceled = false;

        void Awake()
        {
            photonManager = GetComponentInChildren<PhotonManager>();
            playerDataManager = GameObject.FindWithTag("DataManager").GetComponent<PlayerDataManager>();
            playerDataManager.gameObject.SetActive(false);
        }

        public void GetData()
        {
            playerDataManager.GetData();
        }

        public void CreateData()
        {
            playerDataManager.CreateData();
        }

        public void Interactable(bool isConnect)
        {
            connectButton.interactable = !isConnect;
            localButton.interactable = !isConnect;
            disconnectButton.interactable = isConnect;
            accountButton.interactable = !isConnect;
        }

        public void OnClickConnectButton()
        {
            Interactable(true);

            var matchmakingPlayer = new MatchmakingPlayer
            {
                // Entityは下記のコードで決め打ちで大丈夫です。
                Entity = new PlayFab.MultiplayerModels.EntityKey
                {
                    Id = PlayFabSettings.staticPlayer.EntityId,
                    Type = PlayFabSettings.staticPlayer.EntityType
                },

                Attributes = new MatchmakingPlayerAttributes
                {
                    DataObject = new { /*Rating = usersData.Rating*/ }
                }
            };

            var request = new CreateMatchmakingTicketRequest
            {
                // 先程作っておいたプレイヤー情報です。
                Creator = matchmakingPlayer,
                // マッチングできるまで待機する秒数を指定します。最大600秒です。
                GiveUpAfterSeconds = 12,
                // GameManagerで作ったキューの名前を指定します。
                QueueName = queueName,
            };

            PlayFabMultiplayerAPI.CreateMatchmakingTicket(request, OnMatchmakingTicketCreated, OnMatchmakingError);

            void OnMatchmakingTicketCreated(CreateMatchmakingTicketResult result)
            {
                // キューに積んだチケットの状態をマッチングするかタイムアウトするまでポーリングします。
                var getMatchmakingTicketRequest = new GetMatchmakingTicketRequest
                {
                    TicketId = ticketId = result.TicketId,
                    QueueName = request.QueueName,
                };

                StartCoroutine(Polling(getMatchmakingTicketRequest));
            }
        }

        public void OnClickDisconnectButton()
        {
            Interactable(false);
            outputText.text = "";

#if false
            PlayFabMultiplayerAPI.CancelMatchmakingTicket(
                new CancelMatchmakingTicketRequest
                {
                    TicketId = ticketId,
                    QueueName = queueName,
                }, OnTicketCanceled, OnFailure);

            void OnTicketCanceled(CancelMatchmakingTicketResult result) { }
#endif
        }

        public void OnClickLocalButton()
        {
            SceneManager.LoadScene("PlayScene");
        }

        IEnumerator Polling(GetMatchmakingTicketRequest request)
        {
            // ポーリングは1分間に10回まで許可されているので、6秒間隔で実行するのがおすすめです。
            var seconds = 6f;
            matchedOrCanceled = false;

            while (true)
            {
                if (matchedOrCanceled || !disconnectButton.interactable)
                {
                    outputText.text = "";
                    yield break;
                }

                PlayFabMultiplayerAPI.GetMatchmakingTicket(request, OnGetMatchmakingTicket, OnMatchmakingError);
                yield return new WaitForSeconds(seconds);
            }

            void OnGetMatchmakingTicket(GetMatchmakingTicketResult result)
            {
                Debug.Log(result.Status);
                switch (result.Status)
                {
                    case "Matched":
                        matchedOrCanceled = true;
                        outputText.text = "対戦相手が見つかりました！";
                        JoinOrCreateRoom(result.MatchId);
                        return;

                    case "Canceled":
                        matchedOrCanceled = true;
                        outputText.text = "キャンセルしました";
                        connectButton.interactable = true;
                        disconnectButton.interactable = false;
                        return;

                    default:
                        outputText.text = "対戦相手を探しています";
                        return;
                }
            }
        }

        void JoinOrCreateRoom(string MatchId)
        {
            var request = new GetMatchRequest
            {
                MatchId = MatchId,
                ReturnMemberAttributes = true,
                QueueName = queueName,
            };
            PlayFabMultiplayerAPI.GetMatch(request, OnGetMatch, OnMatchmakingError);

            void OnGetMatch(GetMatchResult result)
            {
                photonManager.SetProperties(playerDataManager.playerData);
                photonManager.JoinOrCreateRoom(MatchId);
            }
        }

        void OnMatchmakingError(PlayFabError error)
        {
            Debug.LogError($"{error.ErrorMessage}");
        }

        void OnFailure (PlayFabError error)
        {
            Debug.LogError($"{error.ErrorMessage}");
        }
    }
}
