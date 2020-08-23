using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System;
using TMPro;

namespace Assets.Scripts.Game
{
    public class Timer : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI countdown = default;
        [SerializeField] double maxSeconds = 20f;

        private bool isTimerRunning;
        double startTime, seconds, oldSeconds;

        void Start()
        {
            //isTimerRunning = false;
            //countdown.text = null;
        }

        void Update()
        {
            if (!isTimerRunning)
                return;

            seconds = Math.Ceiling(maxSeconds - (PhotonNetwork.Time - startTime));

            //　タイマー表示用UIテキストに時間を表示する
            if ((int)seconds != (int)oldSeconds)
            {
                countdown.text = ((int)seconds).ToString();
            }

            oldSeconds = seconds;

            if (seconds <= 0)
                isTimerRunning = false;
        }

        public void SetTimer()
        {
            isTimerRunning = true;
            startTime = PhotonNetwork.Time;
            seconds = oldSeconds = maxSeconds;
            countdown.text = ((int)seconds).ToString();
        }

        public void ResetTimer()
        {
            isTimerRunning = false;
            countdown.text = null;
        }
    }
}
