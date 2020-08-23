using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Assets.Scripts.Shogi;

namespace Assets.Scripts.Data
{
    public class KifuManager : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        public static void Save(List<Move> moveList)
        {
#if false
            var sb = new StringBuilder();
            foreach (var move in moveList)
            {
                sb.AppendLine(USIExtensions.USI(move));
            }
#endif

            string path = "test.kif";

            using (StreamWriter sw = System.IO.File.CreateText(path))
            {
                sw.WriteLine("手合割：５五将棋");
                sw.WriteLine("手数----指手-------- - 消費時間--");
            }
        }
    }
}