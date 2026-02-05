using BeziqueCore;
using BeziqueCLI.UI.Components;

namespace BeziqueCLI.UI;

/// <summary>
/// Handles all visual output for the Bezique CLI game
/// </summary>
public class GameRenderer
{
    private const int CardWidth = 10;
        private readonly CardRenderer _cardRenderer;

        public GameRenderer()
        {
            _cardRenderer = new CardRenderer();
        }

        public void ShowWelcome()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                                            ║");
            Console.WriteLine("║                    ♠♥ BEZIQUE CARD GAME ♦♣                  ║");
            Console.WriteLine("║                                                            ║");
            Console.WriteLine("║              A Classic Two-Player Card Game                ║");
            Console.WriteLine("║                                                            ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("Press any key to start...");
            Console.ReadKey(true);
        }

        public void ShowConfigHeader()
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine("                    GAME CONFIGURATION");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine();
        }

        public void ShowGameStart(BeziqueGameController controller)
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                       GAME STARTED                           ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            ShowGameInfo(controller);
        }

        public void ShowGameState(BeziqueGameController controller)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine($"  TRUMP: {GetSuitSymbol(controller.Context.TrumpSuit)} {controller.Context.TrumpSuit}");
            Console.WriteLine($"  PHASE: {(controller.IsPhase2 ? "LAST 9 CARDS" : "NORMAL")}");
            Console.WriteLine($"  TARGET: {controller.TargetScore} pts");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine();

            // Show all players
            for (int i = 0; i < controller.PlayerCount; i++)
            {
                ShowPlayerInfo(controller, i);
                Console.WriteLine();
            }

            // Show current trick if cards have been played
            if (controller.PlayedCards.Count > 0)
            {
                ShowCurrentTrick(controller);
            }
        }

        private void ShowPlayerInfo(BeziqueGameController controller, int playerId)
        {
            var player = controller.Players[playerId];
            bool isCurrentPlayer = controller.CurrentPlayer == playerId;
            bool isTrickWinner = controller.LastWinner == playerId;

            string prefix = isCurrentPlayer ? "► " : "  ";
            string suffix = isCurrentPlayer ? " ◄" : "";
            string winnerIndicator = isTrickWinner ? " 👑" : "";

            Console.WriteLine($"{prefix}PLAYER {playerId}{suffix}{winnerIndicator}");
            Console.WriteLine($"  Hand: {player.Hand.Count} cards | Table: {player.TableCards.Count} | Won: {player.WonPile.Count}");
            Console.WriteLine($"  Score: {player.RoundScore} (Round) | {player.TotalScore} (Total)");

            if (isCurrentPlayer)
            {
                Console.WriteLine();
                Console.WriteLine("  Your Cards:");
                _cardRenderer.ShowHand(player.Hand);
            }
        }

        private void ShowCurrentTrick(BeziqueGameController controller)
        {
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine("  CURRENT TRICK");
            Console.WriteLine("═══════════════════════════════════════════════════════════");

            int currentPlayer = controller.CurrentPlayer;
            int playerCount = controller.PlayerCount;

            // Calculate who played each card
            for (int i = 0; i < controller.PlayedCards.Count; i++)
            {
                int playerIdx = (currentPlayer - controller.PlayedCards.Count + i + playerCount) % playerCount;
                var card = controller.PlayedCards[i];
                string cardDisplay = _cardRenderer.RenderCard(card);
                Console.WriteLine($"  Player {playerIdx}: {cardDisplay}");
            }

            Console.WriteLine();
        }

        public void ShowCardPlayed(int playerId, Card card, int cardsInTrick, int playerCount)
        {
            string cardDisplay = _cardRenderer.RenderCard(card);
            Console.WriteLine($"\nPlayer {playerId} played: {cardDisplay} ({cardsInTrick}/{playerCount} played)");
        }

        public void ShowTrickCards(List<Card> playedCards, int playerCount)
        {
            Console.WriteLine("\n═══════════════════════════════════════════════════════════");
            Console.WriteLine("  TRICK COMPLETE");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
        }

        public void ShowTrickWinner(int winnerId)
        {
            Console.WriteLine($"\n🎉 Player {winnerId} wins the trick!");
        }

        public void ShowMeldOpportunity(int playerId, MeldOpportunity meld)
        {
            Console.WriteLine($"\n═══════════════════════════════════════════════════════════");
            Console.WriteLine($"  MELD OPPORTUNITY for Player {playerId}");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine($"  Type: {meld.Type}");
            Console.WriteLine($"  Points: {meld.Points}");
            Console.WriteLine($"  Cards: {string.Join(", ", meld.Cards.Select(c => _cardRenderer.RenderCard(c)))}");
        }

        public void ShowMeldSuccess(int playerId, MeldOpportunity meld)
        {
            Console.WriteLine($"\n✓ Meld declared! Player {playerId} scores {meld.Points} points for {meld.Type}!");
        }

        public void ShowMeldFailed()
        {
            Console.WriteLine("\n✗ Meld failed!");
        }

        public void ShowMeldSkipped(int playerId)
        {
            Console.WriteLine($"\nPlayer {playerId} skipped melding.");
        }

        public void ShowPhase2Warning()
        {
            Console.WriteLine("\n⚠️  PHASE 2: LAST 9 CARDS - STRICT RULES APPLY!");
            Console.WriteLine("   No melding allowed. Must follow suit.");
        }

        public void ShowFinalTrickBonus()
        {
            Console.WriteLine("\n★ FINAL TRICK BONUS AWARDED! ★");
        }

        public void ShowPhaseChange(GamePhase newPhase)
        {
            Console.WriteLine($"\n{'═' * 60}");
            if (newPhase == GamePhase.Phase2_Last9)
            {
                Console.WriteLine("  🔄 PHASE TRANSITION: LAST 9 CARDS");
                Console.WriteLine("  Melding disabled. All table cards returned to hand.");
            }
            Console.WriteLine($"{'═' * 60}");
        }

        public void ShowMeldDeclared(int playerId, MeldType meldType, int points)
        {
            Console.WriteLine($"\n★ Player {playerId} declared {meldType} for {points} points!");
        }

        public void ShowRoundEnded(int winnerId, int[] scores)
        {
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                       ROUND ENDED                            ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.WriteLine($"\nRound Winner: Player {winnerId}");
            Console.WriteLine("\nFinal Scores:");
            for (int i = 0; i < scores.Length; i++)
            {
                Console.WriteLine($"  Player {i}: {scores[i]} points");
            }
        }

        public void ShowGameWinner(int winnerId)
        {
            Console.WriteLine($"\n╔{'═' * 60}╗");
            Console.WriteLine($"║  {'★' * 20} GAME OVER {'★' * 20}  ║");
            Console.WriteLine($"║  {'═' * 60}║");
            Console.WriteLine($"║           PLAYER {winnerId} WINS THE GAME!                ║");
            Console.WriteLine($"║  {'═' * 60}║");
            Console.WriteLine($"║  {'★' * 20}★★★★★★★{'★' * 20}  ║");
            Console.WriteLine($"╚{'═' * 60}╝");
        }

        public void ShowGameOver(BeziqueGameController controller)
        {
            Console.WriteLine("\n\n═══════════════════════════════════════════════════════════");
            Console.WriteLine("                    FINAL SCOREBOARD");
            Console.WriteLine("═══════════════════════════════════════════════════════════");

            var sortedPlayers = controller.Players
                .Select((p, i) => new { Player = p, Id = i })
                .OrderByDescending(x => x.Player.TotalScore)
                .ToList();

            foreach (var item in sortedPlayers)
            {
                Console.WriteLine($"  Player {item.Id}: {item.Player.TotalScore} points " +
                    $"(Round: {item.Player.RoundScore})");
            }

            Console.WriteLine("\n═══════════════════════════════════════════════════════════");
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey(true);
        }

        public void ShowError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n❌ ERROR: {message}");
            Console.ResetColor();
        }

        private void ShowGameInfo(BeziqueGameController controller)
        {
            Console.WriteLine($"Players: {controller.PlayerCount}");
            Console.WriteLine($"Mode: {controller.Context.GameMode}");
            Console.WriteLine($"Target Score: {controller.TargetScore}");
            Console.WriteLine($"Trump: {controller.Context.TrumpSuit} ({controller.Context.TrumpCard.Rank})");
            Console.WriteLine($"Cards Remaining: {controller.Context.DrawDeck.Count}");
            Console.WriteLine();

            for (int i = 0; i < controller.PlayerCount; i++)
            {
                Console.WriteLine($"Player {i}: {controller.Players[i].Hand.Count} cards");
            }

            Console.WriteLine("\nPress any key to begin...");
            Console.ReadKey(true);
        }

        private string GetSuitSymbol(Suit suit)
        {
            return suit switch
            {
                Suit.Hearts => "♥",
                Suit.Diamonds => "♦",
                Suit.Clubs => "♣",
                Suit.Spades => "♠",
                _ => "?"
            };
        }
    }
