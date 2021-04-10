using Assets.Scripts.Shogi;
using UnityEngine;

namespace Assets.Scripts.Misc {
    public class SpriteManager {
        private static Sprite[] Sprites = new Sprite[16];

        public static void Load() {
            Sprites = Resources.LoadAll<Sprite>("image/shogi");
        }

        public static Sprite GetSprite(Piece pt) {
            return System.Array.Find(Sprites, (sprite) => sprite.name.Equals(FileName[(int)pt]));
        }

        public static readonly string[] FileName =
        {
            ""       , "piece_0" , "piece_1" , "piece_2" , "piece_3" , "piece_4" , "piece_5" , "piece_6",
            "piece_7", "piece_28", "piece_29", "piece_30", "piece_31", "piece_32", "piece_33", ""       ,
        };
    }
}
