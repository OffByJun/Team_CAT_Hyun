using School.PositionSync;
using TMPro;
using UnityEngine;

namespace _001_Scripts.UI.Component
{
    public class World : GameBehaviour
    {
        [SerializeField] private TextMeshProUGUI worldTxt;
        
        public void UpdateInfo(string mapId)
        {
            worldTxt.text = mapId == null? mapId : " ";
        }
    }
}