using BeziqueCore.Interfaces;

namespace BeziqueCore.Models
{
    public class GameState : IGameState
    {
        public List<Player> Players { get; }
        public Player CurrentPlayer { get; set; }
        public Player Winner { get; set; }
        public Suit TrumpSuit { get; set; }
        public Card TrumpCard { get; set; }
        public IPlayerTimer CurrentPlayerTimer { get; }
        public Dictionary<Player, int> RoundScores { get; }

        public GameState(IPlayerTimer playerTimer)
        {
            CurrentPlayerTimer = playerTimer;
            Players = new List<Player>();
            RoundScores = new Dictionary<Player, int>();
        }

        public void AddPlayer(Player player)
        {
            Players.Add(player);
            RoundScores[player] = 0;
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
