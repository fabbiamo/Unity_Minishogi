using System;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Shogi
{
    public enum Color { BLACK, WHITE, NB, ZERO = 0 }

    public static class ColorExtensions
    {
        public static Int32 ToInt(this Color c) { return (Int32)c; }
        public static Color Not(this Color c) { return (Color)(c.ToInt() ^ 1); }
        public static string Pretty(this Color c) { return (c == Color.BLACK) ? "先手" : "後手"; }
    }

    public enum File { FILE_1, FILE_2, FILE_3, FILE_4, FILE_5, NB, ZERO = 0 }

    public static class FileExtensions
    {
        private static readonly string[] FileToString = { "１", "２", "３", "４", "５" };
        public static Int32 ToInt(this File f) { return (Int32)f; }
        public static string Pretty(this File f) { return FileToString[(int)f]; }
    }    

    public enum Rank { RANK_1, RANK_2, RANK_3, RANK_4, RANK_5, NB, ZERO = 0 }

    public static class RankExtensions
    {
        private static readonly string[] RankToString = { "一", "二", "三", "四", "五" };
        public static Int32 ToInt(this Rank r) { return (Int32)r; }
        public static string Pretty(this Rank r) { return RankToString[(int)r]; }
    }

    public enum Square
    { 
        SQ_11, SQ_12, SQ_13, SQ_14, SQ_15,
        SQ_21, SQ_22, SQ_23, SQ_24, SQ_25,
        SQ_31, SQ_32, SQ_33, SQ_34, SQ_35,
        SQ_41, SQ_42, SQ_43, SQ_44, SQ_45,
        SQ_51, SQ_52, SQ_53, SQ_54, SQ_55,
        NB, ZERO = 0,

        SQ_U = -1, SQ_D = +1, SQ_L = +5, SQ_R = -5,
        SQ_RU = SQ_R + SQ_U,
        SQ_RD = SQ_R + SQ_D,
    }

    public static class SquareExtensions
    {
        public static Int32 ToInt(this Square sq) { return (Int32)sq; }
        public static File ToFile(this Square sq) { return (File)(sq.ToInt() / 5); }
        public static Rank ToRank(this Square sq) { return (Rank)(sq.ToInt() % 5); }
        public static string Pretty(this Square sq) { return sq.ToFile().Pretty() + sq.ToRank().Pretty(); }
        public static bool IsOk(this Square sq) { return Square.ZERO <= sq && sq < Square.NB; }
    }

    public enum Piece
    {
        NO_PIECE, PAWN, LANCE, KNIGHT, SILVER, BISHOP, ROOK, GOLD, KING,
        PRO_PAWN, PRO_LANCE, PRO_KNIGHT, PRO_SILVER, HORSE, DRAGON, QUEEN,

        B_PAWN = 1, B_LANCE, B_KNIGHT, B_SILVER, B_BISHOP, B_ROOK, B_GOLD, B_KING,
        B_PRO_PAWN, B_PRO_LANCE, B_PRO_KNIGHT, B_PRO_SILVER, B_HORSE, B_DRAGON, B_QUEEN,
        W_PAWN = 17, W_LANCE, W_KNIGHT, W_SILVER, W_BISHOP, W_ROOK, W_GOLD, W_KING,
        W_PRO_PAWN, W_PRO_LANCE, W_PRO_KNIGHT, W_PRO_SILVER, W_HORSE, W_DRAGON, W_QUEEN,

        NB = 32, ZERO = 0,
        PROMOTE = 8,
        WHITE = 16,
        HAND_NB = KING,

        // byTypeBB用
        ALL = 0,
        GOLDS = QUEEN,
        HDK,
        BISHOP_HORSE,
        ROOK_DRAGON,
        SILVER_HDK,
        GOLDS_HDK,
        BB_NB,
    }

    public static class PieceExtensions
    {
        private static readonly string[] PieceToString =
        {
            "　口", "　歩", "　香", "　桂", "　銀", "　角", "　飛", "　金",
            "　玉", "　と", "　杏", "　圭", "　全", "　馬", "　龍", "　菌",
            "　口", "＾歩", "＾香", "＾桂", "＾銀", "＾角", "＾飛", "＾金",
            "＾玉", "＾と", "＾杏", "＾圭", "＾全", "＾馬", "＾龍", "＾菌",
        };

        public static Int32 ToInt(this Piece pc) { return (Int32)pc; }
        public static Color PieceColor(this Piece pc) { return pc == Piece.NO_PIECE ? Color.NB : pc < Piece.WHITE ? Color.BLACK : Color.WHITE; }
        public static string Pretty(this Piece pc) { return PieceToString[(int)pc]; }
        public static bool IsOk(this Piece pc) { return Piece.PAWN <= pc && pc < Piece.NB; }

        /// <summary>
        /// 後手の歩→先手の歩のように、後手という属性を取り払った駒種を返す
        /// </summary>
        public static Piece Type(this Piece pc) { return (Piece)(pc.ToInt() & 0xf); }

        /// <summary>
        /// 成ってない駒を返す。後手という属性も消去する。
        /// 例) 成銀→銀 , 後手の馬→先手の角 , 先手玉→先手の玉
        /// NO_PIECEはNO_PIECEが返る。
        /// </summary>
        public static Piece RawType(this Piece pc)
        {
            if (pc == Piece.NO_PIECE || pc == Piece.WHITE)
                return Piece.NO_PIECE;

            // KINGがNO_PIECEになってしまうといけないので、1引いてから下位3bit取り出して、1足しておく。
            return (Piece)((((int)pc - 1) & 7) + 1);
        }
    }

    // bit 0..4   : Square
    // bit 5      : 1(borrow)
    // bit 6..9   : 右に存在する升の数(bit9はborrow)
    // bit 10..13 : 上
    // bit 14..17 : 下
    // bit 18..21 : 左
    public enum SquareWithWall
    {
        SQWW_R = Square.SQ_R - (1 << 6) + (1 << 18),
        SQWW_U = Square.SQ_U - (1 << 10) + (1 << 14),
        SQWW_D = -SQWW_U,
        SQWW_L = -SQWW_R,
        SQWW_RU = SQWW_R + SQWW_U,
        SQWW_RD = SQWW_R + SQWW_D,
        SQWW_LU = SQWW_L + SQWW_U,
        SQWW_LD = SQWW_L + SQWW_D,

        SQWW_11 = Square.SQ_11 | (1 << 5) | (0 << 6) | (0 << 10) | (4 << 14) | (4 << 18),
        SQWW_12 = Square.SQ_12 | (1 << 5) | (0 << 6) | (1 << 10) | (3 << 14) | (4 << 18),
        SQWW_13 = Square.SQ_13 | (1 << 5) | (0 << 6) | (2 << 10) | (2 << 14) | (4 << 18),
        SQWW_14 = Square.SQ_14 | (1 << 5) | (0 << 6) | (3 << 10) | (1 << 14) | (4 << 18),
        SQWW_15 = Square.SQ_15 | (1 << 5) | (0 << 6) | (4 << 10) | (0 << 14) | (4 << 18),
        SQWW_21 = Square.SQ_21 | (1 << 5) | (1 << 6) | (0 << 10) | (4 << 14) | (3 << 18),
        SQWW_22 = Square.SQ_22 | (1 << 5) | (1 << 6) | (1 << 10) | (3 << 14) | (3 << 18),
        SQWW_23 = Square.SQ_23 | (1 << 5) | (1 << 6) | (2 << 10) | (2 << 14) | (3 << 18),
        SQWW_24 = Square.SQ_24 | (1 << 5) | (1 << 6) | (3 << 10) | (1 << 14) | (3 << 18),
        SQWW_25 = Square.SQ_25 | (1 << 5) | (1 << 6) | (4 << 10) | (0 << 14) | (3 << 18),
        SQWW_31 = Square.SQ_31 | (1 << 5) | (2 << 6) | (0 << 10) | (4 << 14) | (2 << 18),
        SQWW_32 = Square.SQ_32 | (1 << 5) | (2 << 6) | (1 << 10) | (3 << 14) | (2 << 18),
        SQWW_33 = Square.SQ_33 | (1 << 5) | (2 << 6) | (2 << 10) | (2 << 14) | (2 << 18),
        SQWW_34 = Square.SQ_34 | (1 << 5) | (2 << 6) | (3 << 10) | (1 << 14) | (2 << 18),
        SQWW_35 = Square.SQ_35 | (1 << 5) | (2 << 6) | (4 << 10) | (0 << 14) | (2 << 18),
        SQWW_41 = Square.SQ_41 | (1 << 5) | (3 << 6) | (0 << 10) | (4 << 14) | (1 << 18),
        SQWW_42 = Square.SQ_42 | (1 << 5) | (3 << 6) | (1 << 10) | (3 << 14) | (1 << 18),
        SQWW_43 = Square.SQ_43 | (1 << 5) | (3 << 6) | (2 << 10) | (2 << 14) | (1 << 18),
        SQWW_44 = Square.SQ_44 | (1 << 5) | (3 << 6) | (3 << 10) | (1 << 14) | (1 << 18),
        SQWW_45 = Square.SQ_45 | (1 << 5) | (3 << 6) | (4 << 10) | (0 << 14) | (1 << 18),
        SQWW_51 = Square.SQ_51 | (1 << 5) | (4 << 6) | (0 << 10) | (4 << 14) | (0 << 18),
        SQWW_52 = Square.SQ_52 | (1 << 5) | (4 << 6) | (1 << 10) | (3 << 14) | (0 << 18),
        SQWW_53 = Square.SQ_53 | (1 << 5) | (4 << 6) | (2 << 10) | (2 << 14) | (0 << 18),
        SQWW_54 = Square.SQ_54 | (1 << 5) | (4 << 6) | (3 << 10) | (1 << 14) | (0 << 18),
        SQWW_55 = Square.SQ_55 | (1 << 5) | (4 << 6) | (4 << 10) | (0 << 14) | (0 << 18),
    
        SQWW_BORROW_MASK = (1 << 9) | (1 << 13) | (1 << 17) | (1 << 21),
    }

    public static class SquareWithWallExtensions
    {
        public static Int32 ToInt(this SquareWithWall sqww) { return (Int32)sqww; }
        public static Square ToSquare(this SquareWithWall sqww) { return (Square)(sqww.ToInt() & 0x1f); }
        public static SquareWithWall ToSqww(this Square sq) { return SquareToSqww[(int)sq]; }
        public static string Pretty(this SquareWithWall sqww) { return sqww.ToSquare().Pretty(); }

        public static SquareWithWall[] SquareToSqww = new SquareWithWall[(int)Square.NB]
        {
            SquareWithWall.SQWW_11, SquareWithWall.SQWW_12, SquareWithWall.SQWW_13, SquareWithWall.SQWW_14, SquareWithWall.SQWW_15,
            SquareWithWall.SQWW_21, SquareWithWall.SQWW_22, SquareWithWall.SQWW_23, SquareWithWall.SQWW_24, SquareWithWall.SQWW_25,
            SquareWithWall.SQWW_31, SquareWithWall.SQWW_32, SquareWithWall.SQWW_33, SquareWithWall.SQWW_34, SquareWithWall.SQWW_35,
            SquareWithWall.SQWW_41, SquareWithWall.SQWW_42, SquareWithWall.SQWW_43, SquareWithWall.SQWW_44, SquareWithWall.SQWW_45,
            SquareWithWall.SQWW_51, SquareWithWall.SQWW_52, SquareWithWall.SQWW_53, SquareWithWall.SQWW_54, SquareWithWall.SQWW_55,
        };

        /// <summary>
        /// 盤内(True)，盤外(False)
        /// </summary>
        /// <param name="sqww"></param>
        /// <returns></returns>
        public static bool IsOk(this SquareWithWall sqww) { return (sqww & SquareWithWall.SQWW_BORROW_MASK) == 0; }
    }

    // 0..4(to), 5..9(from or pc), 10(drop), 11(promote)
    public enum Move
    {
        NONE = 0,
        PROMOTE = 1 << 10,
        DROP = 1 << 11,
        MAX_MOVES = 256,

        SPECIAL = DROP + PROMOTE,
        NULL,
        RESIGN,
        MATED,
        REPETITION_WIN,
        REPETITION_LOSE,
        TIME_UP,
        INTERRUPT,
    }

    /// <summary>
    /// special moveの指し手が勝ち・負け・引き分けのいずれに属するかを判定する時の結果
    /// </summary>
    public enum MoveGameResult
    {
        WIN,  // 勝ち
        LOSE, // 負け
        DRAW, // 引き分け
        UNKNOWN,   // 分類不可のもの

        // ---

        INTERRUPT, // LocalGameServerのイベントで使う用。(GameResult()などではこれを返さない)
    }

    public static class MoveExtensions
    {
        public static bool IsOk(this Move m) { return !(m == Move.NONE || m.IsSpecial()); }
        public static bool IsSpecial(this Move m) { return m >= Move.SPECIAL; }
        public static Int32 ToInt(this Move m) { return (Int32)m; }
        public static Square To(this Move m) { return (Square)(m.ToInt() & 0x1f); }
        public static Square From(this Move m) { return (Square)((m.ToInt() >> 5) & 0x1f); }
        public static Piece DroppedPiece(this Move m) { return (Piece)((m.ToInt() >> 5) & 0x1f); }

        public static bool IsDrop(this Move m) { return (m & Move.DROP) != 0; }
        public static bool IsPromote(this Move m) { return (m & Move.PROMOTE) != 0; }
        public static string Pretty(this Move m)
        {
            var sb = new StringBuilder();
            if (m.IsDrop())
            {
                sb.Append(m.To().Pretty() + m.DroppedPiece().Pretty() + "打");
            }
            else
            {
                sb.Append(m.From().Pretty() + m.To().Pretty());
                if (m.IsPromote())
                    sb.Append("成");
            }
            return sb.ToString();
        }

        /// <summary>
        /// mが、勝ち・負け・引き分けのいずれに属するかを返す。
        /// mは specail moveでなければならない。
        /// 
        /// 連続自己対局の時に結果の勝敗を判定する時などに用いる。
        /// m == INTERRUPTでもMoveGameResult.INTERRUPTではなくUNKNOWNが返るので注意。
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static MoveGameResult GameResult(this Move m)
        {
            Debug.Assert(m.IsSpecial());

            switch (m)
            {
                case Move.REPETITION_WIN:
                    return MoveGameResult.WIN;

                case Move.RESIGN:
                case Move.MATED:
                case Move.REPETITION_LOSE:
                case Move.TIME_UP:
                    return MoveGameResult.LOSE;

                case Move.NULL:       // これもないと思うが..
                case Move.INTERRUPT:  // 中断も決着がついていないので不明扱い。
                    return MoveGameResult.UNKNOWN;

                default:
                    return MoveGameResult.UNKNOWN;
            }
        }
    }

    /// <summary>
    /// USI形式
    /// </summary>
    public static class USIExtensions
    {
        public static readonly string PieceToCharBW = " PLNSBRGK        plnsbrgk";
        public static string USI(File f) { return (f.ToInt() + 1).ToString(); }
        public static string USI(Rank r) { return ((char)(r.ToInt() + 'a')).ToString(); }
        public static string USI(Square sq) { return USI(sq.ToFile()) + USI(sq.ToRank()); }
        public static string USI(Piece pc) { return PieceToCharBW[(int)pc].ToString(); }
        public static string USI(this Move m)
        {
            if (m.IsSpecial())
                return SpecialMoveUSI(m);
            else if (m.IsDrop())
                return USI(m.DroppedPiece()) + '*' + USI(m.To());
            else if (m.IsPromote())
                return USI(m.From()) + USI(m.To()) + '+';
            else
                return USI(m.From()) + USI(m.To());
        }

        public static string SpecialMoveUSI(Move m)
        {
            switch (m)
            {
                case Move.RESIGN:
                    return "resign";
                default:
                    return "";
            }
        }
    }

    public enum Hand
    {
        ZERO = 0,
        MAX = 0b10101010000010, // 歩・銀・角・飛・金 ｘ２枚
    }

    public static class HandExtensions
    {
        private static readonly int[] PieceBits = { 0, 0, 0, 0, 6, 8, 10, 12 };
        public static Int32 ToInt(this Hand h) { return (Int32)h; }
        public static void Add(this ref Hand h, Piece pr, int cnt = 1) { h += cnt * (1 << PieceBits[(int)pr]); }
        public static void Sub(this ref Hand h, Piece pr, int cnt = 1) { h -= cnt * (1 << PieceBits[(int)pr]); }
        public static int Count(this Hand h, Piece pr) { return (h.ToInt() >> PieceBits[(int)pr]) & 0b11; }
        public static bool Exist(this Hand h, Piece pr) { return h.Count(pr) != 0; }
        public static string Pretty(this Hand h)
        {
            var sb = new StringBuilder();
            foreach (var pr in HandPiece)
            {
                int cnt = h.Count(pr);
                if (cnt > 0)
                {
                    sb.Append(pr.Pretty().Substring(1));
                    if (cnt > 1)
                        sb.Append(cnt.ToString());
                }
            }
            return sb.ToString();
        }
        
        internal static readonly Piece[] HandPiece = { Piece.PAWN, Piece.SILVER, Piece.GOLD, Piece.BISHOP, Piece.ROOK };
    }

    // 駒台まで含めた升
    public enum SquareHand
    {
        SquareZero = 0,
        SquareNB = 25,

        Hand = 25,
        HandBlack = 25,
        HandWhite = 25 + 5,
        HandNB = 25 + 10,

        Zero = 0, NB = HandNB,
    }

    public static class SquareHandExtensions
    {
        public static Int32 ToInt(this SquareHand sq) { return (Int32)sq; }
        public static bool IsOk(this SquareHand sq) { return SquareHand.Zero <= sq && sq <= SquareHand.NB; }
        
        /// <summary>
        /// sqに対してどちらのColorの手駒を表現しているのかを返す。
        /// 盤上、駒箱の升に対して呼び出してはならない。
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static Color PieceColor(this SquareHand sq)
        {
            Debug.Assert(IsHandPiece(sq));
            return sq < SquareHand.HandWhite ? Color.BLACK : Color.WHITE;
        }
        
        public static bool IsBoardPiece(this SquareHand sq) { return SquareHand.SquareZero <= sq && sq < SquareHand.SquareNB; }
        public static bool IsHandPiece(this SquareHand sq) { return SquareHand.Hand <= sq && sq < SquareHand.HandNB; }

        /// <summary>
        /// sqの手駒に対して、その駒種を返す
        /// sqは手駒か駒箱の駒でないといけない。
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static Piece ToPiece(this SquareHand sq)
        {
            Debug.Assert(!IsBoardPiece(sq));

            return (sq < SquareHand.HandWhite)
                ? HandExtensions.HandPiece[sq - SquareHand.HandBlack]
                : HandExtensions.HandPiece[sq - SquareHand.HandWhite];
        }
    }

    public enum RepetitionState : Int32
    {
        NONE, // 千日手ではない
        // DRAW, // 千日手(未使用)
        WIN,  // (連続王手の)千日手を相手が行った(ので手番側の勝ちの局面)
        LOSE, // (連続王手の)千日手を自分が行った(ので手番側の負けの局面)
    };

    /// <summary>
    /// 駒番号
    /// 盤上のどの駒がどこに移動したかを追跡するために用いる
    /// 1 ～ 12までの番号がついている。
    /// </summary>
    public enum PieceNo : Int32
    {
        // 駒がない場合
        NONE = 0,

        ZERO = 1, // これややこしいかな…。
        NB = 13,

        // 歩の枚数の最大
        PAWN_MAX = 18,
    }

    public static partial class Util
    {
        public static Square MakeSquare(File f, Rank r) { return (Square)(f.ToInt() * 5 + r.ToInt()); }
        public static Square MakeSquare(char f, char r) { return MakeSquare((File)(f - '1'), (Rank)(r - 'a')); }
        public static Piece MakePiece(Color c, Piece pr) { return c == Color.BLACK ? pr : (pr.ToInt() + Piece.WHITE); }
        public static Piece MakePiece(char pc) { return (Piece)USIExtensions.PieceToCharBW.IndexOf(pc); }
        public static Move MakeMove(Square from, Square to) { return (Move)(to.ToInt() + (from.ToInt() << 5)); }
        public static Move MakeMovePromote(Square from, Square to) { return to.ToInt() + (from.ToInt() << 5) + Move.PROMOTE; }
        public static Move MakeMoveDrop(Piece pr, Square to) { return to.ToInt() + (pr.ToInt() << 5) + Move.DROP; }
        public static Move MakeMove(SquareHand from, SquareHand to, bool promote)
        {
            if (!to.IsBoardPiece())
                return Move.NONE;

            var to_ = (Square)to;

            if (from.IsHandPiece())
            {
                Piece pr = from.ToPiece();
                return MakeMoveDrop(pr, to_);
            }
            else if (from.IsBoardPiece())
            {
                var from_ = (Square)from;
                if (promote)
                    return MakeMovePromote(from_, to_);
                else
                    return MakeMove(from_, to_);
            }
            
            return Move.NONE;
        }
        
        public static bool CanPromote(Color c, Rank r) { return r == (c == Color.BLACK ? Rank.RANK_1 : Rank.RANK_5); }
        public static bool CanPromote(Color c, Square sq) { return CanPromote(c, sq.ToRank()); }
        public static SquareHand MakeSquareHand(Color c, Piece pr)
        {
            if (c == Color.BLACK)
                return SquareHand.HandBlack + HandSquare[(int)pr];
            else
                return SquareHand.HandWhite + HandSquare[(int)pr];
        }

        private static readonly int[] HandSquare = { 0, 0, 0, 0, 1, 3, 4, 2 };
    }
};