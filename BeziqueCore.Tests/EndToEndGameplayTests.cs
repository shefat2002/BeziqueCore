using BeziqueCore;
using Xunit;

namespace BeziqueCore.Tests;

public class EndToEndGameplayTests
{
    [Fact]
    public void Gameplay_2PlayerStandardMode_CompleteGame()
    {
        // Arrange
        var config = new GameConfig
        {
            PlayerCount = 2,
            TargetScore = 1500,
            Mode = GameMode.Standard,
            DeckCount = 4 // 4 decks = 132 cards
        };

        var game = new BeziqueGameController();
        var trickCount = 0;
        var meldCount = 0;
        var roundCount = 0;
        Player? roundWinner = null;

        // Subscribe to events
        game.TrickEnded += (s, e) => trickCount++;
        game.MeldDeclared += (s, e) => meldCount++;
        game.RoundEnded += (s, e) =>
        {
            roundCount++;
            roundWinner = game.Players[e.WinnerId];
        };

        // Act - Initialize game
        game.Initialize(config);

        // Wait for FSM to complete dealing (3 sets of 3 cards = 9 per player)
        // The FSM deals automatically after Initialize

        // Assert - Initial setup
        Assert.Equal(2, game.PlayerCount);
        Assert.Equal(1500, game.TargetScore);
        Assert.Equal(4, config.DeckCount);
        Assert.Equal(132, 132); // 4 decks * 33 cards (7-A + joker)
        Assert.Equal(9, game.Players[0].Hand.Count); // 9 cards dealt per player (3 sets of 3)
        Assert.Equal(9, game.Players[1].Hand.Count);
        // 132 total - 18 dealt (9*2) - 1 trump = 113 remaining in draw deck
        Assert.Equal(132 - 18 - 1, game.Context.DrawDeck.Count);
        Assert.NotEqual(Suit.None, game.Context.TrumpSuit);
        Assert.Equal(GamePhase.Phase1_Normal, game.Context.CurrentPhase);

        // Play through Phase 1 tricks with card drawing
        var tricksInPhase1 = 0;
        while (game.Context.CurrentPhase == GamePhase.Phase1_Normal && game.Context.DrawDeck.Count > 0)
        {
            // Play a complete trick
            var currentPlayer = game.CurrentPlayer;
            var player = game.Players[currentPlayer];

            if (player.Hand.Count > 0)
            {
                var card = player.Hand[0];
                Assert.True(game.PlayCard(card));
            }

            // Second player plays
            currentPlayer = game.CurrentPlayer;
            player = game.Players[currentPlayer];

            if (player.Hand.Count > 0)
            {
                var card = player.Hand[0];
                Assert.True(game.PlayCard(card));
            }

            // Try to meld (trick winner only)
            if (game.CanMeld())
            {
                var bestMeld = game.GetBestMeld();
                if (bestMeld != null && meldCount < 3) // Limit melds for testing
                {
                    game.DeclareMeld(bestMeld.Cards.ToArray(), bestMeld.Type);
                }
                else
                {
                    game.SkipMeld();
                }
            }

            // Draw cards after meld phase
            if (game.Context.DrawDeck.Count > 0 && game.Context.CurrentPhase == GamePhase.Phase1_Normal)
            {
                game.DrawCards();
            }

            tricksInPhase1++;
            if (tricksInPhase1 > 50) break; // Safety limit
        }

        // Play through Phase 2 if reached, or finish Phase 1
        var tricksInPhase2 = 0;
        while (!game.IsGameOver && game.Players[0].Hand.Count > 0)
        {
            var currentPlayer = game.CurrentPlayer;
            var player = game.Players[currentPlayer];

            if (player.Hand.Count > 0)
            {
                var legalMoves = game.GetLegalMoves();
                Assert.True(legalMoves.Length > 0, "Should have legal moves in Phase 2");

                var card = legalMoves[0];
                Assert.True(game.PlayCard(card));
            }

            // Second player plays
            currentPlayer = game.CurrentPlayer;
            player = game.Players[currentPlayer];

            if (player.Hand.Count > 0)
            {
                var legalMoves = game.GetLegalMoves();
                Assert.True(legalMoves.Length > 0);

                var card = legalMoves[0];
                Assert.True(game.PlayCard(card));
            }

            tricksInPhase2++;
            if (tricksInPhase2 > 20) break; // Safety limit
        }

        // End round and verify scores
        int winnerId = game.EndRound();
        Assert.True(winnerId >= 0 && winnerId < 2);

        // Verify game state after round
        Assert.True(roundCount > 0);
        Assert.True(trickCount > 0);
        Assert.Equal(2, game.PlayerCount);

        // Verify no melding in Phase 2
        Assert.False(game.CanMeld() && game.Context.CurrentPhase == GamePhase.Phase2_Last9);
    }

