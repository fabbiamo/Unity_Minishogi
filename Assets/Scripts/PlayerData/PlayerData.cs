namespace Assets.Scripts.PlayerData {
    public class PlayerData {

        public double Rating;
        public int Game;
        public int Win;
        public int Lose;

        public PlayerData () {
            Rating = 1500;
            Game = 0;
            Win = 0;
            Lose = 0;
        }

        public void Update(double diff, bool isWin) {
            ++Game;
            if (isWin) {
                ++Win;
                Rating += diff;
            }
            else {
                ++Lose;
                Rating -= diff;
            }
        }

    }
}
