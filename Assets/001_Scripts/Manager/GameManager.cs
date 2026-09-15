using System;
using _001_Scripts.Manager.Base;
using School.PositionSync;
using UnityEngine;

namespace _001_Scripts.Manager
{
    public class GameManager : SinManagerBase<GameManager>
    {
        private float gTime = 0;
        private int Coin = 0;
        private int Score = 0;
        private bool isArrived = false;

        [SerializeField] private Transform spawnPoint;

        public String GetTime() => Mathf.RoundToInt(gTime).ToString();
        public int GetCoin() => Coin;
        public int GetScore() => Score;
        public Transform GetSpawnPoint() => spawnPoint;
        public bool GetIsArrived() => isArrived;

        private void Update()
        {
            gTime += Time.deltaTime;
        }
        
        public void StopGame()
        {

        }

        public void StartGame()
        {

        }

        public void Arrive()
        {
            isArrived = true;
            StopGame();
        }
    }
}