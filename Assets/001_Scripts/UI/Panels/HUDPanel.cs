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
        
        private void Update()
        {
            coin.UpdateInfo(GameManager.instance.GetCoin());
            score.UpdateInfo(GameManager.instance.GetScore());
            time.UpdateInfo(GameManager.instance.GetTime());
            // world.UpdateInfo(MapManager.instance.GetMapId();
        }
    }
}