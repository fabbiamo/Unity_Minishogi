using Assets.Scripts.Shogi;

namespace Assets.Scripts.GameServer {
    enum MatchingState {
        Start,
        Playing,
        Pause,
        Fin,
    };

    public enum GameResult {
        None,
        BlackWin,
        WhiteWin,
        Draw,
    };

    public class BoardManager {
        /// <summary>
        /// 局面情報
        /// </summary>
        public Position Position { get; private set; } = new Position();

        public static void Init() {
            Zobrist.Init();
            Bitboard.Init();
        }

        public void DoMove(Move m) {
            Position.DoMove(m);
        }

        public void UnDoMove() {
            Position.UndoMove();
        }

        /// <summary>
        /// PickedFromが適切であるか
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public bool IsOkPickedFrom(SquareHand from) {
            Piece pc = Position.PieceOn(from);
            if (pc == Piece.NO_PIECE)
                return false;

            return pc.PieceColor() == Position.sideToMove;
        }

        /// <summary>
        /// PickedToが適切であるか
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public bool IsOkPickedTo(SquareHand to) {
            if (!to.IsBoardPiece())
                return false;

            Piece pc = Position.PieceOn(to);
            if (pc == Piece.NO_PIECE)
                return true;

            return pc.PieceColor() == Position.sideToMove.Not();
        }

        public GameResult IsEndGame() {
            Move[] moves = new Move[(int)Move.MAX_MOVES];
            if (MoveGen.LegalAll(Position, moves, 0) == 0) {
                if (Position.sideToMove == Color.BLACK)
                    return GameResult.WhiteWin;
                else
                    return GameResult.BlackWin;
            }

            switch (Position.IsRepetition()) {
            case RepetitionState.WIN: {
                    if (Position.sideToMove == Color.BLACK)
                        return GameResult.BlackWin;
                    else
                        return GameResult.WhiteWin;
                }

            case RepetitionState.LOSE: {
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