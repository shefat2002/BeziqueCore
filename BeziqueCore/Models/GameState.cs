using BeziqueCore.Interfaces;

namespace BeziqueCore.Models
{
    public class GameState : IGameState
    {
        public List<Player> Players { get; private set; }
        public Player CurrentPlayer { get; set; }
        public Player Winner { get; private set; }
        public Suit TrumpSuit { get; set; }
        public Card TrumpCard { get; set; }
        public IPlayerTimer CurrentPlayerTimer { get; private set; }
        public Dictionary<Player, int> RoundScores { get; private set; }

        public GameState(IPlayerTimer playerTimer)
        {
            CurrentPlayerTimer = playerTimer;
            Players = new List<Player>();
            RoundScores = new Dictionary<Player, int>();
        }

        public void Reset()
        {
            Players.Clear();
            RoundScores.Clear();
            Winner = null;
            CurrentPlayer = null;
            TrumpSuit = default;
            TrumpCard = null;
        }
    }
}
