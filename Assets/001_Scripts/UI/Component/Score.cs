using System;
using _001_Scripts.Manager;
using TMPro;
using UnityEngine;

namespace _001_Scripts.UI.Component
{
    public class Score : GameBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;

        public void UpdateInfo(int score)
        {
            text.text = score.ToString();
        }
    }
}