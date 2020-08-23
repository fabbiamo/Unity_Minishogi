using Assets.Scripts.Matching;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.Game
{
    // MonoBehaviourではなくMonoBehaviourPunCallbacksを継承して、Photonのコールバックを受け取れるようにする
    /// <summary>
    /// Unity: 2019.3.3f1
    /// Pun: 2.16.0
    /// Photon lib: 
    /// </summary>
    public class GameInit : MonoBehaviourPunCallbacks
    {
        [SerializeField] GameObject firstPanel = default;
        [SerializeField] GameObject secondPanel = default;
        [SerializeField] Camera sceneCamera = default;
        [SerializeField] Button resignButton = default;
        private int maxPlayers = 2;

        BoardManager board;
        MatchingState matchingState;
        Shogi.Color myColor;
        Timer firstTimer, secondTimer;

        void Start()
        {
            if (PhotonNetwork.InRoom)
            {
                matchingState = MatchingState.Start;
                myColor = PhotonNetwork.IsMasterClient ? Shogi.Color.BLACK : Shogi.Color.WHITE;

                CameraSetting();

                board = GameObject.FindWithTag("BOARD").GetComponent<BoardManager>();
                board.Init();

                if (myColor == Shogi.Color.BLACK)
                    board.NewGame(GameFormat.OnlineBlack);
                else
                    board.NewGame(GameFormat.OnlineWhite);

                firstPanel.SetActive(false);
                secondPanel.SetActive(false);

                firstTimer = firstPanel.transform.GetChild(1).GetComponent<Timer>();
                secondTimer = secondPanel.transform.GetChild(1).GetComponent<Timer>();
            }
        }

        void Update()
        {
            if (!PhotonNetwork.InRoom)
                return;

            if ((matchingState == MatchingState.Start) || (matchingState == MatchingState.Playing && board.IsMove()))
            {
                if (GameCore.Position.sideToMove == myColor)
                {
                    firstPanel.SetActive(true);
                    secondPanel.SetActive(false);
                    firstTimer.SetTimer();
                }
                else
                {
                    firstPanel.SetActive(false);
                    secondPanel.SetActive(true);
                    secondTimer.SetTimer();
                }

                matchingState = MatchingState.Playing;
                board.MoveReset();
            }

            // 勝ち負けの判定
            if (matchingState == MatchingState.Playing)
            {
                var gameResult = GameCore.IsEndGame();
                bool isWin;

                switch (gameResult)
                {
                    case GameResult.BlackWin:
                    case GameResult.WhiteWin:
                        {
                            isWin = gameResult == GameResult.BlackWin
                                ? (myColor == Shogi.Color.BLACK)
                                : (myColor == Shogi.Color.WHITE);

                            LeaveRoom(isWin);
                            break;
                        }
                    default:
                        break;
                }

                if (PhotonNetwork.PlayerList.Length < maxPlayers)
                {
                    LeaveRoom(true);
                }
            }
        }

        public void CameraSetting()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                sceneCamera.enabled = true;
            }
            else
            {
                sceneCamera.transform.rotation = Quaternion.Euler(0, 0, 180);
            }
        }

        public void Resign()
        {
            if (PhotonNetwork.InRoom)
            {
                LeaveRoom(false);
            }
        }

        private void LeaveRoom(bool isWin)
        {
            matchingState = MatchingState.Fin;

            firstPanel.SetActive(false);
            secondPanel.SetActive(false);

            PlayFabLogin.UpdateUserData(isWin);

            PhotonNetwork.LeaveRoom();
        }

        public override void OnLeftRoom()
        {
            Debug.Log("LeftRoom");

            SceneManager.LoadScene("TitleScene");
        }
    }
}