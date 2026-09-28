using _001_Scripts.Manager;
using _001_Scripts.Player.Interface;
using UnityEngine;

public sealed class GoalPoint : GameBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        IPlayer player = other.GetComponent<IPlayer>();
        if (player == null) return;

        GameManager.instance.Arrive();
    }
}
