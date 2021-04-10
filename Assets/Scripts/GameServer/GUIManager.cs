using System.Collections.Generic;
using System.Text;
using Assets.Scripts.Misc;
using Assets.Scripts.Shogi;
using Photon.Pun;
using UnityEngine;
using SColor = Assets.Scripts.Shogi.Color;

namespace Assets.Scripts.GameServer {
    public class GUIManager : MonoBehaviourPunCallbacks, IPunObservable {
        [SerializeField]
        GameObject ControlForLocalObject = default;

        [SerializeField]
        GameObject ControlForOnlineObject = default;

        [SerializeField]
        LayerMask SquareMask = default;

        BoardManager BoardManager { get; set; }

        MouseControl MouseControl { get; set; }

        List<Move> MoveList { get; set; }

        ScreenControl ScreenControl;

        int CurrentValue { get; set; } = 0;

        public bool UpdateIgnore { get; set; } = false;

        public Position Position { get { return BoardManager.Position; } }

        public SColor MyColor { get; private set; } = SColor.NB;

        public SColor Winner { get; private set; } = SColor.NB;

        void Awake() {
            BoardManager = new BoardManager();
            MouseControl = new MouseControl();
            MoveList = new List<Move>();
            ScreenControl = ControlForLocalObject.GetComponent<ScreenControl>();
        }

        void Update() {
            if (Winner != SColor.NB || UpdateIgnore)
                return;

            void SetSquareColor(SquareHand sq, UnityEngine.Color c) {
                var collider = Physics2D.OverlapPoint(PositionConst.SquareToPosition(sq), SquareMask);
                if (collider != null)
                    collider.GetComponent<SpriteRenderer>().color = c;
            }

            SquareHand from, to;
            if (Input.GetMouseButtonDown(0)) {
                var collider = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                if (collider == null)
                    return;

                switch (MouseControl.State) {
                case MouseStateEnum.None:
                    if (collider.tag != "Piece")
                        return;

                    from = PositionConst.MakeSquare(collider.transform.position);
                    if (BoardManager.IsOkPickedFrom(from)) {
                        MouseControl.PickedFrom = from;
                        MouseControl.State = MouseStateEnum.Picked;
                        SetSquareColor(from, SelectedColor);
                    }
                    break;

                case MouseStateEnum.Picked:
                    from = MouseControl.PickedFrom;
                    to = PositionConst.MakeSquare(collider.transform.position);

                    if (from == to) {
                        MouseControl.State = MouseStateEnum.None;
                        SetSquareColor(to, InitialColor);
                    }
                    else {
                        if (BoardManager.IsOkPickedTo(to)) {
                            MouseControl.PickedTo = to;
                            SetSquareColor(from, InitialColor);
                            DoMoveGUI();
                        }
                        else if (BoardManager.IsOkPickedFrom(to)) {
                            SetSquareColor(from, InitialColor);
                            SetSquareColor(to, SelectedColor);
                            MouseControl.PickedFrom = to;
                        }
                    }
                    break;

                case MouseStateEnum.PromoteDialog:
                    if (MouseControl.Select == PromoteDialogSelectEnum.None) {
                        if (collider.name == "Pro")
                            MouseControl.Select = PromoteDialogSelectEnum.Promote;
                        else if (collider.name == "NonPro")
                            MouseControl.Select = PromoteDialogSelectEnum.NonPromote;
                        else
                            return;

                        Destroy(collider.transform.parent.gameObject);
                    }

                    var m = Util.MakeMove(MouseControl.PickedFrom, MouseControl.PickedTo,
                        MouseControl.Select == PromoteDialogSelectEnum.Promote);

                    if (GameServer.IsOnline)
                        photonView.RPC(nameof(DoMove), RpcTarget.All, m);
                    else
                        DoMove(m);

                    MouseControl.State = MouseStateEnum.None;
                    break;
                }
            }
        }

        public void Init() {
            BoardManager.Init();
            PiecePrefabs.Load();
            SpriteManager.Load();

            ControlForLocalObject.SetActive(!GameServer.IsOnline);
            ControlForOnlineObject.SetActive(GameServer.IsOnline);
        }

