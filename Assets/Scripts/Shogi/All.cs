using System.Collections.Generic;

namespace Assets.Scripts.Shogi
{
    public class All
    {
        public static IEnumerable<Color> Colors()
        {
            for (var c = Color.ZERO; c < Color.NB; ++c)
                yield return c;
        }

        public static IEnumerable<int> IntColors()
        {
            for (var c = Color.ZERO; c < Color.NB; ++c)
                yield return (int)c;
        }

        public static IEnumerable<Square> Squares()
        {
            for (var sq = Square.ZERO; sq < Square.NB; ++sq)
                yield return sq;
        }

        public static IEnumerable<File> files()
        {
            for (var f = File.ZERO; f < File.NB; ++f)
                yield return f;
        }
    }
}
