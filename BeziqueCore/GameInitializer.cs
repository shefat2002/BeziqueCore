using BeziqueCore.Interfaces;

namespace BeziqueCore;

public static class GameInitializer
{
    public static void InitializeGame(GameConfig config, out Player[] players, out GameContext context)
    {
        var deck = DeckFactory.Shuffled(new SystemRandom(), config.DeckCount);
        var drawDeck = new Stack<Card>(deck);

        players = new Player[config.PlayerCount];
        for (int i = 0; i < config.PlayerCount; i++)
        {
            var hand = new List<Card>();
            for (int j = 0; j < 9; j++)
            {
                if (drawDeck.Count > 0) hand.Add(drawDeck.Pop());
            }
            players[i] = new Player(i) { Hand = hand };
        }

        var trumpCard = drawDeck.Count > 0 ? drawDeck.Pop() : new Card((byte)0, -1);
        var trumpSuit = trumpCard.IsJoker ? Suit.Diamonds : trumpCard.Suit;

        context = new GameContext
        {
            DrawDeck = drawDeck,
            TrumpCard = trumpCard,
            TrumpSuit = trumpSuit,
            CurrentPhase = GamePhase.Phase1_Normal,
            CurrentTurnPlayer = 0,
            LastTrickWinner = 0
        };

        if (trumpCard.Rank == Rank.Seven)
        {
            players[config.PlayerCount - 1].RoundScore += 10;
        }
    }
}
