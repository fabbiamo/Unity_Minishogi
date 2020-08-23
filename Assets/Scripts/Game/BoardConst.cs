using UnityEngine;
using Assets.Scripts.Shogi;
using System;

// 駒のスケール
// v1 縦64, 横60, scale40
// box collider size x = 1.53, y = 1.66

// v2   
// 縦90, 横81, scale54-56
// box collider size x = 1, y = 1

namespace Assets.Scripts.Game
{
    public enum GameFormat
    {
        None,
        LocalHumanHuman,
        LocalHumanCpu,
        LocalCpuHuman,
        OnlineBlack,
        OnlineWhite,
        NB,

        Zero = 1,
        LocalNB = LocalCpuHuman + 1,
    }

    public static class GameFormatExtensions
    {
        public static bool isLocal(this GameFormat g) { return GameFormat.Zero <= g && g < GameFormat.LocalNB; }
    }

    public enum EngineState
    {
        Null,
        Run,
        UsiOk,
        ReadyOk,
        Thinking,
        //Quit,
    };

    public enum BoardState
    {
        Playing,
        Picked,
        PromoteDialog,
        Finished,
    };

    public enum PromoteDialogSelect
    {
        Null,
        Promote,
        NonPromote,
    };

    /// <summary>
    ///  状態の集合
    /// </summary>
    public class BoardController
    {
        // 盤面の状態
        public BoardState boardState;

        // マウスの入力先
        public SquareHand pickedFrom;
        public SquareHand pickedTo;

        // ダイアログの選択
        public PromoteDialogSelect promoteDialogSelect;
    };


    public class BoardConst
    {
        /// <summary>
        /// 駒の横の長さ
        /// </summary>
        public const float PIECE_X = 0.6f;

        /// <summary>
        /// 駒の縦の長さ
        /// </summary>
        public const float PIECE_Y = 0.64f;

        /// <summary>
        /// 駒台の座標(飛車)
        /// </summary>
        private const float RookHandX = -1.2f;
        private const float RookHandY = -2.4f;

#if false
        /// <summary>
        /// 駒台からの相対位置
        /// </summary>
        public static readonly Vector3 diff_BLACK = new Vector3(19f, -30f, 0f);
        public static readonly Vector3 diff_WHITE = new Vector3(20f,  47f, 0f);
#endif

        public static readonly UnityEngine.Color InitialColor =
#if false
        new UnityEngine.Color(1.0f, 1.0f, 1.0f, 1.0f);
#else
        new UnityEngine.Color(253f / 255f, 201f / 255f, 176f / 255f, 249f / 255f);
#endif
        public static readonly UnityEngine.Color SelectedColor = new UnityEngine.Color(247f / 255f, 1.0f, 0.0f, 1.0f);

        /// <summary>
        /// 升から座標の変換
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static Vector3 SquareToPosition(Square sq)
        {
            return new Vector3(PIECE_X * (2 - sq.ToFile().ToInt()), PIECE_Y * (2 - sq.ToRank().ToInt()), 0);
        }

        /// <summary>
        /// 座標の微調整
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static Vector3 AdjustPosition(Piece pc)
        {
            switch(pc)
            {
                case Piece.W_SILVER:  return new Vector3(-0.02f,      0f, 0f);
                case Piece.W_ROOK:    return new Vector3(-0.02f,      0f, 0f);
                case Piece.W_KING:    return new Vector3(-0.03f, +0.008f, 0f);
                default:              return new Vector3(    0f,      0f, 0f);
            }
        }

        /// <summary>
        /// 升から座標の変換(駒台を含む)
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static Vector3 SquareToPosition(SquareHand sq)
        {
            if (sq.IsBoardPiece())
                return SquareToPosition((Square)sq);

            int index = (int)sq.ToPiece();

            if (sq < SquareHand.HandWhite)
                return new Vector3(RookHandX + HAND_X[index], RookHandY, 0);
            else
                return new Vector3(-RookHandX - HAND_X[index], -RookHandY, 0);
        }

        /// <summary>
        /// 座標から升への変換
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static SquareHand MakeSquare(Vector3 pos)
        {
            if (pos.y == RookHandY)
            {
                // 先手駒台
                return (SquareHand)(SquareHand.HandBlack.ToInt() + (int)(4 - (pos.x - RookHandX) / PIECE_X));
            }
            else if (pos.y == -RookHandY)
            {
                // 後手駒台
                return (SquareHand)(SquareHand.HandWhite.ToInt() + (int)(4 + (pos.x + RookHandX) / PIECE_X));
            }
            else
            {
                // 盤上

                // 座標の微調整を考慮してから計算
                float x = Mathf.Ceil(pos.x * 10) / 10;      // x方向負に調整 --> 切り上げ 
                float y = Mathf.Floor(pos.y * 100) / 100;   // y方向正に調整 --> 切り捨て

                return (SquareHand)(12 - (int)(x / PIECE_X * 5) - (int)(y / PIECE_Y));
            }
        }

#if true
        public static readonly string[] ImagePath =
        {
        ""       , "piece_0" , "piece_1" , "piece_2" , "piece_3" , "piece_4" , "piece_5" , "piece_6",
        "piece_7", "piece_28", "piece_29", "piece_30", "piece_31", "piece_32", "piece_33", ""       ,
    };
#else
    public static readonly string[] ImagePath =
    {
        ""      , "koma_7" , "", "", "koma_4" , "koma_2" , "koma_1" , "koma_3",
        "koma_0", "koma_14", "", "", "koma_11", "koma_10", "koma_9" , ""      ,
    };
#endif

        /*
            public static readonly string[] ImagePath =
            {
               ""     , "koma0", "", "", "koma3" , "koma4" , "koma5" , "koma6",
               "koma7", "koma8", "", "", "koma11", "koma12", "koma13", ""     ,
            };
        */

        private static readonly float[] HAND_X = { 0, PIECE_X * 4, 0, 0, PIECE_X * 3, PIECE_X, 0, PIECE_X * 2 };

        //private static readonly float[] HAND_Y = { 0, -PIECE_Y * 2, 0, 0, -PIECE_Y, 0, 0, -PIECE_Y };
    }
}