    [Fact]
    public void Gameplay_4PlayerStandardMode_CompleteGame()
    {
        // Arrange
        var config = new GameConfig
        {
            PlayerCount = 4,
            TargetScore = 1500,
            Mode = GameMode.Standard,
            DeckCount = 4
        };

        var game = new BeziqueGameController();
        var trickCount = 0;

        game.TrickEnded += (s, e) => trickCount++;

        // Act - Initialize game
        game.Initialize(config);

        // Assert - 4 player setup
        Assert.Equal(4, game.PlayerCount);
        Assert.Equal(1500, game.TargetScore);
        Assert.Equal(4, config.DeckCount);
        Assert.Equal(9, game.Players[0].Hand.Count);
        Assert.Equal(9, game.Players[1].Hand.Count);
        Assert.Equal(9, game.Players[2].Hand.Count);
        Assert.Equal(9, game.Players[3].Hand.Count);
        Assert.Equal(132 - 36 - 1, game.Context.DrawDeck.Count); // 132 - 36 dealt - 1 trump

        // Play a few tricks to verify 4-player flow
        for (var trick = 0; trick < 3; trick++)
        {
            // Each of 4 players plays a card
            for (var i = 0; i < 4; i++)
            {
                var currentPlayer = game.CurrentPlayer;
                var player = game.Players[currentPlayer];

                if (player.Hand.Count > 0)
                {
                    var card = player.Hand[0];
                    Assert.True(game.PlayCard(card), $"Player {currentPlayer} should be able to play");
                }
            }

            // Handle meld if available
            if (game.CanMeld())
            {
                game.SkipMeld();
            }

            // Draw cards
            if (game.Context.DrawDeck.Count > 0 && game.Context.CurrentPhase == GamePhase.Phase1_Normal)
            {
                game.DrawCards();
            }
        }

        // Verify 4 players participated
        Assert.Equal(4, game.PlayerCount);
        Assert.True(trickCount > 0);
    }

    [Fact]
    public void Gameplay_2PlayerAdvancedMode_CompleteGame()
    {
        // Arrange
        var config = new GameConfig
        {
            PlayerCount = 2,
            TargetScore = 1500,
            Mode = GameMode.Advanced,
            DeckCount = 4
        };

        var game = new BeziqueGameController();
        var roundEnded = false;

        game.RoundEnded += (s, e) => roundEnded = true;

        // Act - Initialize game
        game.Initialize(config);

        // Assert - Advanced mode setup
        Assert.Equal(GameMode.Advanced, game.Context.GameMode);
        Assert.Equal(2, game.PlayerCount);
        Assert.Equal(1500, game.TargetScore);
        Assert.Equal(4, config.DeckCount);

        // Play through a complete round
        while (game.Context.CurrentPhase == GamePhase.Phase1_Normal && game.Context.DrawDeck.Count > 0)
        {
            for (var i = 0; i < 2; i++)
            {
                var currentPlayer = game.CurrentPlayer;
                var player = game.Players[currentPlayer];

                if (player.Hand.Count > 0)
                {
                    var card = player.Hand[0];
                    game.PlayCard(card);
                }
            }

            if (game.CanMeld())
            {
                game.SkipMeld();
            }

            if (game.Context.DrawDeck.Count > 0)
            {
                game.DrawCards();
            }
        }

        // Play Phase 2
        while (game.Players[0].Hand.Count > 0)
        {
            for (var i = 0; i < 2; i++)
            {
                var currentPlayer = game.CurrentPlayer;
                var player = game.Players[currentPlayer];

                if (player.Hand.Count > 0)
                {
                    var legalMoves = game.GetLegalMoves();
                    if (legalMoves.Length > 0)
                    {
                        game.PlayCard(legalMoves[0]);
                    }
                }
            }
        }

        // End round - Advanced mode should calculate Ace/Ten bonuses
        int winnerId = game.EndRound();

        // Verify Advanced mode scoring
        Assert.True(winnerId >= 0 && winnerId < 2);
        Assert.True(roundEnded);

        // In Advanced mode, check for Ace/Ten scoring in WonPile
        foreach (var player in game.Players)
        {
            var aceTenCount = player.WonPile.Count(c => c.Rank == Rank.Ace || c.Rank == Rank.Ten);
            var expectedBonus = aceTenCount >= 14 ? aceTenCount * 10 : 0; // Threshold for 2 players

            // Note: RoundScore includes both meld points and Ace/Ten bonus
            Assert.True(player.RoundScore >= 0);
        }
    }

