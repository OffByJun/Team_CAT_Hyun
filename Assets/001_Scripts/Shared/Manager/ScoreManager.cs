using _001_Scripts.Manager.Base;
using UnityEngine;

namespace _001_Scripts.Manager
{
    public class ScoreManager : SinManagerBase<ScoreManager>
    {
        private float gTime = 0;
        private int Coin = 0;
        private int Score = 0;

        public string GetTime() => Mathf.RoundToInt(gTime).ToString();
        public int GetCoin() => Coin;
        public int GetScore() => Score;

        public void AddCoin(int amount = 1)
            => Coin += amount;

        public void AddScore(int amount)
            => Score += amount;

        private void Update()
        {
            gTime += Time.deltaTime;
        }
    }
}