        public void NewGame(SColor us) {
            Position.SetHirate();
            SetBoard(us);
            MyColor = us;
            Winner = SColor.NB;
            CurrentValue = 0;

            MoveList.Clear();
            MouseControl.State = MouseStateEnum.None;

            ScreenControl.Clear();
            ScreenControl.Interactable(MyColor == SColor.NB, false);
        }

        void SetBoard(SColor us) {
            var sq = new Square[] {
                Square.SQ_55, Square.SQ_54, Square.SQ_45, Square.SQ_35, Square.SQ_25, Square.SQ_15,
                Square.SQ_11, Square.SQ_12, Square.SQ_21, Square.SQ_31, Square.SQ_41, Square.SQ_51,
            };
            var piece = new Piece[] {
                Piece.B_KING, Piece.B_PAWN, Piece.B_GOLD, Piece.B_SILVER, Piece.B_BISHOP, Piece.B_ROOK,
                Piece.W_KING, Piece.W_PAWN, Piece.W_GOLD, Piece.W_SILVER, Piece.W_BISHOP, Piece.W_ROOK,
            };

            PiecePrefabs.Clear();
            for (int i = 0; i < 12; ++i)
                PiecePrefabs.PutPiece(sq[i], piece[i], Position.PieceNoOn(sq[i]), us, transform);
        }

        void DoMoveGUI() {
            Winner = MyColor == SColor.NB ? SColor.NB : Winner;
            var us = Position.sideToMove;
            var from = MouseControl.PickedFrom;
            var to = MouseControl.PickedTo;
            var pt = Position.PieceOn(from).Type();

            bool canPromote = (from.IsBoardPiece() && pt < Piece.GOLD)
                ? (Util.CanPromote(us, (Square)from) | Util.CanPromote(us, (Square)to))
                : false;

            // 1. 駒打ち
            // 2. 成ることができないとき
            // 3. 不成ができないとき
            // 4. 成り・不成が選択できるとき

            Move m = Move.NONE;
            if (canPromote && pt == Piece.PAWN)      // 3
                m = Util.MakeMove(from, to, true);
            else
                m = Util.MakeMove(from, to, false);  // 1,2,4

            if (!Position.IsLegal(m)) {
                MouseControl.State = MouseStateEnum.None;
                return;
            }

            if (canPromote && pt != Piece.PAWN) // 4
            {
                PiecePrefabs.PutPromotedialog((Square)MouseControl.PickedTo, Position.PieceOn(from), transform);
                MouseControl.State = MouseStateEnum.PromoteDialog;
                MouseControl.Select = PromoteDialogSelectEnum.None;
            }
            else // 1,2,3
            {
                MouseControl.State = MouseStateEnum.None;
                if (GameServer.IsOnline)
                    photonView.RPC(nameof(DoMove), RpcTarget.All, m, true);
                else
                    DoMove(m);
            }
        }

