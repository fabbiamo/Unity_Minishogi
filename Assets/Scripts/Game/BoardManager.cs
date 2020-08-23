#define DROPDOWN

using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Shogi;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Game
{
    public class BoardManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        [SerializeField]
        private LayerMask squareMask_ = default;

        [SerializeField]
        private Text b_rook = default, b_bishop = default, b_gold = default, b_silver = default, b_pawn = default;

        [SerializeField]
        private Text w_rook = default, w_bishop = default, w_gold = default, w_silver = default, w_pawn = default;

#if DROPDOWN
        [SerializeField]
        private Dropdown kifDropdown = default;
#endif

        // テーブル
        private GameObject[] pieces_ = new GameObject[(int)PieceNo.NB];
        private Text[] counters_ = new Text[10];
        private Sprite[] sprites_;
        
        // 記録
        private Position shogiPosition_ = GameCore.Position;
        private List<Move> moveList_;
        private GameFormat gameFormat_;

        // Prefab
        private GameObject pieceObjectPrefab, dialogObjectPrefab;
        
        private BoardController boardController_;

        public ref GameObject Pieces(PieceNo pn) { return ref pieces_[(int)pn]; }
        public ref Text Counters(SquareHand sq) { return ref counters_[sq - SquareHand.Hand]; }
        public ref BoardController BoardController() { return ref boardController_; }

        public bool isExistNextMove()
        {
#if DROPDOWN
            return shogiPosition_.gamePly == kifDropdown.value;
#else
            return false;
#endif
            // return shogiPosition_.gamePly >= kifDropdown.value;
        }

        public string KifString() 
        {
           
            string kif = "";
            foreach (var move in moveList_)
                kif += ' ' + USIExtensions.USI(move);
            return kif;
        }

        private static bool isMove;
        public bool IsMove() { return isMove; }
        public void MoveReset() { isMove = false; }

        public void Init()
        {
            GameCore.Init();

            moveList_ = new List<Move>();

            boardController_ = new BoardController();

            // Load
            pieceObjectPrefab = Resources.Load("Piece") as GameObject;
            dialogObjectPrefab = Resources.Load("PromoteDialog") as GameObject;
            sprites_ = Resources.LoadAll<Sprite>("koma_v1");

            GeneratePiece(Square.SQ_55, Piece.B_KING);
            GeneratePiece(Square.SQ_54, Piece.B_PAWN);
            GeneratePiece(Square.SQ_45, Piece.B_GOLD);
            GeneratePiece(Square.SQ_35, Piece.B_SILVER);
            GeneratePiece(Square.SQ_25, Piece.B_BISHOP);
            GeneratePiece(Square.SQ_15, Piece.B_ROOK);
            GeneratePiece(Square.SQ_11, Piece.W_KING);
            GeneratePiece(Square.SQ_12, Piece.W_PAWN);
            GeneratePiece(Square.SQ_21, Piece.W_GOLD);
            GeneratePiece(Square.SQ_31, Piece.W_SILVER);
            GeneratePiece(Square.SQ_41, Piece.W_BISHOP);
            GeneratePiece(Square.SQ_51, Piece.W_ROOK);
        }

        public void NewGame(GameFormat gameFormat)
        {
            shogiPosition_.SetHirate();
            gameFormat_ = gameFormat;       
            boardController_.boardState = BoardState.Playing;

            moveList_.Clear();

#if DROPDOWN
            // dropdown
            kifDropdown.options.RemoveRange(1, kifDropdown.options.Count - 1);
            kifDropdown.template.GetComponentInChildren<Toggle>().interactable = false;
#endif
            MoveReset();

#region オブジェクトの配置
            Shogi.Color myColor;
            switch (gameFormat)
            {
                case GameFormat.LocalHumanCpu:
                case GameFormat.OnlineBlack:
                    myColor = Shogi.Color.BLACK;
                    break;
                case GameFormat.LocalCpuHuman:
                case GameFormat.OnlineWhite:
                    myColor = Shogi.Color.WHITE;
                    break;
                case GameFormat.LocalHumanHuman:
                default:
                    myColor = Shogi.Color.NB;
                    break;
            }
            PutPiece(Square.SQ_55, Piece.B_KING,   myColor);
            PutPiece(Square.SQ_54, Piece.B_PAWN,   myColor);
            PutPiece(Square.SQ_45, Piece.B_GOLD,   myColor);
            PutPiece(Square.SQ_35, Piece.B_SILVER, myColor);
            PutPiece(Square.SQ_25, Piece.B_BISHOP, myColor);
            PutPiece(Square.SQ_15, Piece.B_ROOK,   myColor);
            PutPiece(Square.SQ_11, Piece.W_KING,   myColor);
            PutPiece(Square.SQ_12, Piece.W_PAWN,   myColor);
            PutPiece(Square.SQ_21, Piece.W_GOLD,   myColor);
            PutPiece(Square.SQ_31, Piece.W_SILVER, myColor);
            PutPiece(Square.SQ_41, Piece.W_BISHOP, myColor);
            PutPiece(Square.SQ_51, Piece.W_ROOK,   myColor);
#endregion

#region テキストテーブルの初期化
            bool isRotation = (myColor == Shogi.Color.WHITE);
            if (!isRotation)
            {
                counters_[0] = b_pawn;
                counters_[1] = b_silver;
                counters_[2] = b_gold;
                counters_[3] = b_bishop;
                counters_[4] = b_rook;
                counters_[5] = w_pawn;
                counters_[6] = w_silver;
                counters_[7] = w_gold;
                counters_[8] = w_bishop;
                counters_[9] = w_rook;
            }
            else
            {
                counters_[0] = w_pawn;
                counters_[1] = w_silver;
                counters_[2] = w_gold;
                counters_[3] = w_bishop;
                counters_[4] = w_rook;
                counters_[5] = b_pawn;
                counters_[6] = b_silver;
                counters_[7] = b_gold;
                counters_[8] = b_bishop;
                counters_[9] = b_rook;
            }
#endregion
        }

        void GeneratePiece(Square sq, Piece pc)
        {
            var us = pc.PieceColor();
            var pt = pc.Type();
            var pieceObject = Instantiate(pieceObjectPrefab, transform);
            
            pieceObject.name = "PIECE";
            pieceObject.transform.position = BoardConst.SquareToPosition(sq) + BoardConst.AdjustPosition(pc);

            // Sprite
            var sr = pieceObject.GetComponent<SpriteRenderer>();
            sr.sprite = GetSprite(pt);
            sr.flipX = sr.flipY = us == Shogi.Color.WHITE;

            // Box Collider2D
            pieceObject.GetComponent<BoxCollider2D>().enabled = true;

            // PieceNo
            PieceNo pn = shogiPosition_.PieceNoOn(sq);
            if (pn != PieceNo.NONE)
                Pieces(pn) = pieceObject;
        }

        void PutPiece(Square sq, Piece pc, Shogi.Color humanColor)
        {
            var us = pc.PieceColor();
            var pt = pc.Type();

            PieceNo pn = shogiPosition_.PieceNoOn(sq);
            if (pn != PieceNo.NONE)
            {
                var pieceObject = Pieces(pn);
                
                // position
                pieceObject.transform.position = BoardConst.SquareToPosition(sq) + BoardConst.AdjustPosition(pc);

                // sprite
                var sr = pieceObject.GetComponent<SpriteRenderer>();
                sr.sprite = GetSprite(pt);
                sr.flipX = sr.flipY = us == Shogi.Color.WHITE;

                // Box Collider2D
                pieceObject.GetComponent<BoxCollider2D>().enabled
                    = (humanColor == Shogi.Color.NB || humanColor == us);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (boardController_.boardState == BoardState.Finished)
            {
                return;
            }
            else if (boardController_.boardState == BoardState.PromoteDialog)
            {
                // ダイアログが出現している
                if (boardController_.promoteDialogSelect == PromoteDialogSelect.Null)
                    return;

                var m = Util.MakeMove(boardController_.pickedFrom, boardController_.pickedTo,
                    boardController_.promoteDialogSelect == PromoteDialogSelect.Promote);

                if (gameFormat_.isLocal())
                    UpdateKifInfo(m);
                else
                    photonView.RPC(nameof(MoveObject), RpcTarget.All, m);

                boardController_.boardState = BoardState.Playing;
            }
            else if (Input.GetMouseButtonDown(0))
            {
                // ダイアログが出ていない、かつマウスがクリックされている
                Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Collider2D collider = Physics2D.OverlapPoint(pos);

                if (collider == null)
                    return;

                // 升の色を変更するための関数
                void SetSquareColor(SquareHand sq, UnityEngine.Color c)
                {
                    var collider2 = Physics2D.OverlapPoint(BoardConst.SquareToPosition(sq), squareMask_);
                    if (collider2 != null)
                        collider2.GetComponent<SpriteRenderer>().color = c;
                }

                switch (boardController_.boardState)
                {
                    case BoardState.Playing:
                        {
                            if (collider.name != "PIECE")
                                return; // 駒がない

                            SquareHand from = BoardConst.MakeSquare(collider.transform.position);
                            if (GameCore.IsOk_PickedFrom(from))
                            {
                                boardController_.pickedFrom = from;
                                boardController_.boardState = BoardState.Picked;
                                SetSquareColor(from, BoardConst.SelectedColor);
                            }
                            break;
                        }

                    case BoardState.Picked:
                        {
                            var from = boardController_.pickedFrom;
                            var sq = BoardConst.MakeSquare(collider.transform.position);

                            if (sq == from)
                            {
                                boardController_.boardState = BoardState.Playing;
                                SetSquareColor(sq, BoardConst.InitialColor);
                            }
                            else
                            {
                                if (GameCore.IsOk_PickedTo(sq))
                                {
                                    boardController_.pickedTo = sq;
                                    SetSquareColor(from, BoardConst.InitialColor);
                                    MovePiece();
                                }
                                else if (GameCore.IsOk_PickedFrom(sq))
                                {
                                    SetSquareColor(from, BoardConst.InitialColor);
                                    SetSquareColor(sq, BoardConst.SelectedColor);
                                    boardController_.pickedFrom = sq;
                                }
                            }
                            break;
                        }
                }
            }
        }

        void MovePiece()
        {
            var us = shogiPosition_.sideToMove;

            var from = boardController_.pickedFrom;
            var to = boardController_.pickedTo;
            var pt = shogiPosition_.PieceOn(from).Type();

            bool canPromote = (from.IsBoardPiece() && pt < Piece.GOLD) ?
                (Util.CanPromote(us, (Square)from) | Util.CanPromote(us, (Square)to)) : false;

            // (1) 駒打ち
            // (2) 成ることができないとき
            // (3) 不成ができないとき
            // (4) 成り・不成が選択できるとき

            Move m = Move.NONE;
            if (canPromote && pt == Piece.PAWN)
            {
                // (3)
                m = Util.MakeMove(from, to, true);
            }
            else
            {
                // (1)(2)(4)
                // (4)の場合、どちらか片方が非合法手になることはないはず...
                m = Util.MakeMove(from, to, false);
            }

            if (!shogiPosition_.IsLegal(m))
            {
                Debug.Log("Ilegal Move: " + USIExtensions.USI(m));
                return;
            }
            //Debug.Log(USIExtensions.USI(m));

            if (canPromote && pt != Piece.PAWN)
            {
                // (4)
                GameObject dialogObject = Instantiate(dialogObjectPrefab, transform) as GameObject;
                
                var pc = Util.MakePiece(us, pt);
                dialogObject.GetComponent<PromoteDialog>().PutDialog(pc, BoardConst.SquareToPosition(boardController_.pickedTo));
                
                boardController_.boardState = BoardState.PromoteDialog;
                boardController_.promoteDialogSelect = PromoteDialogSelect.Null;
            }
            else
            {
                // (1)(2)(3)
                boardController_.boardState = BoardState.Playing;

                if (gameFormat_.isLocal())
                    UpdateKifInfo(m); /*MoveObject(m)*/
                else
                    photonView.RPC(nameof(MoveObject), RpcTarget.All, m);
            }
        }

        [PunRPC]
        void MoveObject(Move m)
        {
            Debug.Log(USIExtensions.USI(m));

            if (m.IsSpecial())
            {
                // gamestate
                boardController_.boardState = BoardState.Finished;

#if DROPDOWN
                // dropdown interctable == true
                kifDropdown.template.GetComponentInChildren<Toggle>().interactable = true;
#endif
                return;
            }

            var us = shogiPosition_.sideToMove;
            var from = m.From();

            Piece moved_pc = m.IsDrop() ? m.DroppedPiece() : shogiPosition_.PieceOn(from);
            PieceNo pn;

            if (m.IsDrop())
            {
                // 駒打ち
                int cnt = shogiPosition_.Hand(us).Count(moved_pc);
                pn = shogiPosition_.HandPieceNo(us, moved_pc, cnt - 1);

                // テキストを非表示にする
                Counters(Util.MakeSquareHand(us, moved_pc)).text = null;
            }
            else
            {
                pn = shogiPosition_.PieceNoOn(from);
            }

            Debug.Assert(pn != PieceNo.NONE);
            var picked_from = Pieces(pn);

            if (m.IsPromote())
            {
                // 駒を成る
                var pt = moved_pc.Type();

                // 画像を変更
                picked_from.GetComponent<SpriteRenderer>().sprite = GetSprite(pt.ToInt() + Piece.PROMOTE);
            }

            // 駒をfromからtoに移動
            picked_from.transform.position = BoardConst.SquareToPosition(m.To()) + BoardConst.AdjustPosition(moved_pc);

            PieceNo pn2 = shogiPosition_.PieceNoOn(m.To());
            if (pn2 != PieceNo.NONE)
            {
                var picked_to = Pieces(pn2);
                var to_pr = shogiPosition_.PieceOn(m.To()).RawType();

                // 画像と反転
                var sr = picked_to.GetComponent<SpriteRenderer>();
                sr.sprite = GetSprite(to_pr);
                sr.flipX = sr.flipY = !sr.flipX;

                // 敵の駒をtoから駒台に移動
                picked_to.transform.position = BoardConst.SquareToPosition(Util.MakeSquareHand(us, to_pr));

                // BoxColider2D
                picked_to.GetComponent<BoxCollider2D>().enabled = true;

                // テキストを表示
                if (shogiPosition_.Hand(us).Count(to_pr) == 1)
                    Counters(Util.MakeSquareHand(us, to_pr)).text = "2";
            }

            GameCore.DoMove(m);

            isMove = true;
        }

        void UndoObject()
        {
            var us = shogiPosition_.sideToMove.Not();
            var st = shogiPosition_.State();
            var move = st.lastMove;

            Square to = move.To();
            Piece to_pc = shogiPosition_.PieceOn(to);

            PieceNo pn = shogiPosition_.PieceNoOn(to);
            Debug.Assert(pn != PieceNo.NONE);
            var picked_to = Pieces(pn);

            SquareHand from = move.IsDrop()
                ? Util.MakeSquareHand(us, move.DroppedPiece())
                : (SquareHand)move.From();

            if (move.IsPromote())
            {
                // 駒を成る
                var pr = to_pc.RawType();

                // 画像を変更
                picked_to.GetComponent<SpriteRenderer>().sprite = GetSprite(pr);
            }

            // 駒をtoからfromに移動
            picked_to.transform.position = BoardConst.SquareToPosition(from) + BoardConst.AdjustPosition(to_pc);

            if (st.capturedPiece != Piece.NO_PIECE)
            {
                // 駒台の駒をtoに移動
                Piece pc = st.capturedPiece;
                Piece pr = pc.RawType();
                int cnt = shogiPosition_.Hand(us).Count(pr);
                PieceNo pn2 = shogiPosition_.HandPieceNo(us, pr, cnt - 1);
                Debug.Assert(pn2 != PieceNo.NONE);

                var picked_hand = Pieces(pn2);

                // 画像と反転
                var sr = picked_hand.GetComponent<SpriteRenderer>();
                sr.sprite = GetSprite(pc.Type());
                sr.flipX = sr.flipY = !sr.flipX;

                // 駒を駒台からtoに移動
                picked_hand.transform.position = BoardConst.SquareToPosition(to);

                // BoxColider2D
                var box2D = picked_hand.GetComponent<BoxCollider2D>();
                box2D.enabled = gameFormat_ == GameFormat.LocalHumanHuman ?
                    true : !box2D.enabled;

                // テキストを非表示にする
                Counters(Util.MakeSquareHand(us, pr)).text = null;
            }

            GameCore.UnDoMove();
        }

        public void UpdateKifInfo(Move m)
        {
            // 1. moveList
            moveList_.Add(m);

            // 2. kifDropdown
            var us = shogiPosition_.sideToMove;
            Piece moved_pc = m.IsDrop() ? m.DroppedPiece() : shogiPosition_.PieceOn(m.From());

            // Arialの場合
            //    ply     space
            //   1 -  9     5
            //  10 - 99     3
            // 100 -        1

            string kifText =
                (m.IsSpecial() ? "     " : (shogiPosition_.gamePly + 1).ToString().PadRight(4, ' '))
                + Shogi.Kif.ToKif(m, us, moved_pc);

#if DROPDOWN
            kifDropdown.options.Add(new Dropdown.OptionData { text = kifText });

            kifDropdown.value = kifDropdown.options.Count;
#endif
            // --> OnValueChanged()
        }

        public void DoMove(string strMove)
        {
            if (strMove.Length <= 3)
                return;

            bool promote = strMove.Length == 5 && strMove[4] == '+';
            bool drop = strMove[1] == '*';

            var to = Util.MakeSquare(strMove[2], strMove[3]);
            if (drop)
            {
                var pr = Util.MakePiece(strMove[0]);
                UpdateKifInfo(Util.MakeMoveDrop(pr, to));
            }
            else
            {
                var from = Util.MakeSquare(strMove[0], strMove[1]);
                if (promote)
                    UpdateKifInfo(Util.MakeMovePromote(from, to));
                else
                    UpdateKifInfo(Util.MakeMove(from, to));
            }
        }

        public void Undo(bool isRemove)
        {
            if (isRemove)
            {
                var len = moveList_.Count;
                moveList_.RemoveAt(len - 1);
#if DROPDOWN
                kifDropdown.options.RemoveAt(len);
#endif
            }
#if DROPDOWN
            kifDropdown.value -= 1; /* value == len - 1 */
#endif
        }

        public void Skip(int target)
        {
#if DROPDOWN
            kifDropdown.value = target;
#endif
        }

        public void SkipEnd()
        {
            Skip(moveList_.Count);
        }

        public void OnValueChanged()
        {
            int ply = GameCore.Position.gamePly;
#if DROPDOWN
            int cnt = kifDropdown.value - ply;

            if (cnt == 0)
            {
                return;
            }
            else if (cnt > 0)
            {
                for (int i = 0; i < cnt; ++i)
                    MoveObject(moveList_[ply + i]);
            }
            else
            {
                for (int i = 0; i < -cnt; ++i)
                    UndoObject();
            }
#endif
        }

        public void SaveKifu()
        {
            Data.KifuManager.Save(moveList_);
        }

        public Sprite GetSprite(Piece pt)
        {
            Debug.Assert(Piece.PAWN <= pt && pt < Piece.QUEEN);
            return System.Array.Find(sprites_, (sprite) => sprite.name.Equals(BoardConst.ImagePath[(int)pt]));
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
        }
    }
}
