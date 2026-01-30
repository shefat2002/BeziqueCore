using BeziqueGame.CLI;
using System;
using BeziqueCore.Models;

namespace BeziqueGame.CLI.UI
{
    /// <summary>
    /// Main menu for the Bezique CLI game.
    /// Supports game mode selection (2-player vs 4-player) and rules display.
    /// </summary>
    public class MainMenu
    {
        private readonly GameController _controller;

        public MainMenu(GameController controller)
        {
            _controller = controller;
        }

        public void Show()
        {
            while (true)
            {
                Console.Clear();
                DisplayHeader();

                Console.WriteLine("\n=== BEZIQUE CARD GAME ===\n");
                Console.WriteLine("1. 2-Player Game (vs AI)");
                Console.WriteLine("2. 4-Player Game (Local Multiplayer)");
                Console.WriteLine("3. Show Rules");
                Console.WriteLine("4. Exit\n");

                Console.Write("Select option: ");
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        StartTwoPlayerGame();
                        break;
                    case "2":
                        StartFourPlayerGame();
                        break;
                    case "3":
                        ShowRules();
                        break;
                    case "4":
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("\nInvalid option. Press any key to try again...");
                        Console.ReadKey(true);
                        break;
                }
            }
        }

        private void DisplayHeader()
        {
            Console.WriteLine();
            Console.WriteLine("╔═══════════════════════════════════╗");
            Console.WriteLine("║                                   ║");
            Console.WriteLine("║           ♠ BEZIQUE ♠            ║");
            Console.WriteLine("║         Classic Card Game          ║");
            Console.WriteLine("║                                   ║");
            Console.WriteLine("╚═══════════════════════════════════╝");
        }

        private void StartTwoPlayerGame()
        {
            Console.Clear();
            Console.WriteLine("=== 2-PLAYER GAME ===\n");

            Console.Write("Enter your name: ");
            var playerName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(playerName))
            {
                playerName = "Player 1";
            }

            var mode = SelectGameMode();

            try
            {
                _controller.StartTwoPlayerGame(playerName, mode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
                Console.WriteLine("\nPress any key to return to menu...");
                Console.ReadKey(true);
            }
        }

        private void StartFourPlayerGame()
        {
            Console.Clear();
            Console.WriteLine("=== 4-PLAYER LOCAL GAME ===\n");
            Console.WriteLine("All players will play on this device.\n");

            var mode = SelectGameMode();

            try
            {
                _controller.StartFourPlayerGame(mode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
                Console.WriteLine("\nPress any key to return to menu...");
                Console.ReadKey(true);
            }
        }

        private GameMode SelectGameMode()
        {
            Console.WriteLine("\nSelect Game Mode:");
            Console.WriteLine("1. Standard Mode");
            Console.WriteLine("2. Advanced Mode (with Aces & Tens bonus)\n");

            Console.Write("Select mode: ");
            var choice = Console.ReadLine();

            return choice == "2" ? GameMode.Advanced : GameMode.Standard;
        }

        private void ShowRules()
        {
            Console.Clear();
            Console.WriteLine(_controller.GetRulesText());
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey(true);
        }
    }
}
