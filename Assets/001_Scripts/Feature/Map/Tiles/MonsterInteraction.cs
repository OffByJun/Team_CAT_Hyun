using _001_Scripts.Map.Tiles.Interface;
using UnityEngine;

namespace _001_Scripts.Map.Tiles
{
    public sealed class MonsterInteraction : MonoBehaviour, IStompable
    {
        private MapManager owner;
        public int MonsterId { get; private set; }

        public void Initialize(MapManager map, int id)
        {
            owner = map;
            MonsterId = id;
        }

        public bool TryStomp() => isActiveAndEnabled && owner != null && owner.TryStompMonster(MonsterId);
    }
}