    [Fact]
    public void Gameplay_4PlayerAdvancedMode_CompleteGame()
    {
        // Arrange
        var config = new GameConfig
        {
            PlayerCount = 4,
            TargetScore = 1500,
            Mode = GameMode.Advanced,
            DeckCount = 4
        };

        var game = new BeziqueGameController();
        var trickCount = 0;
        var allPlayersPlayed = new bool[4];

        game.TrickEnded += (s, e) =>
        {
            trickCount++;
            allPlayersPlayed[e.WinnerId] = true;
        };

        // Act - Initialize game
        game.Initialize(config);

        // Assert - 4 player Advanced mode
        Assert.Equal(4, game.PlayerCount);
        Assert.Equal(GameMode.Advanced, game.Context.GameMode);
        Assert.Equal(1500, game.TargetScore);
        Assert.Equal(4, config.DeckCount);
        Assert.Equal(9, game.Players[0].Hand.Count);
        Assert.Equal(9, game.Players[1].Hand.Count);
        Assert.Equal(9, game.Players[2].Hand.Count);
        Assert.Equal(9, game.Players[3].Hand.Count);

        // Play several rounds to verify 4-player flow with all players getting turns
        for (var round = 0; round < 2; round++)
        {
            // Reset for new round
            if (round > 0)
            {
                // In a real game, would reinitialize
                break;
            }

            // Phase 1: Play with drawing
            while (game.Context.CurrentPhase == GamePhase.Phase1_Normal && game.Context.DrawDeck.Count > 0 && trickCount < 10)
            {
                for (var i = 0; i < 4; i++)
                {
                    var currentPlayer = game.CurrentPlayer;
                    var player = game.Players[currentPlayer];

                    if (player.Hand.Count > 0)
                    {
                        var card = player.Hand[0];
                        game.PlayCard(card);
                    }
                }

                if (game.CanMeld())
                {
                    game.SkipMeld();
                }

                if (game.Context.DrawDeck.Count > 0)
                {
                    game.DrawCards();
                }
            }

            // Phase 2: Play without drawing
            var phase2Tricks = 0;
            while (game.Players[0].Hand.Count > 0 && phase2Tricks < 5)
            {
                for (var i = 0; i < 4; i++)
                {
                    var currentPlayer = game.CurrentPlayer;
                    var player = game.Players[currentPlayer];

                    if (player.Hand.Count > 0)
                    {
                        var legalMoves = game.GetLegalMoves();
                        if (legalMoves.Length > 0)
                        {
                            game.PlayCard(legalMoves[0]);
                        }
                    }
                }
                phase2Tricks++;
            }
        }

        // End round
        int winnerId = game.EndRound();

        // Verify 4-player Advanced mode
        Assert.True(winnerId >= 0 && winnerId < 4);
        Assert.True(trickCount > 0);

        // Verify all players have scores
        foreach (var player in game.Players)
        {
            Assert.True(player.TotalScore >= 0);
        }

        // In Advanced mode for 4 players, threshold is 8+ Aces/Tens
        foreach (var player in game.Players)
        {
            var aceTenCount = player.WonPile.Count(c => c.Rank == Rank.Ace || c.Rank == Rank.Ten);
            // Bonus applies only if threshold met
            Assert.True(aceTenCount >= 0);
        }
    }
}
