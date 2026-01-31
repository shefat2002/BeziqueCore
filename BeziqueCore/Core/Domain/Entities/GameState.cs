using BeziqueCore.Core.Application.Interfaces;

namespace BeziqueCore.Core.Domain.Entities
{
    public enum GameMode
    {
        Standard,
        Advanced
    }

    public class GameState : IGameState
    {
        private readonly object _lock = new();

        public List<Player> Players { get; }
        public Player CurrentPlayer { get; set; }
        public Player Winner { get; set; }
        public Player LastTrickWinner { get; set; }
        public Suit TrumpSuit { get; set; }
        public Card TrumpCard { get; set; }
        public IPlayerTimer CurrentPlayerTimer { get; }
        public Dictionary<Player, int> RoundScores { get; }

        // Trick tracking
        public Dictionary<Player, Card> CurrentTrick { get; set; }
        public Suit? LeadSuit { get; set; }

        // Game mode - Standard or Advanced
        public GameMode Mode { get; set; } = GameMode.Standard;

        // State machine tracking for multiplayer
        public string CurrentStateName { get; set; } = "GAME_INIT";

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
            lock (_lock)
            {
                RoundScores.Clear();
                Winner = null;
                CurrentPlayer = null;
                LastTrickWinner = null;
                TrumpSuit = default;
                TrumpCard = null;
                CurrentTrick = new Dictionary<Player, Card>();
                LeadSuit = null;
            }
        }

        public void StartNewTrick()
        {
            lock (_lock)
            {
                CurrentTrick = new Dictionary<Player, Card>();
                LeadSuit = null;
            }
        }

        public void AddCardToTrick(Player player, Card card)
        {
            lock (_lock)
            {
                CurrentTrick[player] = card;
                if (!LeadSuit.HasValue && !card.IsJoker)
                {
                    LeadSuit = card.Suit;
                }
            }
        }

        public bool IsTrickComplete()
        {
            lock (_lock)
            {
                return CurrentTrick.Count == Players.Count;
            }
        }
    }
}
