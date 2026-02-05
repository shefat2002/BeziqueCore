namespace BeziqueCore;

public enum GameState : byte
{
    Deal,
    Play,
    Meld,
    NewTrick,
    L9Play,
    L9NewTrick,
    RoundEnd,
    GameOver
}

public static class TurnManager
{
    public static int AdvanceTurn(int currentPlayerId, byte playerCount) =>
        (currentPlayerId + 1) % playerCount;

    public static int SetFirstTurn(int winnerId) => winnerId;
}

public static class GameInitializer
{
    public static void InitializeGame(GameConfig config, out Player[] players, out GameContext context)
    {
        var deck = DeckFactory.Shuffled(new Interfaces.SystemRandom(), config.DeckCount);
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

        // Select trump card
        var trumpCard = drawDeck.Count > 0 ? drawDeck.Pop() : new Card((byte)0, -1);

        context = new GameContext
        {
            DrawDeck = drawDeck,
            TrumpCard = trumpCard,
            TrumpSuit = trumpCard.IsJoker ? Suit.Diamonds : trumpCard.Suit,
            CurrentPhase = GamePhase.Phase1_Normal,
            CurrentTurnPlayer = 0,
            LastTrickWinner = 0,
            GameMode = config.Mode
        };

        // Dealer bonus if trump is a seven
        if (trumpCard.Rank == Rank.Seven && !trumpCard.IsJoker)
        {
            players[config.PlayerCount - 1].RoundScore += 10;
        }
    }
}

public static class PlayStateHandler
{
    public static bool ValidateAndPlayCardPhase1(Player player, Card card, List<Card> playedCards, int playerIndex)
    {
        if (!PlayCardValidator.TryPlayCard(player, card, out var playedCard)) return false;

        playedCards.Add(playedCard);
        return true;
    }
}

public static class L9PlayStateHandler
{
    public static bool ValidateAndPlayCardPhase2(Player player, Card card, List<Card> playedCards, int playerIndex, Suit leadSuit, Card? currentWinner, Suit trump)
    {
        if (!Phase2MoveValidator.IsLegalMove(player.Hand, card, leadSuit, currentWinner, trump)) return false;

        if (!PlayCardValidator.TryRemoveCard(player.Hand, card)) return false;

        playedCards.Add(card);
        return true;
    }
}

public static class TrickResolverHandler
{
    public static int ResolveTrick(List<Card> playedCards, Player[] players, int[] playerIndices, Suit trump, bool isFinalTrick)
    {
        int winnerIndex = TrickEvaluator.GetWinner(playedCards.ToArray(), trump);

        var trumpSevenIndices = TrumpSevenDetector.FindTrumpSevenPlayers(playedCards.ToArray(), trump);
        foreach (var cardIndex in trumpSevenIndices)
        {
            players[playerIndices[cardIndex]].RoundScore += 10;
        }

        if (isFinalTrick)
        {
            bool winnerUsedTrumpSeven = false;
            for (int i = 0; i < playedCards.Count; i++)
            {
                if (i == winnerIndex && TrumpSevenDetector.IsTrumpSeven(playedCards[i], trump))
                {
                    winnerUsedTrumpSeven = true;
                    break;
                }
            }

            if (winnerUsedTrumpSeven)
            {
                players[playerIndices[winnerIndex]].RoundScore += 20;
            }
            else
            {
                players[playerIndices[winnerIndex]].RoundScore += 10;
            }
        }

        int winnerPlayerId = playerIndices[winnerIndex];

        foreach (var card in playedCards)
        {
            players[winnerPlayerId].WonPile.Add(card);
        }

        return winnerPlayerId;
    }
}

public static class MeldStateHandler
{
    public static bool DeclareMeld(Player player, Card[] cards, MeldType meldType, Suit trump)
    {
        return MeldValidator.TryExecuteMeld(player, cards, meldType, trump);
    }

    public static bool DeclareMeld(Player player, List<Card> cards, MeldType meldType, Suit trump)
    {
        return MeldValidator.TryExecuteMeld(player, cards, meldType, trump);
    }

    public static bool CanSwapTrumpSeven(Player player, Card trumpCard, Suit trump)
    {
        if (player.HasSwappedSeven) return false;
        if (trumpCard.Rank == Rank.Seven) return false;

        for (int i = 0; i < player.Hand.Count; i++)
        {
            if (TrumpSevenDetector.IsTrumpSeven(player.Hand[i], trump))
            {
                return true;
            }
        }

        return false;
    }

    public static bool SwapTrumpSeven(Player player, ref Card trumpCard, Suit trump)
    {
        if (!CanSwapTrumpSeven(player, trumpCard, trump)) return false;

        for (int i = 0; i < player.Hand.Count; i++)
        {
            if (TrumpSevenDetector.IsTrumpSeven(player.Hand[i], trump))
            {
                var temp = player.Hand[i];
                player.Hand[i] = trumpCard;
                trumpCard = temp;
                player.HasSwappedSeven = true;
                player.RoundScore += 10;
                return true;
            }
        }

        return false;
    }
}

public static class NewTrickHandler
{
    public static void StartNewTrick(List<Card> playedCards, int winnerPlayerId, ref int currentTurnPlayer)
    {
        playedCards.Clear();
        currentTurnPlayer = winnerPlayerId;
    }
}

public static class RoundEndHandler
{
    public static int EndRound(Player[] players, GameMode mode, byte playerCount)
    {
        ScoringManager.CalculateRoundScores(players, mode, playerCount);

        int highestScore = int.MinValue;
        int winnerId = -1;

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].RoundScore > highestScore)
            {
                highestScore = players[i].RoundScore;
                winnerId = i;
            }
        }

        return winnerId;
    }

    public static void ResetRound(Player[] players)
    {
        for (int i = 0; i < players.Length; i++)
        {
            players[i].RoundScore = 0;
            players[i].Hand.Clear();
            players[i].TableCards.Clear();
            players[i].WonPile.Clear();
            players[i].HasSwappedSeven = false;
            players[i].MeldHistory.Clear();
        }
    }

    public static bool CheckGameOver(Player[] players, ushort winningScoreThreshold)
    {
        return ScoringManager.HasAnyPlayerReachedTarget(players, winningScoreThreshold);
    }

    public static int GetFinalWinner(Player[] players)
    {
        return ScoringManager.GetWinnerId(players);
    }
}
