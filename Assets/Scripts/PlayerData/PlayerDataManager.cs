using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.PlayerData {
    public class PlayerDataManager : MonoBehaviour {

        public PlayerData PlayerData { get; private set; }

        Queue<ValueTuple<double, double, bool>> TaskQueue;

        void Awake() {
            PlayerData = new PlayerData();
            TaskQueue = new Queue<ValueTuple<double, double, bool>>();
        }

        public void SetPlayerData(PlayerData playerData) {
            PlayerData = playerData;
            
            // 勝敗結果を反映する
            CompleteTask();
        }

        public void EntryTask(ValueTuple<double, double, bool> value) {
            TaskQueue.Enqueue(value);
        }

        void CompleteTask() {
            Debug.Assert(TaskQueue.Count <= 1);
            while (TaskQueue.Count > 0) {
                var (a, b, isWin) = TaskQueue.Dequeue();
                PlayerData.Update(Misc.EloRating.Update(a, b, isWin), isWin);
            }
        }

    }
}
