using School.PositionSync;

namespace _001_Scripts.Map.Tiles.Interface
{
    public interface IBreakable
    {
        // 로컬 플레이어가 아래에서 머리로 쳤을 때만 호출합니다.
        // true는 요청 전달 성공이며 서버 승인/보상 소유권 확정이 아닙니다.
        bool TryHit(out BlockContent content);
    }
}
