using System;

namespace Assets.Scripts.Shogi
{
    /// <summary>
    /// BitOperation一式
    /// </summary>
    public static class BitOp
    {
        /// <summary>
        /// 2進数で見て1になっている一番下位のbit位置を返し、そのbitを0にする。
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public static int LSB32(ref UInt32 n)
        {
            // cf. Bit Twiddling Hacks : http://graphics.stanford.edu/~seander/bithacks.html

            // Count the consecutive zero bits (trailing) on the right with multiply and lookup
            UInt32 v = (UInt32)n;
            int r = MultiplyDeBruijnBitPosition[((UInt32)((v & -v) * 0x077CB531U)) >> 27];
            n ^= (1U << r);
            return r;
        }

        private static readonly int[] MultiplyDeBruijnBitPosition =
        {
          0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8,
          31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9
        };

        /// <summary>
        /// for non-AVX2 : software emulationによるpext実装(やや遅い。とりあえず動くというだけ。)
        /// </summary>
        /// <param name="val"></param>
        /// <param name="mask"></param>
        /// <returns></returns>
        public static UInt32 PEXT32(UInt32 val , UInt32 mask)
        {
            UInt32 res = 0;
            for (UInt32 bb = 1; mask != 0 ; bb += bb)
            {
                if (((Int32)val & (Int32)mask & -(Int32)mask) != 0)
                    res |= bb;
                // マスクを1bitずつ剥がしていく実装なので処理時間がbit長に依存しない。
                // ゆえに、32bit用のpextを別途用意する必要がない。
                mask &= mask - 1;
            }
            return res;
        }
    }

    /// <summary>
    /// BitOpに関するextension methods
    /// </summary>
    public static class BitOpExtensions
    {
        /// <summary>
        /// 2進数として見たときに1になっているbitの数を数える。
        /// ソフトウェア実装なのでそこそこ遅い。
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static int PopCount(this UInt32 n)
        {
            UInt32 n0 = (UInt32)n;

            // cf. Checking CPU Popcount from C# : https://stackoverflow.com/questions/6097635/checking-cpu-popcount-from-c-sharp?lq=1
            ulong result0 = n0 - ((n0 >> 1) & 0x5555555555555555UL);
            result0 = (result0 & 0x3333333333333333UL) + ((result0 >> 2) & 0x3333333333333333UL);
            var r0 = (int)(unchecked(((result0 + (result0 >> 4)) & 0xF0F0F0F0F0F0F0FUL) * 0x101010101010101UL) >> 56);
            return r0;
        }
    }

}
