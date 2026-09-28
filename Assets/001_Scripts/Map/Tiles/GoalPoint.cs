using _001_Scripts.Manager;
using _001_Scripts.Player.Interface;
using UnityEngine;

namespace _001_Scripts.Map.Tiles
{
    public sealed class GoalPoint : GameBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            IPlayer player = other.GetComponent<IPlayer>();
            if (player == null) return;

            GameManager.instance.Arrive();
        }
    }
}
