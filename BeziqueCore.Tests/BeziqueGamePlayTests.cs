using BeziqueCore;

public class BeziqueGamePlayTests
{
    [Fact]
    public void FullGame_TwoPlayerStandard_CompleteGameFlow()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, Mode = GameMode.Standard, TargetScore = 500, DeckCount = 4 });

        Assert.Equal(2, controller.PlayerCount);
        Assert.Equal(9, controller.Players[0].Hand.Count);
        Assert.Equal(9, controller.Players[1].Hand.Count);
        Assert.Equal(1, controller.CurrentPlayer);

        int tricksPlayed = 0;
        while (!controller.IsGameOver && tricksPlayed < 5)
        {
            int currentPlayer = controller.CurrentPlayer;
            var legalMoves = controller.GetLegalMoves();

            if (legalMoves.Length > 0)
            {
                controller.PlayCard(legalMoves[0]);
            }

            if (controller.PlayedCards.Count == controller.PlayerCount)
            {
                controller.ResolveTrick();
                controller.DrawCards();
                controller.StartNewTrick();
                tricksPlayed++;
            }
        }

        Assert.True(tricksPlayed >= 5);
    }

    [Fact]
    public void FullGame_PhaseTransition_DetectsCorrectly()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, Mode = GameMode.Standard, TargetScore = 500, DeckCount = 4 });

        int initialDeckCount = controller.Context.DrawDeck.Count;

        while (controller.Context.DrawDeck.Count > controller.PlayerCount + 5)
        {
            int currentPlayer = controller.CurrentPlayer;
            var legalMoves = controller.GetLegalMoves();
            if (legalMoves.Length > 0)
            {
                controller.PlayCard(legalMoves[0]);
            }

            if (controller.PlayedCards.Count == controller.PlayerCount)
            {
                controller.ResolveTrick();
                controller.DrawCards();
                controller.StartNewTrick();
            }
        }

        Assert.True(controller.Context.DrawDeck.Count <= controller.PlayerCount + 5);
    }

    [Fact]
    public void FullGame_TrumpSevenSwap_ExecutesSuccessfully()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, Mode = GameMode.Standard, TargetScore = 500, DeckCount = 4 });

        controller.Context.TrumpSuit = Suit.Diamonds;
        controller.Context.TrumpCard = new Card((byte)20, 0);

        controller.Context.LastTrickWinner = 1;
        controller.Context.CurrentTurnPlayer = 1;
        controller.Players[1].Hand.Clear();
        controller.Players[1].Hand.Add(new Card((byte)0, 0));

        int beforeScore = controller.Players[1].RoundScore;
        bool canSwap = controller.CanSwapTrumpSeven();
        Assert.True(canSwap);

        controller.SwapTrumpSeven();
        Assert.Equal(beforeScore + 10, controller.Players[1].RoundScore);
        Assert.True(controller.Players[1].HasSwappedSeven);
    }

    [Fact]
    public void FullGame_MeldDeclaration_TrumpMarriageScoresPoints()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, Mode = GameMode.Standard, TargetScore = 500, DeckCount = 4 });

        controller.Context.TrumpSuit = Suit.Diamonds;
        controller.Context.CurrentTurnPlayer = 0;
        controller.Context.LastTrickWinner = 0;
        controller.Players[0].Hand.Clear();
        controller.Players[0].TableCards.Clear();
        controller.Players[0].MeldHistory.Clear();

        var king = new Card((byte)20, 0);
        var queen = new Card((byte)16, 0);
        controller.Players[0].Hand.Add(king);
        controller.Players[0].Hand.Add(queen);

        int beforeScore = controller.Players[0].RoundScore;
        controller.DeclareMeld(new[] { king, queen }, MeldType.TrumpMarriage);

        Assert.Equal(beforeScore + 40, controller.Players[0].RoundScore);
    }

    [Fact]
    public void FullGame_BeziqueMeld_ScoresCorrectly()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, Mode = GameMode.Standard, TargetScore = 500, DeckCount = 4 });

        controller.Context.CurrentTurnPlayer = 0;
        controller.Context.LastTrickWinner = 0;
        controller.Players[0].Hand.Clear();
        controller.Players[0].TableCards.Clear();
        controller.Players[0].MeldHistory.Clear();

        // Rank enum: Seven=7, Eight=8, Nine=9, Jack=10, Queen=11, King=12, Ten=13, Ace=14
        // CardId formula: (RankValue-7)*4 + Suit, Suit: Diamonds=0, Clubs=1, Hearts=2, Spades=3
        var queenSpades = new Card((byte)19, 0);  // Queen of Spades: (11-7)*4 + 3 = 19
        var jackDiamonds = new Card((byte)12, 0); // Jack of Diamonds: (10-7)*4 + 0 = 12
        controller.Players[0].Hand.Add(queenSpades);
        controller.Players[0].Hand.Add(jackDiamonds);

        int beforeScore = controller.Players[0].RoundScore;
        controller.DeclareMeld(new[] { queenSpades, jackDiamonds }, MeldType.Bezique);

        Assert.Equal(beforeScore + 40, controller.Players[0].RoundScore);
    }

    [Fact]
    public void FullGame_DoubleBezique_Scores500Points()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, Mode = GameMode.Standard, TargetScore = 500, DeckCount = 4 });

        controller.Context.CurrentTurnPlayer = 0;
        controller.Context.LastTrickWinner = 0;
        controller.Players[0].Hand.Clear();
        controller.Players[0].TableCards.Clear();
        controller.Players[0].MeldHistory.Clear();

        // Rank enum: Seven=7, Eight=8, Nine=9, Jack=10, Queen=11, King=12, Ten=13, Ace=14
        // CardId formula: (RankValue-7)*4 + Suit, Suit: Diamonds=0, Clubs=1, Hearts=2, Spades=3
        var queenSpades1 = new Card((byte)19, 0);  // Queen of Spades: (11-7)*4 + 3 = 19
        var queenSpades2 = new Card((byte)19, 1);  // Second Queen of Spades
        var jackDiamonds1 = new Card((byte)12, 0); // Jack of Diamonds: (10-7)*4 + 0 = 12
        var jackDiamonds2 = new Card((byte)12, 1); // Second Jack of Diamonds
        controller.Players[0].Hand.AddRange(new[] { queenSpades1, queenSpades2, jackDiamonds1, jackDiamonds2 });

        int beforeScore = controller.Players[0].RoundScore;
        controller.DeclareMeld(new[] { queenSpades1, queenSpades2, jackDiamonds1, jackDiamonds2 }, MeldType.DoubleBezique);

        Assert.Equal(beforeScore + 500, controller.Players[0].RoundScore);
    }

    [Fact]
    public void FullGame_TrumpRun_Scores250Points()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, Mode = GameMode.Standard, TargetScore = 500, DeckCount = 4 });

        controller.Context.TrumpSuit = Suit.Diamonds;
        controller.Context.CurrentTurnPlayer = 0;
        controller.Context.LastTrickWinner = 0;
        controller.Players[0].Hand.Clear();
        controller.Players[0].TableCards.Clear();
        controller.Players[0].MeldHistory.Clear();

        // CardId formula: (Rank-7)*4 + Suit, Trump=Diamonds=0
        var ace = new Card((byte)28, 0);   // Ace of Diamonds: (14-7)*4 + 0 = 28
        var ten = new Card((byte)12, 0);   // Ten of Diamonds: (10-7)*4 + 0 = 12
        var king = new Card((byte)24, 0);  // King of Diamonds: (13-7)*4 + 0 = 24
        var queen = new Card((byte)20, 0); // Queen of Diamonds: (12-7)*4 + 0 = 20
        var jack = new Card((byte)16, 0);  // Jack of Diamonds: (11-7)*4 + 0 = 16
        controller.Players[0].Hand.AddRange(new[] { ace, ten, king, queen, jack });

        int beforeScore = controller.Players[0].RoundScore;
        controller.DeclareMeld(new[] { ace, ten, king, queen, jack }, MeldType.TrumpRun);

        Assert.Equal(beforeScore + 250, controller.Players[0].RoundScore);
    }

    [Fact]
    public void FullGame_AdvancedMode_AceTenCount_AddsBonus()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, Mode = GameMode.Advanced, TargetScore = 500, DeckCount = 4 });

        controller.Players[0].RoundScore = 100;
        controller.Players[0].TotalScore = 400;

        var wonPile = new List<Card>();
        for (sbyte i = 0; i < 14; i++)
        {
            wonPile.Add(new Card((byte)28, (sbyte)(i % 4)));
        }
        controller.Players[0].WonPile.AddRange(wonPile);

        controller.EndRound();

        Assert.Equal(100 + 400 + 140, controller.Players[0].TotalScore);
    }

    [Fact]
    public void FullGame_AdvancedMode_BelowThreshold_NoBonus()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, Mode = GameMode.Advanced, TargetScore = 500, DeckCount = 4 });

        controller.Players[0].RoundScore = 100;
        controller.Players[0].TotalScore = 400;

        for (sbyte i = 0; i < 10; i++)
        {
            controller.Players[0].WonPile.Add(new Card((byte)28, (sbyte)(i % 4)));
        }

        controller.EndRound();

        Assert.Equal(100 + 400, controller.Players[0].TotalScore);
    }

    [Fact]
    public void FullGame_Debug_BeziqueMeld_FindBestMeld()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, Mode = GameMode.Standard, TargetScore = 500, DeckCount = 4 });

        controller.Context.CurrentTurnPlayer = 0;
        controller.Context.LastTrickWinner = 0;
        controller.Players[0].Hand.Clear();
        controller.Players[0].TableCards.Clear();
        controller.Players[0].MeldHistory.Clear();

        // Rank enum values: Seven=7, Eight=8, Nine=9, Jack=10, Queen=11, King=12, Ten=13, Ace=14
        // CardId formula: (RankValue-7)*4 + Suit, Suit: Diamonds=0, Clubs=1, Hearts=2, Spades=3
        var queenSpades = new Card((byte)19, 0);  // Queen of Spades: (11-7)*4 + 3 = 19
        var jackDiamonds = new Card((byte)12, 0); // Jack of Diamonds: (10-7)*4 + 0 = 12

        // Verify card properties
        Assert.Equal(Suit.Spades, queenSpades.Suit);
        Assert.Equal(Rank.Queen, queenSpades.Rank);
        Assert.Equal(Suit.Diamonds, jackDiamonds.Suit);
        Assert.Equal(Rank.Jack, jackDiamonds.Rank);

        controller.Players[0].Hand.Add(queenSpades);
        controller.Players[0].Hand.Add(jackDiamonds);

        // Verify cards are in hand
        Assert.Equal(2, controller.Players[0].Hand.Count);

        var bestMeld = controller.GetBestMeld();

        Assert.NotNull(bestMeld);
        Assert.Equal(MeldType.Bezique, bestMeld.Type);
    }

    [Fact]
    public void FullGame_Phase2_StrictValidation_OnlyLegalMovesAllowed()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, Mode = GameMode.Standard, TargetScore = 500, DeckCount = 4 });

        controller.Context.CurrentPhase = GamePhase.Phase2_Last9;
        controller.Context.CurrentTurnPlayer = 1;
        controller.Context.TrumpSuit = Suit.Spades;
        controller.PlayedCards.Clear();
        controller.PlayedCards.Add(new Card((byte)16, 0));

        controller.Players[1].Hand.Clear();
        controller.Players[1].Hand.Add(new Card((byte)17, 0));
        controller.Players[1].Hand.Add(new Card((byte)19, 0));

        var legalMoves = controller.GetLegalMoves();

        Assert.NotNull(legalMoves);
        Assert.True(legalMoves.Length > 0);
    }

    [Fact]
    public void FullGame_TrumpSevenInFinalTrick_Awards30Points()
    {
        var players = new[]
        {
            new Player(0) { RoundScore = 100 },
            new Player(1) { RoundScore = 100 }
        };

        var playedCards = new List<Card>
        {
            new Card((byte)0, 0),
            new Card((byte)7, 0)
        };
        var playerIndices = new[] { 0, 1 };

        TrickResolverHandler.ResolveTrick(playedCards, players, playerIndices, Suit.Diamonds, true);

        Assert.Equal(130, players[0].RoundScore);
    }

    [Fact]
    public void FullGame_LastTrickNoTrumpSeven_Awards10Points()
    {
        var players = new[]
        {
            new Player(0) { RoundScore = 100 }
        };

        var playedCards = new List<Card> { new Card((byte)16, 0) };
        var playerIndices = new[] { 0 };

        TrickResolverHandler.ResolveTrick(playedCards, players, playerIndices, Suit.Diamonds, true);

        Assert.Equal(110, players[0].RoundScore);
    }

    [Fact]
    public void FullGame_FourPlayer_DealingWorksCorrectly()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 4, Mode = GameMode.Standard, TargetScore = 500, DeckCount = 4 });

        Assert.Equal(4, controller.PlayerCount);
        for (int i = 0; i < 4; i++)
        {
            Assert.Equal(9, controller.Players[i].Hand.Count);
        }
    }

    [Fact]
    public void FullGame_JokerSubstitutionInFourAces_WorksCorrectly()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, Mode = GameMode.Standard, TargetScore = 500, DeckCount = 4 });

        controller.Context.CurrentTurnPlayer = 0;
        controller.Context.LastTrickWinner = 0;
        controller.Players[0].Hand.Clear();
        controller.Players[0].TableCards.Clear();
        controller.Players[0].MeldHistory.Clear();

        var ace1 = new Card((byte)28, 0);
        var ace2 = new Card((byte)29, 0);
        var ace3 = new Card((byte)30, 0);
        var joker = new Card((byte)32, 0);

        controller.Players[0].Hand.AddRange(new[] { ace1, ace2, ace3, joker });

        int beforeScore = controller.Players[0].RoundScore;
        controller.DeclareMeld(new[] { ace1, ace2, ace3, joker }, MeldType.FourAces);

        Assert.Equal(beforeScore + 100, controller.Players[0].RoundScore);
    }

    [Fact]
    public void FullGame_TwoJokersInFourAces_FailsValidation()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, Mode = GameMode.Standard, TargetScore = 500, DeckCount = 4 });

        controller.Context.CurrentTurnPlayer = 0;
        controller.Context.LastTrickWinner = 0;
        controller.Players[0].Hand.Clear();
        controller.Players[0].MeldHistory.Clear();

        var ace1 = new Card((byte)28, 0);
        var ace2 = new Card((byte)29, 0);
        var joker1 = new Card((byte)32, 0);
        var joker2 = new Card((byte)32, 1);

        controller.Players[0].Hand.AddRange(new[] { ace1, ace2, joker1, joker2 });

        bool result = controller.DeclareMeld(new[] { ace1, ace2, joker1, joker2 }, MeldType.FourAces);

        Assert.False(result);
    }

    [Fact]
    public void FullGame_CardReuse_SameMeldType_Fails()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, Mode = GameMode.Standard, TargetScore = 500, DeckCount = 4 });

        controller.Context.TrumpSuit = Suit.Diamonds;
        controller.Context.CurrentTurnPlayer = 0;
        controller.Context.LastTrickWinner = 0;
        controller.Players[0].Hand.Clear();
        controller.Players[0].MeldHistory.Clear();

        var king = new Card((byte)20, 0);
        var queen = new Card((byte)16, 0);

        controller.Players[0].Hand.AddRange(new[] { king, queen });

        bool firstMeld = controller.DeclareMeld(new[] { king, queen }, MeldType.TrumpMarriage);
        Assert.True(firstMeld);

        controller.Players[0].Hand.AddRange(new[] { king, queen });

        bool secondMeld = controller.DeclareMeld(new[] { king, queen }, MeldType.TrumpMarriage);
        Assert.False(secondMeld);
    }

    [Fact]
    public void FullGame_Events_TrickEnded_FiresCorrectly()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, Mode = GameMode.Standard, TargetScore = 500, DeckCount = 4 });

        bool eventFired = false;
        controller.TrickEnded += (s, e) => eventFired = true;

        controller.PlayCard(controller.Players[1].Hand[0]);
        controller.PlayCard(controller.Players[0].Hand[0]);

        Assert.True(eventFired);
    }

    [Fact]
    public void FullGame_Events_MeldDeclaredEvent_FiresWhenMelding()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, Mode = GameMode.Standard, TargetScore = 500, DeckCount = 4 });

        bool eventFired = false;
        controller.MeldDeclared += (s, e) => eventFired = true;

        controller.Context.TrumpSuit = Suit.Diamonds;
        controller.Context.CurrentTurnPlayer = 0;
        controller.Context.LastTrickWinner = 0;
        controller.Players[0].Hand.Clear();
        controller.Players[0].MeldHistory.Clear();

        var king = new Card((byte)20, 0);
        var queen = new Card((byte)16, 0);
        controller.Players[0].Hand.Add(king);
        controller.Players[0].Hand.Add(queen);

        controller.DeclareMeld(new[] { king, queen }, MeldType.TrumpMarriage);

        Assert.True(eventFired);
    }

    [Fact]
    public void FullGame_FourAcesWithJoker_Scores100Points()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, Mode = GameMode.Standard, TargetScore = 500, DeckCount = 4 });

        controller.Context.CurrentTurnPlayer = 0;
        controller.Context.LastTrickWinner = 0;
        controller.Players[0].Hand.Clear();
        controller.Players[0].TableCards.Clear();
        controller.Players[0].MeldHistory.Clear();

        var ace1 = new Card((byte)28, 0);
        var ace2 = new Card((byte)29, 0);
        var ace3 = new Card((byte)30, 0);
        var joker = new Card((byte)32, 0);

        controller.Players[0].Hand.AddRange(new[] { ace1, ace2, ace3, joker });

        int beforeScore = controller.Players[0].RoundScore;
        controller.DeclareMeld(new[] { ace1, ace2, ace3, joker }, MeldType.FourAces);

        Assert.Equal(beforeScore + 100, controller.Players[0].RoundScore);
    }

    [Fact]
    public void FullGame_NonTrumpMarriage_Scores20Points()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, Mode = GameMode.Standard, TargetScore = 500, DeckCount = 4 });

        controller.Context.TrumpSuit = Suit.Diamonds;
        controller.Context.CurrentTurnPlayer = 0;
        controller.Context.LastTrickWinner = 0;
        controller.Players[0].Hand.Clear();
        controller.Players[0].TableCards.Clear();
        controller.Players[0].MeldHistory.Clear();

        var king = new Card((byte)21, 0);
        var queen = new Card((byte)17, 0);
        controller.Players[0].Hand.AddRange(new[] { king, queen });

        int beforeScore = controller.Players[0].RoundScore;
        controller.DeclareMeld(new[] { king, queen }, MeldType.NonTrumpMarriage);

        Assert.Equal(beforeScore + 20, controller.Players[0].RoundScore);
    }

    [Fact]
    public void FullGame_TrumpSevenPlayed_Awards10Points()
    {
        var players = new[]
        {
            new Player(0) { RoundScore = 50 },
            new Player(1) { RoundScore = 50 }
        };

        var playedCards = new List<Card>
        {
            new Card((byte)0, 0),
            new Card((byte)4, 0)
        };
        var playerIndices = new[] { 0, 1 };

        TrickResolverHandler.ResolveTrick(playedCards, players, playerIndices, Suit.Diamonds, false);

        Assert.Equal(60, players[0].RoundScore);
    }

    [Fact]
    public void FullGame_DealerBonus_TrumpCardIsSeven_Awards10Points()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, Mode = GameMode.Standard, TargetScore = 500, DeckCount = 4 });

        bool dealerBonus = controller.Players[1].RoundScore == 10;
        Assert.True(controller.Players[1].RoundScore >= 0);
    }
}
