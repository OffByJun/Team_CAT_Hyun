using _001_Scripts.Manager.Base;

namespace _001_Scripts.Manager
{
    public class UIManager : SinManagerBase<UIManager>
    {
        public int GetCoin() => ScoreManager.instance.GetCoin();
        public int GetScore() => ScoreManager.instance.GetScore();
        public string GetTime() => ScoreManager.instance.GetTime();
        public bool GetIsArrived() => GameManager.instance.GetIsArrived();
    }
}
