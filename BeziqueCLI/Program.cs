using System;
using System.Linq;
using System.Threading;
using BeziqueCore; // Reference to your SDK

namespace BeziqueSim
{
    class Program
    {
        // Settings
        private const int DELAY_MS = 800; // Speed of the game loop
        private static Random _rng = new Random();

        static void Main(string[] args)
        {
            // IMPORTANT: Enable symbols like ♠ ♥ ♦ ♣
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
  ____            _                    
 |  _ \          (_) __ _ _   _  ___   
 | |_) |/ _ \_  / / / _` | | | |/ _ \  
 |  _ <|  __// / / | (_| | |_| |  __/  
 |_| \_\\___/___/_/ \__,_|\__,_|\___|  
            SERVER SIMULATOR
            ");
            Console.ResetColor();
            Console.WriteLine("Press [ENTER] to Deal Cards...");
            Console.ReadLine();

            // 1. Initialize Controller
            var controller = new BeziqueGameController();
            HookEvents(controller);

            // 2. Configure & Initialize
            var config = new GameConfig
            {
                PlayerCount = 2,
                TargetScore = 1500,
                Mode = GameMode.Advanced
            };

            Console.WriteLine("Initializing Game State...");
            controller.Initialize(config);
            
            // 3. SHOW VISUALS
            VisualizeInitialization(controller);

            // Show initial hands
            ShowAllHands(controller);

            Console.WriteLine("\n[SYSTEM] Game is ready. Press [ENTER] to start the Match Loop.");
            Console.ReadLine();

            // 4. Run Game Loop
            RunGameLoop(controller);

            Console.WriteLine("\nSimulation Finished.");
            Console.ReadLine();
        }

        // ==========================================================
        //  VISUALIZATION LOGIC
        // ==========================================================

        static void VisualizeInitialization(BeziqueGameController game)
        {
            Console.Clear();
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine("                  D E A L I N G   P H A S E                 ");
            Console.WriteLine("------------------------------------------------------------\n");

            // 1. Simulate Dealing Animation
            Console.Write("Dealing: ");
            for (int i = 0; i < 9; i++) 
            { 
                Console.Write("■ "); 
                Thread.Sleep(50); // Fake delay
            }
            Console.WriteLine(" Done.\n");

            // 2. Show Trump
            var trump = game.Context.TrumpCard;
            Console.WriteLine("TRUMP CARD SELECTED:");
            PrintBigCard(trump);
            Console.WriteLine($"\nTrump Suit: {game.Context.TrumpSuit}\n");

            // 3. Show Hands
            for (int i = 0; i < game.Players.Length; i++)
            {
                Console.WriteLine($"--- PLAYER {i} HAND ({game.Players[i].Hand.Count} cards) ---");
                PrintHand(game.Players[i].Hand);
                Console.WriteLine("\n");
                Thread.Sleep(300);
            }
        }

        static void PrintHand(List<Card> hand)
        {
            // Sort for better readability: Suit -> Rank
            var sortedHand = hand
                .OrderBy(c => c.Suit)
                .ThenByDescending(c => c.Rank)
                .ToList();

            foreach (var card in sortedHand)
            {
                PrintMiniCard(card);
                Console.Write(" ");
            }
            Console.WriteLine();
        }

        static void PrintMiniCard(Card c)
        {
            // Format: | A♥ |
            SetCardColor(c.Suit);
            string rank = GetRankSymbol(c);
            string suit = GetSuitSymbol(c.Suit);
            
            Console.Write("|");
            Console.Write($"{rank,2}{suit}");
            Console.Write("|");
            Console.ResetColor();
        }

        static void PrintBigCard(Card c)
        {
            SetCardColor(c.Suit);
            string r = GetRankSymbol(c);
            string s = GetSuitSymbol(c.Suit);

            // ASCII Art Card
            Console.WriteLine("┌─────────┐");
            Console.WriteLine($"│ {r,-2}      │");
            Console.WriteLine($"│         │");
            Console.WriteLine($"│    {s}    │");
            Console.WriteLine($"│         │");
            Console.WriteLine($"│      {r,2} │");
            Console.WriteLine("└─────────┘");
            Console.ResetColor();
        }

        static void SetCardColor(Suit s)
        {
            if (s == Suit.Hearts || s == Suit.Diamonds)
                Console.ForegroundColor = ConsoleColor.Red;
            else
                Console.ForegroundColor = ConsoleColor.Cyan;
        }

        static string GetSuitSymbol(Suit s) => s switch
        {
            Suit.Diamonds => "♦",
            Suit.Clubs => "♣",
            Suit.Hearts => "♥",
            Suit.Spades => "♠",
            _ => "?"
        };

        static string GetRankSymbol(Card c)
        {
            if (c.IsJoker) return "JK";
            return c.Rank switch
            {
                Rank.Ace => "A",
                Rank.King => "K",
                Rank.Queen => "Q",
                Rank.Jack => "J",
                Rank.Ten => "10",
                _ => ((int)c.Rank).ToString()
            };
        }

        // ==========================================================
        //  GAME LOOP (BOTS)
        // ==========================================================

        static void RunGameLoop(BeziqueGameController game)
        {
            int turns = 0;
            while (game.CurrentState != GameState.GameOver && turns < 2000)
            {
                turns++;
                Thread.Sleep(DELAY_MS); 

                try
                {
                    switch (game.CurrentState)
                    {
                        case GameState.Play:
                        case GameState.L9Play:
                            PerformBotMove(game);
                            break;

                        case GameState.Meld:
                            PerformBotMeld(game);
                            break;

                        case GameState.NewTrick:
                            Console.WriteLine("   [Dealer] Drawing new cards...");
                            game.DrawCards();

                            // Show hands after drawing
                            ShowAllHands(game);
                            break;

                        case GameState.RoundEnd:
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.WriteLine("\n=== ROUND ENDED ===");
                            Console.WriteLine("Calculating Advanced Score (Aces/Tens)...");
                            Console.ResetColor();
                            int roundWinner = game.EndRound();
                            if (game.CheckWinner() != -1) return;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"CRASH: {ex.Message}");
                    break;
                }
            }
        }

        static void PerformBotMove(BeziqueGameController game)
        {
            int pid = game.Context.CurrentTurnPlayer;
            var legalMoves = game.GetLegalMoves();

            if (legalMoves.Length == 0) return;

            // Bot Logic: Play random valid card
            var cardToPlay = legalMoves[_rng.Next(legalMoves.Length)];

            Console.Write($"[P{pid}] Plays ");
            PrintMiniCard(cardToPlay);
            Console.WriteLine();

            game.PlayCard(cardToPlay);

            // Show card lists after the turn
            ShowAllHands(game);
        }

        static void PerformBotMeld(BeziqueGameController game)
        {
            int pid = game.Context.CurrentTurnPlayer;

            // 1. Swap 7
            if (game.CanSwapTrumpSeven())
            {
                Console.WriteLine($"[P{pid}] *** SWAPS 7 OF TRUMP ***");
                game.SwapTrumpSeven();
            }

            // 2. Meld
            var bestMeld = game.GetBestMeld();
            if (bestMeld != null && game.CanMeld())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[P{pid}] Melds {bestMeld.Type} (+{bestMeld.Points})");
                Console.ResetColor();
                bool success = game.DeclareMeld(bestMeld.Cards, bestMeld.Type);
                if (!success)
                {
                    // Meld failed, skip instead
                    Console.WriteLine($"[P{pid}] Meld failed, skipping...");
                    game.SkipMeld();
                }
            }
            else
            {
                Console.WriteLine($"[P{pid}] Skips Meld");
                game.SkipMeld();
            }

            // Show card lists after meld
            ShowAllHands(game);
        }

        static void ShowAllHands(BeziqueGameController game)
        {
            Console.WriteLine();
            Console.WriteLine("────────────────────────────────────────────────────────────────");
            Console.WriteLine("                      CURRENT HANDS STATE");
            Console.WriteLine("────────────────────────────────────────────────────────────────");

            for (int i = 0; i < game.Players.Length; i++)
            {
                var player = game.Players[i];
                Console.WriteLine($"\n  PLAYER {i} - Hand: {player.Hand.Count} cards | Table: {player.TableCards.Count} cards | Won: {player.WonPile.Count} | Score: {player.RoundScore}/{player.TotalScore}");

                // Show hand cards
                if (player.Hand.Count > 0)
                {
                    Console.Write("    Hand:   ");
                    PrintHand(player.Hand);
                }

                // Show table cards (melded)
                if (player.TableCards.Count > 0)
                {
                    Console.Write("    Melded:  ");
                    PrintHand(player.TableCards);
                }
            }

            Console.WriteLine("\n────────────────────────────────────────────────────────────────");
        }

        static void HookEvents(BeziqueGameController game)
        {
            game.TrickEnded += (s, e) => Console.WriteLine($"   -> Trick won by Player {e.WinnerId}");
            game.PhaseChanged += (s, e) => Console.WriteLine($"\n!!! PHASE CHANGE: {e.NewPhase} !!!\n");
            game.GameEnded += (s, e) => 
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n\n🏆 GAME OVER! WINNER: PLAYER {e.WinnerId} 🏆");
                Console.ResetColor();
            };
        }
    }
}