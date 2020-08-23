using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Data;
using System.Diagnostics;

namespace Assets.Scripts.Game
{
    public class LocalGameServer : MonoBehaviour
    {
        [SerializeField] 
        Camera sceneCamera = default;

        [SerializeField]
        Button StartButton = default, UndoButton = default, RedoButton = default, EndButton = default;

        [SerializeField]
        Button DoOverButton = default, ResignButton = default;

        //private bool useEngine_;
        private Process process_;
        private EngineState engineState_;
        private BoardManager boardManager_;
        private Queue<string> moveQueue_ = new Queue<string>();
        private Shogi.Color humanColor_;

        // Start is called before the first frame update
        void Start()
        {
            // 1. engine settings
            // 2. game settings

            // 3. new game
            //boardManager_ = GameObject.FindWithTag("BOARD").GetComponent<BoardManager>();
            boardManager_ = GetComponentInChildren<BoardManager>();
            boardManager_.Init();
            NewGame(GameFormat.LocalHumanHuman);
        }

        // Update is called once per frame
        void Update()
        {
            // Button
            bool isEnd = boardManager_.BoardController().boardState == BoardState.Finished;

            StartButton.interactable = UndoButton.interactable =
                isEnd && GameCore.Position.gamePly >= 1;
            EndButton.interactable = RedoButton.interactable =
                isEnd && boardManager_.isExistNextMove();

            int minPly = humanColor_ == Shogi.Color.WHITE ? 2 : 1;
            DoOverButton.interactable = !isEnd && GameCore.Position.gamePly >= minPly;
            ResignButton.interactable = !isEnd;

            if (engineState_ == EngineState.Null)
                return;

            if (isEnd)
            {
                process_.StandardInput.WriteLine("quit");
                engineState_ = EngineState.Null;
                return;
            }

            if (engineState_ == EngineState.ReadyOk && GameCore.Position.sideToMove != humanColor_)
            {
                string positionCmd = "position startpos";

                // position startpos moves ...
                var kifStr = boardManager_.KifString();
                if (!string.IsNullOrWhiteSpace(kifStr))
                    positionCmd += " moves" + kifStr;
                // UnityEngine.Debug.Log(positionCmd);

                process_.StandardInput.WriteLine(positionCmd);
                process_.StandardInput.WriteLine("go byoyomi 2000");

                engineState_ = EngineState.Thinking;
            }

            if (engineState_ == EngineState.Thinking && moveQueue_.Count > 0)
            {
                boardManager_.DoMove(moveQueue_.Dequeue());
                engineState_ = EngineState.ReadyOk;
            }
        }

        public void NewGame(GameFormat gameFormat, int blackValue = -1, int whiteValue = -1)
        {
            bool useEngine = false;

            // 1. clear
            moveQueue_.Clear();

            // 2. board manager
            string path = "";
            switch (gameFormat)
            {
                case GameFormat.LocalHumanHuman:
                    humanColor_ = Shogi.Color.NB;
                    break;
                case GameFormat.LocalHumanCpu:
                    useEngine = true;
                    humanColor_ = Shogi.Color.BLACK;
                    path = SaveData.Instance.EnginePathList[whiteValue];
                    break;
                case GameFormat.LocalCpuHuman:
                    useEngine = true;
                    humanColor_ = Shogi.Color.WHITE;
                    path = SaveData.Instance.EnginePathList[blackValue];
                    CameraRotation();
                    break;
                default:
                    return;
            }
            boardManager_.NewGame(gameFormat);

            // 3. Run Engine
            if (useEngine)
            {
                engineState_ = EngineState.Run;

                var psi = EngineProcessInfo.RunEngine(path);
                process_ = Process.Start(psi);
                process_.OutputDataReceived += PrintOutputData;
                process_.ErrorDataReceived += PrintErrorData;

                // 標準出力・標準エラーの非同期読み込みを開始する
                process_.BeginOutputReadLine();
                process_.BeginErrorReadLine();

                process_.StandardInput.WriteLine("usi");
            }
            else
            {
                engineState_ = EngineState.Null;
            }
        }

        /// <summary>
        /// Call GameFormat.CpuHuman Only
        /// </summary>
        public void CameraRotation()
        {
            sceneCamera.transform.rotation = Quaternion.Euler(0, 0, 180);
        }

        public void OnClickStartButton()
        {
            boardManager_.Skip(0);
        }

        public void OnClickUndoButton()
        {
            boardManager_.Undo(false);
        }

        public void OnClickRedoButton()
        {
            //boardManager_.Redo();
            boardManager_.Skip(GameCore.Position.gamePly + 1);
        }

        public void OnClickEndButton()
        {
            boardManager_.SkipEnd();
        }

        /// <summary>
        /// UseEngine == true  : Undo 2 moves
        /// UseEngine == false : Undo 1 move
        /// </summary>
        public void OnClickDoOverButton()
        {
            boardManager_.Undo(true);

            if (engineState_ != EngineState.Null)
                boardManager_.Undo(true);
        }

        public void OnClickResignButton()
        {
            //process_.StandardInput.WriteLine("quit");
            boardManager_.UpdateKifInfo(Shogi.Move.RESIGN);
        }

        public void OnClickSaveButton()
        {
            boardManager_.SaveKifu();
        }

        void PrintOutputData(object sender, DataReceivedEventArgs e)
        {
            //Process p = (Process)sender;

            if (string.IsNullOrEmpty(e.Data))
                return;

            string[] cmd = e.Data.Split(' ');

            switch (cmd[0])
            {
                case "usiok":
                    if (engineState_ == EngineState.Run)
                    {
                        engineState_ = EngineState.UsiOk;
                        process_.StandardInput.WriteLine("isready");
                    }
                    break;

                case "readyok":
                    if (engineState_ == EngineState.UsiOk)
                    {
                        engineState_ = EngineState.ReadyOk;
                        process_.StandardInput.WriteLine("usinewgame");
                    }
                    break;

                case "bestmove":
                    {
                        moveQueue_.Enqueue(cmd[1]);
                        break;
                    }

                default:
                    break;
            }
        }

        void PrintErrorData(object sender, DataReceivedEventArgs e)
        {
            //Process p = (Process)sender;

            if (!string.IsNullOrEmpty(e.Data))
                UnityEngine.Debug.LogError(e.Data);
        }
    }
}
