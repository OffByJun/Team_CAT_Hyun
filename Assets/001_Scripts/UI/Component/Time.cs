using _001_Scripts.Manager;
using TMPro;
using UnityEngine;

namespace _001_Scripts.UI.Component
{
    public class Time : GameBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        
        public void UpdateInfo(UnityEngine.Time time)
        {
            text.text = time.ToString();
        }
    }
}