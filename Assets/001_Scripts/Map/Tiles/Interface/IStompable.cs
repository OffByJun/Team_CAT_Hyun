namespace _001_Scripts.Map.Tiles.Interface
{
    public interface IStompable
    {
        int MonsterId { get; }
        // 내려오며 위에서 밟았는지는 플레이어가 판정하고, 성공 시에만 반동을 적용합니다.
        bool TryStomp();
    }
}
