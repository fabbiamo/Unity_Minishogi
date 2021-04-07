using Assets.Scripts.Shogi;
using UnityEngine;

namespace Assets.Scripts.Misc
{
    public class SpriteManager
    {
        private static Sprite[] sprites = new Sprite[16];

        public static void Load()
        {
            sprites = Resources.LoadAll<Sprite>("koma_v1");
        }

        public static Sprite GetSprite(Piece pt)
        {
            return System.Array.Find(sprites, (sprite) => sprite.name.Equals(FileName[(int)pt]));
        }

        public static readonly string[] FileName =
        {
            ""       , "piece_0" , "piece_1" , "piece_2" , "piece_3" , "piece_4" , "piece_5" , "piece_6",
            "piece_7", "piece_28", "piece_29", "piece_30", "piece_31", "piece_32", "piece_33", ""       ,
        };
    }

}
