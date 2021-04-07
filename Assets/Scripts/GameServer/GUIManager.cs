using System.Collections.Generic;
using System.Text;
using Assets.Scripts.Misc;
using Assets.Scripts.Shogi;
using Photon.Pun;
using UnityEngine;
using SColor = Assets.Scripts.Shogi.Color;

namespace Assets.Scripts.GameServer
{
    public class GUIManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        [SerializeField]
        GameObject controlObject = default;

        [SerializeField]
        LayerMask squareMask = default;

        BoardManager BoardManager { get; set; }

        MouseControl MouseControl { get; set; }
        
        List<Move> MoveList { get; set; }

        Position Position { get { return BoardManager.Position; } }

        ScreenControl screenControl;

        SColor MyColor { get; set; } = SColor.NB;

        public SColor SideToMove { get { return Position.sideToMove; } }

        bool IsGameEnded { get; set; } = false;

        int CurrentValue { get; set; } = 0;

        void Awake()
        {
            BoardManager = new BoardManager();
            MouseControl = new MouseControl();
            MoveList = new List<Move>();
            screenControl = controlObject.GetComponent<ScreenControl>();
        }

        void Update()
        {
            if (IsGameEnded && MyColor != SColor.NB)
                return;

            void SetSquareColor(SquareHand sq, UnityEngine.Color c)
            {
                var collider = Physics2D.OverlapPoint(PositionConst.SquareToPosition(sq), squareMask);
                if (collider != null)
                    collider.GetComponent<SpriteRenderer>().color = c;
            }

            SquareHand from, to;
            if (Input.GetMouseButtonDown(0))
            {
                var collider = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                if (collider == null)
                    return;

                switch (MouseControl.state)
                {
                    case ScreenStateEnum.None:
                        if (collider.tag != "Piece")
                            return;

                        from = PositionConst.MakeSquare(collider.transform.position);
                        if (BoardManager.IsOkPickedFrom(from))
                        {
                            MouseControl.pickedFrom = from;
                            MouseControl.state = ScreenStateEnum.Picked;
                            SetSquareColor(from, SelectedColor);
                        }
                        break;

                    case ScreenStateEnum.Picked:
                        from = MouseControl.pickedFrom;
                        to = PositionConst.MakeSquare(collider.transform.position);

                        if (from == to)
                        {
                            MouseControl.state = ScreenStateEnum.None;
                            SetSquareColor(to, InitialColor);
                        }
                        else
                        {
                            if (BoardManager.IsOkPickedTo(to))
                            {
                                MouseControl.pickedTo = to;
                                SetSquareColor(from, InitialColor);
                                DoMoveGUI();
                            }
                            else if (BoardManager.IsOkPickedFrom(to))
                            {
                                SetSquareColor(from, InitialColor);
                                SetSquareColor(to, SelectedColor);
                                MouseControl.pickedFrom = to;
                            }
                        }
                        break;

                    case ScreenStateEnum.PromoteDialog:
                        if (MouseControl.select == PromoteDialogSelectEnum.None)
                        {
                            if (collider.name == "Pro")
                                MouseControl.select = PromoteDialogSelectEnum.Promote;
                            else if (collider.name == "NonPro")
                                MouseControl.select = PromoteDialogSelectEnum.NonPromote;
                            else
                                return;

                            Destroy(collider.transform.parent.gameObject);
                        }

                        var m = Util.MakeMove(MouseControl.pickedFrom, MouseControl.pickedTo,
                            MouseControl.select == PromoteDialogSelectEnum.Promote);

                        if (GameServer.IsOnline)
                            photonView.RPC(nameof(DoMove), RpcTarget.All, m);
                        else
                            DoMove(m);

                        MouseControl.state = ScreenStateEnum.None;
                        break;
                }
            }
        }

        public void Init()
        {
            BoardManager.Init();
            Prefabs.Load();
            SpriteManager.Load();
        }

        public void NewGame(SColor us)
        {
            Position.SetHirate();
            SetBoard(us);
            MyColor = us;
            IsGameEnded = false;
            CurrentValue = 0;

            MoveList.Clear();
            MouseControl.state = ScreenStateEnum.None;
            
            screenControl.gameObject.SetActive(!GameServer.IsOnline);
            screenControl.Clear();
            screenControl.Interactable(MyColor == SColor.NB, IsGameEnded);
        }

