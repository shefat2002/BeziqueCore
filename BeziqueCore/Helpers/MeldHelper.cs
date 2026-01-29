using BeziqueCore.Interfaces;
using BeziqueCore.Models;
using System.Linq;

namespace BeziqueCore.Helpers
{
    /// <summary>
    /// Helper methods for finding all possible melds from a player's hand.
    /// This provides the core meld detection logic used by both AI bots and UI.
    /// </summary>
    public static class MeldHelper
    {
        /// <summary>
        /// Finds all possible melds that can be declared from the player's hand.
        /// Returns a list of all valid melds sorted by points (highest first).
        /// If no melds are available, returns an empty list.
        /// </summary>
        public static List<Meld> FindAllPossibleMelds(Player player, Suit trumpSuit)
        {
            var possibleMelds = new List<Meld>();
            var hand = player.Hand.ToList();

            // Check for Double Bezique (4 cards: 2x Q♠ + 2x J♦)
            var spadeQueens = hand.Where(c => c.Suit == Suit.Spades && c.Rank == Rank.Queen).ToList();
            var diamondJacks = hand.Where(c => c.Suit == Suit.Diamonds && c.Rank == Rank.Jack).ToList();

            if (spadeQueens.Count >= 2 && diamondJacks.Count >= 2)
            {
                possibleMelds.Add(new Meld
                {
                    Type = MeldType.DoubleBezique,
                    Cards = spadeQueens.Take(2).Concat(diamondJacks.Take(2)).ToList(),
                    Points = 500
                });
            }
            else if (spadeQueens.Count >= 1 && diamondJacks.Count >= 1)
            {
                // Check for single Bezique (Q♠ + J♦)
                possibleMelds.Add(new Meld
                {
                    Type = MeldType.Bezique,
                    Cards = new List<Card> { spadeQueens.First(), diamondJacks.First() },
                    Points = 40
                });
            }

            // Check for Four of a Kind (with Joker substitution)
            var ranks = new[] { Rank.Ace, Rank.King, Rank.Queen, Rank.Jack };
            foreach (var rank in ranks)
            {
                var rankCards = hand.Where(c => !c.IsJoker && c.Rank == rank).ToList();
                var jokerCount = hand.Count(c => c.IsJoker);
                var totalCards = rankCards.Count + jokerCount;

                if (totalCards >= 4)
                {
                    var meldType = rank switch
                    {
                        Rank.Ace => MeldType.FourAces,
                        Rank.King => MeldType.FourKings,
                        Rank.Queen => MeldType.FourQueens,
                        Rank.Jack => MeldType.FourJacks,
                        _ => MeldType.InvalidMeld
                    };

                    if (meldType != MeldType.InvalidMeld)
                    {
                        var points = rank switch
                        {
                            Rank.Ace => 100,
                            Rank.King => 80,
                            Rank.Queen => 60,
                            Rank.Jack => 40,
                            _ => 0
                        };

                        var meldCards = rankCards.Take(4).ToList();
                        // Add jokers if needed to make 4
                        var jokersNeeded = 4 - rankCards.Count;
                        if (jokersNeeded > 0 && jokerCount >= jokersNeeded)
                        {
                            var jokers = hand.Where(c => c.IsJoker).Take(jokersNeeded);
                            meldCards.AddRange(jokers);
                        }

                        possibleMelds.Add(new Meld
                        {
                            Type = meldType,
                            Cards = meldCards,
                            Points = points
                        });
                    }
                }
            }

            // Check for Trump Run (5 cards of trump suit: A, 10, K, Q, J)
            var trumpCards = hand.Where(c => c.Suit == trumpSuit).ToList();
            if (trumpCards.Count >= 5 &&
                trumpCards.Any(c => c.Rank == Rank.Ace) &&
                trumpCards.Any(c => c.Rank == Rank.Ten) &&
                trumpCards.Any(c => c.Rank == Rank.King) &&
                trumpCards.Any(c => c.Rank == Rank.Queen) &&
                trumpCards.Any(c => c.Rank == Rank.Jack))
            {
                possibleMelds.Add(new Meld
                {
                    Type = MeldType.TrumpRun,
                    Cards = new List<Card>
                    {
                        trumpCards.First(c => c.Rank == Rank.Ace),
                        trumpCards.First(c => c.Rank == Rank.Ten),
                        trumpCards.First(c => c.Rank == Rank.King),
                        trumpCards.First(c => c.Rank == Rank.Queen),
                        trumpCards.First(c => c.Rank == Rank.Jack)
                    },
                    Points = 250
                });
            }

            // Check for marriages (2 cards: K + Q of same suit)
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                var kings = hand.Where(c => c.Suit == suit && c.Rank == Rank.King).ToList();
                var queens = hand.Where(c => c.Suit == suit && c.Rank == Rank.Queen).ToList();

                if (kings.Any() && queens.Any())
                {
                    var isTrump = suit == trumpSuit;
                    possibleMelds.Add(new Meld
                    {
                        Type = isTrump ? MeldType.TrumpMarriage : MeldType.Marriage,
                        Cards = new List<Card> { kings.First(), queens.First() },
                        Points = isTrump ? 40 : 20
                    });
                }
            }

            // Check for 7 of Trump (single card)
            var sevenOfTrump = hand.FirstOrDefault(c => c.Suit == trumpSuit && c.Rank == Rank.Seven);
            if (sevenOfTrump != null)
            {
                possibleMelds.Add(new Meld
                {
                    Type = MeldType.TrumpSeven,
                    Cards = new List<Card> { sevenOfTrump },
                    Points = 10
                });
            }

            // Sort by points (highest first) and return
            return possibleMelds.OrderByDescending(m => m.Points).ToList();
        }

        /// <summary>
        /// Checks if the player has any valid melds available.
        /// </summary>
        public static bool HasAnyMeld(Player player, Suit trumpSuit)
        {
            return FindAllPossibleMelds(player, trumpSuit).Count > 0;
        }

        /// <summary>
        /// Gets the highest point meld available to the player.
        /// Returns null if no melds are available.
        /// </summary>
        public static Meld? GetBestMeld(Player player, Suit trumpSuit)
        {
            var melds = FindAllPossibleMelds(player, trumpSuit);
            return melds.FirstOrDefault();
        }
    }
}
