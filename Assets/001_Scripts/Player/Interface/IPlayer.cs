using _001_Scripts.Player.Type;

namespace _001_Scripts.Player.Interface
{
    public interface IPlayer
    {
        PlayerState PlayerState { get; }
        
        void TryDmg(float dmg);
        int[][] GetPos();
    }
}