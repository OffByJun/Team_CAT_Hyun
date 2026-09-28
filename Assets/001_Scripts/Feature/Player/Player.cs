using School.PositionSync;
using UnityEngine;

namespace _001_Scripts.Player
{
    public sealed class Player : GameBehaviour
    {
        public void SetPos(Info pos)
        {
            transform.position = new Vector2(pos.X, pos.Y);
        }

        public Vector2 GetPos(Vector2 pos)
        {
            return transform.position;
        }
    }
}