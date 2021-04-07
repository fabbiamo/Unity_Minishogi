using System.Text;

namespace Assets.Scripts.Shogi
{
    public static class Kif
    {
        public static string ToKif(Move m, Color c, Piece moved_pc)
        {
            StringBuilder st = new StringBuilder();
            st.Append(BlackWhiteSymbol[(int)c]);

            if (m.IsSpecial())
            {
                st.Append(SpecialMoveToKif(m));
            }
            else if (m.IsDrop())
            {
                st.Append(m.To().Pretty() + m.DroppedPiece().Pretty().Substring(1) + "打");
            }
            else
            {
                var pt = moved_pc.Type();

                st.Append(m.To().Pretty() + pt.Type().Pretty().Substring(1));
                if (m.IsPromote())
                    st.Append("成");
            }
            return st.ToString();
        }

        public static string SpecialMoveToKif(Move m)
        {
            switch (m)
            {
                case Move.RESIGN:
                    return "投了";
                case Move.MATED:
                    return "詰み";
                case Move.TIME_UP:
                    return "時間切れ";
                case Move.INTERRUPT:
                    return "中断";

#if false
                case Move.REPETITION_WIN:
                case Move.REPETITION_LOSE:
                case Move.NULL:       // これもないと思うが..
#endif
                default:
                    return "終了";
            }
        }

        private static readonly string[] BlackWhiteSymbol = { "☗", "☖" };
    }
}
