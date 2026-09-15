using _001_Scripts.Manager;
using TMPro;
using UnityEngine;

namespace _001_Scripts.UI.Component
{
    public class Goal : GameBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;

        public void UpdateInfo(bool isArrived)
        {
            text.text = isArrived ? "Goal!" : "";
        }
    }
}
