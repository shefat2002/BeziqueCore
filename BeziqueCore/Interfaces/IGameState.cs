using BeziqueCore.Models;

namespace BeziqueCore.Interfaces
{
    public interface IGameState
    {
        List<Player> Players { get; }
        Player CurrentPlayer { get; set; }
        Player Winner { get; set; }
        Player LastTrickWinner { get; set; }
        Suit TrumpSuit { get; set; }
        Card TrumpCard { get; set; }
        IPlayerTimer CurrentPlayerTimer { get; }
        Dictionary<Player, int> RoundScores { get; }

        // Trick tracking
        Dictionary<Player, Card> CurrentTrick { get; set; }
        Suit? LeadSuit { get; set; }

        // Game mode
        GameMode Mode { get; set; }

        void Reset();
        void AddPlayer(Player player);
        void StartNewTrick();
        void AddCardToTrick(Player player, Card card);
        bool IsTrickComplete();
    }
}
