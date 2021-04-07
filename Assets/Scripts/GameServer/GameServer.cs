using Assets.Scripts.LocalEngineManager;
using Assets.Scripts.Shogi;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using SColor = Assets.Scripts.Shogi.Color;

namespace Assets.Scripts.GameServer
{
    public class GameServer : MonoBehaviourPunCallbacks
    {
        [SerializeField]
        GameObject GUIObject = default;

        [SerializeField]
        Camera SceneCamera = default;

        SColor MyColor { set; get; } = SColor.NB;

        GUIManager GUIManager { set; get; }

        public static bool IsOnline { get { return PhotonNetwork.InRoom; } }

        LocalEngineProcess localEngineProcess = null;

        bool isWin;
        double myRating, opponentRating;

        void Awake()
        {
            GUIManager = GUIObject.GetComponent<GUIManager>();
        }

        void Start()
        {
            var us =
                !IsOnline ? SColor.NB :
                PhotonNetwork.IsMasterClient ? SColor.BLACK : SColor.WHITE;

            if (IsOnline)
            {
                foreach (var player in PhotonNetwork.PlayerList)
                {
                    if (player.IsLocal)
                        myRating = (double)player.CustomProperties["Rating"];
                    else
                        opponentRating = (double)player.CustomProperties["Rating"];
                }
            }

            StartGame(us);
        }

        void Update()
        {
            if (localEngineProcess == null || GUIManager.SideToMove == MyColor)
                return;

            if (localEngineProcess.ReadyOk && !localEngineProcess.Thinking)
            {
                Think();
                while (localEngineProcess.BestMove == Move.NONE) ;
                DoMove(localEngineProcess.BestMove);

                localEngineProcess.BestMove = Move.NONE;
                localEngineProcess.Thinking = false;
            }
        }

        public void StartGame(SColor us, string enginePath = null)
        {
            SceneCamera.transform.rotation = Quaternion.Euler(0, 0, us == SColor.WHITE ? 180 : 0);
            GUIManager.gameObject.SetActive(true);
            GUIManager.Init();
            GUIManager.NewGame(us);
            MyColor = us;
            
            if (enginePath == null)
            {
                localEngineProcess = null;
            }
            else
            {
                localEngineProcess = new LocalEngineProcess();
                localEngineProcess.RunEngine(enginePath);
            }
        }

        void DoMove(Move m)
        {
            // 合法手の判定するべき
            GUIManager.DoMove(m);
        }

        void Think()
        {
            if (localEngineProcess != null)
                localEngineProcess.Think(GUIManager.ToPositionCommand());
        }

        void LeaveRoom(bool isWin)
        {
            this.isWin = isWin;
            PhotonNetwork.LeaveRoom();
        }

        public override void OnLeftRoom()
        {
            SceneManager.sceneLoaded += TitleSceneLoaded;
            SceneManager.LoadScene("TitleScene");
        }

        private void TitleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var playerDataManager = GameObject.FindWithTag("DataManager").GetComponent<PlayerData.PlayerDataManager>();
            playerDataManager.UpdateData(Misc.EloRating.Update(myRating, opponentRating, isWin), isWin);
            SceneManager.sceneLoaded -= TitleSceneLoaded;
        }
    }
}
