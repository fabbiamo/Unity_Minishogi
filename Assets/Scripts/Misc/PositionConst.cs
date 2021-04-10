using Assets.Scripts.Shogi;
using UnityEngine;

namespace Assets.Scripts.Misc {
    public class PositionConst {
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
        const float RookHandX = -1.2f;
        const float RookHandY = -2.4f;

        public static SquareHand MakeSquare(Vector3 pos) {
            if (pos.y == RookHandY) {
                // 先手駒台
                return (SquareHand)(SquareHand.HandBlack.ToInt() + (int)(4 - (pos.x - RookHandX) / PIECE_X));
            }
            else if (pos.y == -RookHandY) {
                // 後手駒台
                return (SquareHand)(SquareHand.HandWhite.ToInt() + (int)(4 + (pos.x + RookHandX) / PIECE_X));
            }
            else {
                // 盤上
                // 座標の微調整を考慮してから計算
                float x = Mathf.Ceil(pos.x * 10) / 10;      // x方向負に調整 --> 切り上げ 
                float y = Mathf.Floor(pos.y * 100) / 100;   // y方向正に調整 --> 切り捨て

                return (SquareHand)(12 - (int)(x / PIECE_X * 5) - (int)(y / PIECE_Y));
            }
        }

        public static Vector3 SquareToPosition(Square sq) {
            return new Vector3(PIECE_X * (2 - sq.ToFile().ToInt()), PIECE_Y * (2 - sq.ToRank().ToInt()), 0);
        }

        public static Vector3 SquareToPosition(SquareHand sq) {
            if (sq.IsBoardPiece())
                return SquareToPosition((Square)sq);

            int index = (int)sq.ToPiece();

            if (sq < SquareHand.HandWhite)
                return new Vector3(RookHandX + HAND_X[index], RookHandY, 0);
            else
                return new Vector3(-RookHandX - HAND_X[index], -RookHandY, 0);
        }

        private static readonly float[] HAND_X = { 0, PIECE_X * 4, 0, 0, PIECE_X * 3, PIECE_X, 0, PIECE_X * 2 };
    }
}
