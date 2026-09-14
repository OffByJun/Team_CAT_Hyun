using _001_Scripts.Player.Type;
using UnityEngine;

namespace _001_Scripts.Player.Interface
{
    public interface IPlayer
    {
        PlayerState PlayerState { get; }
        MoveState MoveState { get; }
        
        void TakeDmg(float dmg);
        
        Vector2 GetVector2();
    }
}