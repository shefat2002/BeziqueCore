using BeziqueCore;

namespace BeziqueCLI.UI;

/// <summary>
/// Handles user input for the CLI game
/// </summary>
public class PlayerInputHandler
{
    public int GetPlayerCount()
    {
        while (true)
        {
            Console.Write("Number of players (2 or 4) [default: 2]: ");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                return 2;
            }

            if (int.TryParse(input, out int count) && (count == 2 || count == 4))
            {
                return count;
            }

            Console.WriteLine("Invalid input. Please enter 2 or 4.");
        }
    }

    public GameMode GetGameMode()
    {
        while (true)
        {
            Console.Write("Game mode (1=Standard, 2=Advanced) [default: 1]: ");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                return GameMode.Standard;
            }

            if (int.TryParse(input, out int mode) && mode >= 1 && mode <= 2)
            {
                return mode == 1 ? GameMode.Standard : GameMode.Advanced;
            }

            Console.WriteLine("Invalid input. Please enter 1 or 2.");
        }
    }

    public ushort GetTargetScore()
    {
        while (true)
        {
            Console.Write("Target score (500/1000/1500) [default: 1500]: ");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                return 1500;
            }

            if (ushort.TryParse(input, out ushort score))
            {
                return score;
            }

            Console.WriteLine("Invalid input. Please enter a valid number.");
        }
    }

    public Card SelectCard(Player player, Card[] legalMoves, int playerId)
    {
        while (true)
        {
            Console.Write($"\nPlayer {playerId}, select a card (1-{player.Hand.Count}) or 'q' to quit: ");
            string? input = Console.ReadLine()?.Trim().ToLower();

            if (input == "q")
            {
                Console.WriteLine("\nThanks for playing!");
                Environment.Exit(0);
            }

            if (int.TryParse(input, out int selection) && selection >= 1 && selection <= player.Hand.Count)
            {
                Card selectedCard = player.Hand[selection - 1];

                if (legalMoves.Contains(selectedCard))
                {
                    return selectedCard;
                }

                Console.WriteLine("That card is not a legal move. Please select another.");
            }
            else
            {
                Console.WriteLine($"Invalid selection. Please enter a number between 1 and {player.Hand.Count}.");
            }
        }
    }

    public bool AskToMeld(int playerId, MeldOpportunity meld)
    {
        while (true)
        {
            Console.Write($"\nPlayer {playerId}, declare {meld.Type} for {meld.Points} points? (y/n): ");
            string? input = Console.ReadLine()?.Trim().ToLower();

            if (input == "y" || input == "yes")
            {
                return true;
            }

            if (input == "n" || input == "no")
            {
                return false;
            }

            Console.WriteLine("Invalid input. Please enter 'y' or 'n'.");
        }
    }
}
