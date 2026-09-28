using _001_Scripts.Manager;
using _001_Scripts.UI.Component;
using _001_Scripts.UI.Panels.Base;
using UnityEngine;
using Time = _001_Scripts.UI.Component.Time;

namespace _001_Scripts.UI.Panels
{
    public sealed class HUDPanel : PanelBase
    {
        [SerializeField] private Coin coin;
        [SerializeField] private Score score;
        [SerializeField] private Time time;
        [SerializeField] private World world;
        [SerializeField] private Goal goal;

        private void Update()
        {
            coin.UpdateInfo(UIManager.instance.GetCoin());
            score.UpdateInfo(UIManager.instance.GetScore());
            time.UpdateInfo(UIManager.instance.GetTime());
            // world.UpdateInfo(MapManager.instance.GetMapId();
            goal.UpdateInfo(UIManager.instance.GetIsArrived());
        }
    }
}