        void SetBoard(SColor us)
        {
            var sq = new Square[] {
                Square.SQ_55, Square.SQ_54, Square.SQ_45, Square.SQ_35, Square.SQ_25, Square.SQ_15,
                Square.SQ_11, Square.SQ_12, Square.SQ_21, Square.SQ_31, Square.SQ_41, Square.SQ_51,
            };
            var piece = new Piece[] {
                Piece.B_KING, Piece.B_PAWN, Piece.B_GOLD, Piece.B_SILVER, Piece.B_BISHOP, Piece.B_ROOK,
                Piece.W_KING, Piece.W_PAWN, Piece.W_GOLD, Piece.W_SILVER, Piece.W_BISHOP, Piece.W_ROOK,
            };

            Prefabs.Clear();
            for (int i = 0; i < 12; ++i)
                Prefabs.PutPiece(sq[i], piece[i], Position.PieceNoOn(sq[i]), us, transform);
        }

        void DoMoveGUI()
        {
            IsGameEnded &= MyColor != SColor.NB;
            var us = Position.sideToMove;
            var from = MouseControl.pickedFrom;
            var to = MouseControl.pickedTo;
            var pt = Position.PieceOn(from).Type();

            bool canPromote = (from.IsBoardPiece() && pt < Piece.GOLD) ?
                (Util.CanPromote(us, (Square)from) | Util.CanPromote(us, (Square)to)) : false;

            // 1. 駒打ち
            // 2. 成ることができないとき
            // 3. 不成ができないとき
            // 4. 成り・不成が選択できるとき

            Move m = Move.NONE;
            if (canPromote && pt == Piece.PAWN)      // 3
                m = Util.MakeMove(from, to, true);
            else
                m = Util.MakeMove(from, to, false);  // 1,2,4

            if (!Position.IsLegal(m))
            {
                MouseControl.state = ScreenStateEnum.None;
                return;
            }

            if (canPromote && pt != Piece.PAWN) // 4
            {
                Prefabs.PutPromotedialog((Square)MouseControl.pickedTo, Position.PieceOn(from), transform);
                MouseControl.state = ScreenStateEnum.PromoteDialog;
                MouseControl.select = PromoteDialogSelectEnum.None;
            }
            else // 1,2,3
            {
                MouseControl.state = ScreenStateEnum.None;
                if (GameServer.IsOnline)
                    photonView.RPC(nameof(DoMove), RpcTarget.All, m);
                else
                    DoMove(m);
            }
        }

        [PunRPC]
        public void DoMove(Move move, bool overwrite = true)
        {
            //Debug.Log(USIExtensions.USI(move));

            if (overwrite)
            {
                // 上書きする指し手の数
                var num = MoveList.Count - Position.gamePly;
                if (num > 0)
                {
                    // 棋譜を上書きする
                    RemoveItem(Position.gamePly, num);
                    MoveList.RemoveRange(Position.gamePly, num);
                    CurrentValue = Position.gamePly;
                }

                EntryItem(move);
                MoveList.Add(move);
            }
            ++CurrentValue;
            //Debug.Log(CurrentValue);

            IsGameEnded |= move.IsSpecial();
            screenControl.Interactable(MyColor == SColor.NB, IsGameEnded);
            
            if (move.IsSpecial())
                return;

            var us = Position.sideToMove;
            var from = move.From();

            Piece moved_pc = move.IsDrop() ? move.DroppedPiece() : Position.PieceOn(from);
            Piece moved_after_pc = move.IsPromote() ? moved_pc.ToInt() + Piece.PROMOTE : moved_pc;

            PieceNo pn = move.IsDrop()
                ? Position.HandPieceNo(us, moved_pc, Position.Hand(us).Count(moved_pc) - 1)
                : Position.PieceNoOn(from);
            
            Debug.Assert(pn != PieceNo.NONE);
            Prefabs.MovePiece((SquareHand)move.To(), moved_after_pc, pn, move.IsPromote());

            PieceNo pn2 = Position.PieceNoOn(move.To());
            if (pn2 != PieceNo.NONE)
            {
                var to_pr = Position.PieceOn(move.To()).RawType();
                var sq = Util.MakeSquareHand(us, to_pr);
                Prefabs.CapturePiece(sq, to_pr, pn2, true);

                // テキストを表示
                // if (Position.Hand(us).Count(toPr) == 1)
                //    Counters(Util.MakeSquareHand(us, toPr)).text = "2";
            }

            BoardManager.DoMove(move);
        }

