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

        [SerializeField] private Transform spawnPoint;
        
        public String GetTime() => gTime.ToString();
        public int GetCoin() => Coin;
        public int GetScore() => Score;

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
    }
}