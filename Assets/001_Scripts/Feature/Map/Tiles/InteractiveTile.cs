using _001_Scripts.Map.Tiles.Base;
using _001_Scripts.Map.Tiles.Interface;
using School.PositionSync;

namespace _001_Scripts.Map.Tiles
{
    public sealed class InteractiveTile : BaseTile, IBreakable, ICollectable
    {
        private MapManager owner;
        public GridCell Cell { get; private set; }

        public void Initialize(MapManager map, GridCell cell)
        {
            owner = map;
            Cell = cell;
        }

        public bool TryHit(out BlockContent content)
        {
            content = BlockContent.None;
            return isActiveAndEnabled && owner != null && owner.TryHitBlock(Cell, out content);
        }

        public bool TryCollect(out BlockContent content)
        {
            content = BlockContent.None;
            if (!isActiveAndEnabled || owner == null || !owner.TryCollectCoin(Cell, out content))
                return false;
            gameObject.SetActive(false);
            return true;
        }
    }
}
