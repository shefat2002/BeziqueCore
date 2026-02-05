using BeziqueCore;

namespace BeziqueCLI.UI.Components;

/// <summary>
/// Renders playing cards in CLI format
/// </summary>
public class CardRenderer
{
    private readonly Dictionary<Suit, ConsoleColor> _suitColors = new()
    {
        { Suit.Hearts, ConsoleColor.Red },
        { Suit.Diamonds, ConsoleColor.Red },
        { Suit.Clubs, ConsoleColor.DarkBlue },
        { Suit.Spades, ConsoleColor.DarkBlue }
    };

    public string RenderCard(Card card)
    {
        if (card.IsJoker)
        {
            return "🃏 JOKER";
        }

        string rank = GetRankSymbol(card.Rank);
        string suit = GetSuitSymbol(card.Suit);
        return $"{rank}{suit}";
    }

    public void ShowHand(List<Card> hand)
    {
        if (hand.Count == 0)
        {
            Console.WriteLine("    (empty)");
            return;
        }

        for (int i = 0; i < hand.Count; i++)
        {
            var card = hand[i];
            string cardDisplay = RenderCard(card);

            // Set color based on suit
            if (!card.IsJoker && _suitColors.TryGetValue(card.Suit, out var color))
            {
                Console.ForegroundColor = color;
            }

            Console.WriteLine($"    {i + 1,2}. {cardDisplay}");
            Console.ResetColor();
        }
    }

    private string GetRankSymbol(Rank rank)
    {
        return rank switch
        {
            Rank.Seven => "7",
            Rank.Eight => "8",
            Rank.Nine => "9",
            Rank.Jack => "J",
            Rank.Queen => "Q",
            Rank.King => "K",
            Rank.Ten => "10",
            Rank.Ace => "A",
            _ => "?"
        };
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
