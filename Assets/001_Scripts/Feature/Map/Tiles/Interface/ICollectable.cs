using School.PositionSync;
namespace _001_Scripts.Map.Tiles.Interface
{
    public interface ICollectable
    {
        // 로컬 플레이어만 호출하고 true일 때 한 번만 보상을 적용합니다.
        bool TryCollect(out BlockContent content);
    }
}
