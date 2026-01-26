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

        // Trick tracking
        public Dictionary<Player, Card> CurrentTrick { get; set; }
        public Suit? LeadSuit { get; set; }

        public GameState(IPlayerTimer playerTimer)
        {
            CurrentPlayerTimer = playerTimer;
            Players = new List<Player>();
            RoundScores = new Dictionary<Player, int>();
            CurrentTrick = new Dictionary<Player, Card>();
        }

        public void AddPlayer(Player player)
        {
            Players.Add(player);
            RoundScores[player] = 0;
        }

        public void Reset()
        {
            RoundScores.Clear();
            Winner = null;
            CurrentPlayer = null;
            TrumpSuit = default;
            TrumpCard = null;
            CurrentTrick = new Dictionary<Player, Card>();
            LeadSuit = null;
        }

        public void StartNewTrick()
        {
            CurrentTrick = new Dictionary<Player, Card>();
            LeadSuit = null;
        }

        public void AddCardToTrick(Player player, Card card)
        {
            CurrentTrick[player] = card;
            if (!LeadSuit.HasValue && !card.IsJoker)
            {
                LeadSuit = card.Suit;
            }
        }

        public bool IsTrickComplete()
        {
            return CurrentTrick.Count == Players.Count;
        }
    }
}
