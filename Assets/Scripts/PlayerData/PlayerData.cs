using System.Collections.Generic;
using PlayFab.ClientModels;

namespace Assets.Scripts.PlayerData
{
    public class PlayerData
    {
        public string Name { get; private set; }
        public double Rating { get; private set; }
        public int Game { get; private set; }
        public int Win { get; private set; }
        public int Lose { get; private set; }

        public void SetData(Dictionary<string, UserDataRecord> data)
        {
            Name = data.ContainsKey("name") ? data["name"].Value : "anonymus";
            Game = data.ContainsKey("game") ? int.Parse(data["game"].Value) : 0;
            Win = data.ContainsKey("win") ? int.Parse(data["win"].Value) : 0;
            Lose = data.ContainsKey("lose") ? int.Parse(data["lose"].Value) : 0;
            Rating = data.ContainsKey("rating") ? double.Parse(data["rating"].Value) : 1500;
        }

        public void SetName(string name)
        {
            Name = name;
        }

        public void UpdateData(double diff, bool isWin)
        {
            ++Game;
            if (isWin)
            {
                ++Win;
                Rating += diff;
            }
            else
            {
                ++Lose;
                Rating -= diff;
            }
        }
    }
}