        [PunRPC]
        public void DoMove(Move move, bool overwrite = true) {
            //Debug.Log(USIExtensions.USI(move));

            if (overwrite) {
                // 上書きする指し手の数
                var num = MoveList.Count - Position.gamePly;
                if (num > 0) {
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

            var us = Position.sideToMove;
            var from = move.From();

            if (move.IsSpecial()) {
                Winner = us.Not();
                ScreenControl.Interactable(MyColor == SColor.NB, true);
                return;
            }
            ScreenControl.Interactable(MyColor == SColor.NB, false);

            Piece movedPc = move.IsDrop() ? move.DroppedPiece() : Position.PieceOn(from);
            Piece movedAfterPc = move.IsPromote() ? movedPc.ToInt() + Piece.PROMOTE : movedPc;

            PieceNo pn = move.IsDrop()
                ? Position.HandPieceNo(us, movedPc, Position.Hand(us).Count(movedPc) - 1)
                : Position.PieceNoOn(from);

            Debug.Assert(pn != PieceNo.NONE);
            PiecePrefabs.MovePiece((SquareHand)move.To(), movedAfterPc, pn, move.IsPromote());

            PieceNo pn2 = Position.PieceNoOn(move.To());
            if (pn2 != PieceNo.NONE) {
                var toPr = Position.PieceOn(move.To()).RawType();
                var sq = Util.MakeSquareHand(us, toPr);
                PiecePrefabs.CapturePiece(sq, toPr, pn2, true);

                // テキストを表示
                // if (Position.Hand(us).Count(toPr) == 1)
                //    Counters(Util.MakeSquareHand(us, toPr)).text = "2";
            }

            BoardManager.DoMove(move);
        }

        void UndoMove(Move move, bool overwrite = true) {
            --CurrentValue;

            if (overwrite) {
                RemoveItem(MoveList.Count - 1, 1);
                MoveList.RemoveAt(MoveList.Count - 1);
            }
            ScreenControl.Interactable(MyColor == SColor.NB, Winner != SColor.NB);

            if (move.IsSpecial())
                return;

            var us = Position.sideToMove.Not();
            var st = Position.State();
            Debug.Assert(move == st.lastMove);

            Square to = move.To();
            Piece toPc = Position.PieceOn(to);

            PieceNo pn = Position.PieceNoOn(to);
            SquareHand from = move.IsDrop()
                ? Util.MakeSquareHand(us, move.DroppedPiece())
                : (SquareHand)move.From();
            Piece fromPc = move.IsPromote() ? toPc.RawType() : toPc;
            PiecePrefabs.MovePiece(from, fromPc, pn, move.IsPromote());

            Piece capPc = st.capturedPiece;
            if (capPc != Piece.NO_PIECE) {
                // 駒台の駒をtoに移動
                Piece pr = capPc.RawType();
                PieceNo pn2 = Position.HandPieceNo(us, pr, Position.Hand(us).Count(pr) - 1);

                Debug.Assert(pn2 != PieceNo.NONE);
                PiecePrefabs.CapturePiece((SquareHand)to, capPc.Type(), pn2, MyColor == SColor.NB);

                // テキストを非表示にする
                // Counters(Util.MakeSquareHand(us, pr)).text = null;
            }

            BoardManager.UnDoMove();
        }

        void EntryItem(Move move) {
            StringBuilder kif = new StringBuilder();
            //kif.Append(move.IsSpecial() ? "     " : (Position.gamePly + 1).ToString().PadRight(4, ' '));
            kif.Append((Position.gamePly + 1).ToString());
            kif.Append("\t ");
            kif.Append(Kif.ToKif(move, Position.sideToMove, move.IsDrop() ? move.DroppedPiece() : Position.PieceOn(move.From())));
            ScreenControl.EntryItem(kif.ToString());
        }

        void RemoveItem(int index, int count) {
            ScreenControl.RemoveItem(index, count);
        }

        public void OnClickUndoButton() {
            if (GameServer.IsOnline)
                return; // 非表示のはずだが

            UndoMove(MoveList[Position.gamePly - 1]);
            if (MyColor != SColor.NB)
                UndoMove(MoveList[Position.gamePly - 1]); // CPU対戦のときは2手ずつ戻す
        }

        public void OnClickResignButton() {
            DoMove(Move.RESIGN);
        }

        public void OnClickDropdownItem() {
            int diff = ScreenControl.DropdownValue - CurrentValue;
            //Debug.Log($"{screenControl.DropdownValue} {CurrentValue}");
            if (diff >= 0)
                for (int value = CurrentValue; value < ScreenControl.DropdownValue; ++value)
                    DoMove(MoveList[value], false);
            else
                for (int value = CurrentValue - 1; value > ScreenControl.DropdownValue - 1; --value)
                    UndoMove(MoveList[value], false);
        }

        public string ToPositionCommand() {
            StringBuilder command = new StringBuilder("position startpos");
            if (MoveList.Count > 0) {
                command.Append(" moves");
                foreach (var move in MoveList)
                    command.Append(string.Format(" {0}", USIExtensions.USI(move)));
            }
            return command.ToString();
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        }

        private static readonly UnityEngine.Color InitialColor = new UnityEngine.Color(253f / 255f, 201f / 255f, 176f / 255f, 249f / 255f);

        private static readonly UnityEngine.Color SelectedColor = new UnityEngine.Color(247f / 255f, 1.0f, 0.0f, 1.0f);

    }
}
