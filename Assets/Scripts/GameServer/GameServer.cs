using Assets.Scripts.LocalEngine;
using Assets.Scripts.Shogi;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using SColor = Assets.Scripts.Shogi.Color;

namespace Assets.Scripts.GameServer {
    public class GameServer : MonoBehaviourPunCallbacks {
        [SerializeField]
        GameObject GUIObject = default;

        [SerializeField]
        GameObject ResultPanel = default;

        [SerializeField]
        Camera SceneCamera = default;

        GUIManager GUIManager { set; get; }

        public static bool IsOnline { get { return PhotonNetwork.InRoom; } }

        LocalEngineProcess LocalEngineProcess = null;

        bool IsWin;
        double MyRating, OpponentRating;

        void Awake() {
            GUIManager = GUIObject.GetComponent<GUIManager>();
        }

        void Start() {
            var us = !IsOnline ? SColor.NB
                     : PhotonNetwork.IsMasterClient ? SColor.BLACK
                     : SColor.WHITE;

            if (IsOnline) {
                foreach (var player in PhotonNetwork.PlayerList) {
                    if (player.IsLocal)
                        MyRating = (double)player.CustomProperties["Rating"];
                    else
                        OpponentRating = (double)player.CustomProperties["Rating"];
                }
            }

            StartGame(us);
        }

        void Update() {

            if (IsOnline) {
                if (GUIManager.Winner != SColor.NB) {
                    //LeaveRoom(GUIManager.Winner == GUIManager.MyColor);
                    IsWin = GUIManager.Winner == GUIManager.MyColor;
                    SetResultPanel();
                }

                if (PhotonNetwork.PlayerList.Length < 2) {
                    //LeaveRoom(true); // 接続切れ勝ちとする
                    IsWin = true;
                    SetResultPanel();
                }
            }
            else {
                if (LocalEngineProcess == null)
                    return;

                if (GUIManager.Winner != SColor.NB)
                    LocalEngineProcess.QuitEngine();

                // エンジンの手番であるか
                if (GUIManager.Position.sideToMove == GUIManager.MyColor)
                    return;

                if (LocalEngineProcess.ReadyOk && !LocalEngineProcess.Thinking) {
                    Think();
                    while (LocalEngineProcess.BestMove == Move.NONE) ;
                    DoMove(LocalEngineProcess.BestMove);

                    LocalEngineProcess.BestMove = Move.NONE;
                    LocalEngineProcess.Thinking = false;
                }
            }
        }

        public void StartGame(SColor us, string enginePath = null) {
            SceneCamera.transform.rotation = Quaternion.Euler(0, 0, us == SColor.WHITE ? 180 : 0);
            GUIManager.gameObject.SetActive(true);
            GUIManager.Init();
            GUIManager.NewGame(us);

            if (enginePath == null) {
                LocalEngineProcess = null;
            }
            else {
                LocalEngineProcess = new LocalEngineProcess();
                LocalEngineProcess.RunEngine(enginePath);
            }
        }

        void DoMove(Move m) {
            // 合法手の判定するべき
            GUIManager.DoMove(m);
        }

        void Think() {
            if (LocalEngineProcess != null)
                LocalEngineProcess.Think(GUIManager.ToPositionCommand());
        }

        void SetResultPanel() {
            ResultPanel.GetComponentInChildren<TextMeshProUGUI>().text
                = string.Format($"{GUIManager.Position.gamePly}手にて\nあなたの{(IsWin ? "勝ち" : "負け")}");
            ResultPanel.SetActive(true);
        }

        public void LeaveRoom() {
            PhotonNetwork.LeaveRoom();
        }

        public void LoadScene() {
            SceneManager.LoadScene("TitleScene");
        }

        public override void OnLeftRoom() {
            SceneManager.sceneLoaded += TitleSceneLoaded;
            SceneManager.LoadScene("TitleScene");
        }

        void TitleSceneLoaded(Scene scene, LoadSceneMode mode) {
            var dataManager = GameObject.FindWithTag("DataManager").GetComponent<PlayerData.PlayerDataManager>();
            dataManager.EntryTask((MyRating, OpponentRating, IsWin));
            SceneManager.sceneLoaded -= TitleSceneLoaded;
        }
    }
}
