using BeziqueCore.Models;

namespace BeziqueCore.Interfaces
{
    public interface IGameState
    {
        List<Player> Players { get; }
        Player CurrentPlayer { get; set; }
        Player Winner { get; set; }
        Suit TrumpSuit { get; set; }
        Card TrumpCard { get; set; }
        IPlayerTimer CurrentPlayerTimer { get; }
        Dictionary<Player, int> RoundScores { get; }
        void Reset();
    }
}
