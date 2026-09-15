using _001_Scripts.Manager.Base;
using School.PositionSync;
using UnityEngine;

namespace _001_Scripts.Manager
{
    public class GameManager : SinManagerBase<GameManager>
    {
        private Time gTime = new Time();
        private int Coin = 0;
        private int Score = 0;
        
        public Time GetTime() => gTime;
        public int GetCoin() => Coin;
        public int GetScore() => Score;
        
        public void StopGame()
        {
            
        }

        public void StartGame()
        {
            
        }
    }
}