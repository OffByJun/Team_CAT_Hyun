using System;
using _001_Scripts.Manager;
using TMPro;
using UnityEngine;

namespace _001_Scripts.UI.Component
{
    public class Time : GameBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        
        public void UpdateInfo(String time)
        {
            text.text = $"Time: {time}";
        }
    }
}