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
            players[i] = new Player(i) { Hand = new List<Card>() };
        }

        context = new GameContext
        {
            DrawDeck = drawDeck,
            TrumpCard = new Card((byte)0, -1),
            TrumpSuit = Suit.None,
            CurrentPhase = GamePhase.Phase1_Normal,
            CurrentTurnPlayer = 0,
            LastTrickWinner = 0,
            GameMode = config.Mode
        };
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

    public static bool CheckGameOver(Player[] players, ushort winningScoreThreshold)
    {
        return ScoringManager.HasAnyPlayerReachedTarget(players, winningScoreThreshold);
    }

    public static int GetFinalWinner(Player[] players)
    {
        return ScoringManager.GetWinnerId(players);
    }
}
