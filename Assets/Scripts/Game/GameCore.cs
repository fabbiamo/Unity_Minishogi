using Assets.Scripts.Shogi;

namespace Assets.Scripts.Game
{
    enum MatchingState
    {
        Start,
        Playing,
        Pause,
        Fin,
    };

    public enum GameResult
    {
        None,
        BlackWin,
        WhiteWin,
        Draw,
    };

    public class GameCore
    {
        /// <summary>
        /// 局面情報
        /// </summary>
        public static Position Position { get; private set; } = new Position();

        public static void Init()
        {
            // ここの呼び出しは1回だけでいい
            Zobrist.Init();
            Bitboard.Init();

            // おまけで初期局面にしておく
            Position.SetHirate();
        }

        public static void UserTest()
        {
            Move m1 = Util.MakeMove(Square.SQ_25, Square.SQ_14);
            Move m2 = Util.MakeMove(Square.SQ_41, Square.SQ_14);
            Move m3 = Util.MakeMove(Square.SQ_15, Square.SQ_14);
            
            Position.DoMove(m1);
            Position.DoMove(m2);
            UnityEngine.Debug.LogFormat("1: {0}",Position.State().key.Pretty());


            Position.DoMove(m3);

            Position.UndoMove();
            UnityEngine.Debug.LogFormat("2: {0}", Position.State().key.Pretty());
        }

        public static void DoMove(Move m)
        {
            Position.DoMove(m);
        }

        public static void UnDoMove()
        {
            Position.UndoMove();
        }

        /// <summary>
        /// Picked_fromが適切であるか
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public static bool IsOk_PickedFrom(SquareHand from)
        {
            Piece pc = Position.PieceOn(from);
            if (pc == Piece.NO_PIECE)
                return false;

            return pc.PieceColor() == Position.sideToMove;
        }

        /// <summary>
        /// Picked_toが適切であるか
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public static bool IsOk_PickedTo(SquareHand to)
        {
            if (!to.IsBoardPiece())
                return false;

            Piece pc = Position.PieceOn(to);
            if (pc == Piece.NO_PIECE)
                return true;

            return pc.PieceColor() == Position.sideToMove.Not();
        }

        public static GameResult IsEndGame()
        {
            Move[] moves = new Move[(int)Move.MAX_MOVES];
            if (MoveGen.LegalAll(Position, moves, 0) == 0)
            {
                if (Position.sideToMove == Color.BLACK)
                    return GameResult.WhiteWin;
                else
                    return GameResult.BlackWin;
            }

            switch(Position.IsRepetition())
            {
                case RepetitionState.WIN:
                    {
                        if (Position.sideToMove == Color.BLACK)
                            return GameResult.BlackWin;
                        else
                            return GameResult.WhiteWin;
                    }

                case RepetitionState.LOSE:
                    {
                        if (Position.sideToMove == Color.BLACK)
                            return GameResult.WhiteWin;
                        else
                            return GameResult.BlackWin;
                    }

                case RepetitionState.NONE:
                default:
                    return GameResult.None;
            }
        }
    }
}