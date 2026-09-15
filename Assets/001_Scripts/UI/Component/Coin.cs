using _001_Scripts.Manager;
using TMPro;
using UnityEngine;

namespace _001_Scripts.UI.Component
{
    public class Coin : GameBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;

        public void UpdateInfo(int coin)
        {
            text.text = $"Coin: {coin.ToString()}";
        }
    }
}