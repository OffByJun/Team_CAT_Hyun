using _001_Scripts.Player.Type;
using UnityEngine;

namespace _001_Scripts.Player.Interface
{
    public interface IPlayer
    {
        PlayerState PlayerState { get; }
        
        void TryDmg(float dmg);
        
        Vector2 GetPos();
    }
}