        void UndoMove(Move move, bool overwrite = true)
        {
            --CurrentValue;

            if (overwrite)
            {
                RemoveItem(MoveList.Count - 1, 1);
                MoveList.RemoveAt(MoveList.Count - 1);
            }
            screenControl.Interactable(MyColor == SColor.NB, IsGameEnded);
            
            if (move.IsSpecial())
                return;

            var us = Position.sideToMove.Not();
            var st = Position.State();
            Debug.Assert(move == st.lastMove);

            Square to = move.To();
            Piece to_pc = Position.PieceOn(to);

            PieceNo pn = Position.PieceNoOn(to);
            SquareHand from = move.IsDrop()
                ? Util.MakeSquareHand(us, move.DroppedPiece())
                : (SquareHand)move.From();
            Piece from_pc = move.IsPromote() ? to_pc.RawType() : to_pc;
            Prefabs.MovePiece(from, from_pc, pn, move.IsPromote());

            Piece cap_pc = st.capturedPiece;
            if (cap_pc != Piece.NO_PIECE)
            {
                // 駒台の駒をtoに移動
                Piece pr = cap_pc.RawType();
                PieceNo pn2 = Position.HandPieceNo(us, pr, Position.Hand(us).Count(pr) - 1);
                
                Debug.Assert(pn2 != PieceNo.NONE);
                Prefabs.CapturePiece((SquareHand)to, cap_pc.Type(), pn2, MyColor == SColor.NB);

                // テキストを非表示にする
                // Counters(Util.MakeSquareHand(us, pr)).text = null;
            }

            BoardManager.UnDoMove();
        }

        void EntryItem(Move move)
        {
            StringBuilder kif = new StringBuilder();
            //kif.Append(move.IsSpecial() ? "     " : (Position.gamePly + 1).ToString().PadRight(4, ' '));
            kif.Append((Position.gamePly + 1).ToString());
            kif.Append("\t ");
            kif.Append(Kif.ToKif(move, Position.sideToMove, move.IsDrop() ? move.DroppedPiece() : Position.PieceOn(move.From())));
            screenControl.EntryItem(kif.ToString());
        }

        void RemoveItem(int index, int count)
        {
            screenControl.RemoveItem(index, count);
        }

        public void OnClickUndoButton()
        {
            if (GameServer.IsOnline)
                return; // 非表示のはずだが

            UndoMove(MoveList[Position.gamePly - 1]);
            if (MyColor != SColor.NB)
                UndoMove(MoveList[Position.gamePly - 1]); // CPU対戦のときは2手ずつ戻す
        }

        public void OnClickResignButton()
        {
            if (GameServer.IsOnline)
                return; // 非表示のはずだが

            DoMove(Move.RESIGN);
        }

        public void OnClickDropdownItem()
        {
            int diff = screenControl.DropdownValue - CurrentValue;
            //Debug.LogFormat($"{screenControl.DropdownValue} {CurrentValue}");
            if (diff >= 0)
                for (int value = CurrentValue; value < screenControl.DropdownValue; ++value)
                    DoMove(MoveList[value], false);
            else
                for (int value = CurrentValue - 1; value > screenControl.DropdownValue - 1; --value)
                    UndoMove(MoveList[value], false);
        }

        public string ToPositionCommand()
        {
            StringBuilder command = new StringBuilder("position startpos");
            if (MoveList.Count > 0)
            {
                command.Append(" moves");
                foreach (var move in MoveList)
                    command.Append(string.Format(" {0}", USIExtensions.USI(move)));
            }
            return command.ToString();
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            throw new System.NotImplementedException();
        }

        private static readonly UnityEngine.Color InitialColor = new UnityEngine.Color(253f / 255f, 201f / 255f, 176f / 255f, 249f / 255f);

        private static readonly UnityEngine.Color SelectedColor = new UnityEngine.Color(247f / 255f, 1.0f, 0.0f, 1.0f);

    }
}
