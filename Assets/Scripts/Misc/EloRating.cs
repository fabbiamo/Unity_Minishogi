using System;

namespace Assets.Scripts.Misc {
    public static class EloRating {
        public static double Update(double ratingA, double ratingB, bool isWin) {
            // 定数
            double k = 32f;

            // Bの期待勝率
            double winRate = 1 / (Math.Pow(10, (ratingA - ratingB) / 400) + 1);

            return isWin
                ? k * winRate           // Aが勝ったとき
                : k * (1.00 - winRate); // Bが勝ったとき
        }
    }
}
