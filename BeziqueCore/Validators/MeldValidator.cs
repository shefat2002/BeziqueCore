using BeziqueCore.Interfaces;
using BeziqueCore.Models;
using System.Linq;

namespace BeziqueCore.Validators
{
    public class MeldValidator : IMeldValidator
    {
        private const int TrumpRunPoints = 250;
        private const int TrumpSevenPoints = 10;
        private const int TrumpMarriagePoints = 40;
        private const int MarriagePoints = 20;
        private const int FourAcesPoints = 100;
        private const int FourKingsPoints = 80;
        private const int FourQueensPoints = 60;
        private const int FourJacksPoints = 40;
        private const int BeziquePoints = 40;
        private const int DoubleBeziquePoints = 500;
        private const int FourOfAKindCount = 4;
        private const int TrumpRunCardCount = 5;
        private const int MarriageCardCount = 2;
        private const int SingleCardCount = 1;
        private const int DoubleBeziqueCardCount = 4;

        public bool IsValidMeld(Card[] cards, Suit trumpSuit)
        {
            if (cards == null || cards.Length == 0)
            {
                return false;
            }

            var meldType = DetermineMeldType(cards, trumpSuit);
            return meldType != MeldType.InvalidMeld;
        }

        public int CalculateMeldPoints(Meld meld)
        {
            return meld.Type switch
            {
                MeldType.TrumpRun => TrumpRunPoints,
                MeldType.TrumpSeven => TrumpSevenPoints,
                MeldType.TrumpMarriage => TrumpMarriagePoints,
                MeldType.Marriage => MarriagePoints,
                MeldType.FourAces => FourAcesPoints,
                MeldType.FourKings => FourKingsPoints,
                MeldType.FourQueens => FourQueensPoints,
                MeldType.FourJacks => FourJacksPoints,
                MeldType.Bezique => BeziquePoints,
                MeldType.DoubleBezique => DoubleBeziquePoints,
                _ => 0
            };
        }

        public bool CanPlaceSequence(Card[] cards, List<Meld> meldHistory, Suit trumpSuit)
        {
            bool hasTrumpMarriage = meldHistory.Any(m => m.Type == MeldType.TrumpMarriage);

            if (cards.Length == TrumpRunCardCount)
            {
                return true;
            }

            if (!hasTrumpMarriage)
            {
                return false;
            }

            bool aceOfTrumpScored = meldHistory.Any(m =>
                m.Type == MeldType.FourAces &&
                m.Cards.Any(c => c.Suit == trumpSuit && c.Rank == Rank.Ace));

            if (aceOfTrumpScored)
            {
            }

            return true;
        }

        public MeldType DetermineMeldType(Card[] cards, Suit trumpSuit)
        {
            if (cards == null || cards.Length == 0)
            {
                return MeldType.InvalidMeld;
            }

            if (cards.Length == FourOfAKindCount)
            {
                if (cards.All(c => c.Rank == Rank.Ace))
                {
                    return MeldType.FourAces;
                }
                if (cards.All(c => c.Rank == Rank.King))
                {
                    return MeldType.FourKings;
                }
                if (cards.All(c => c.Rank == Rank.Queen))
                {
                    return MeldType.FourQueens;
                }
                if (cards.All(c => c.Rank == Rank.Jack))
                {
                    return MeldType.FourJacks;
                }

                int spadeQueens = cards.Count(c => c.Suit == Suit.Spades && c.Rank == Rank.Queen);
                int diamondJacks = cards.Count(c => c.Suit == Suit.Diamonds && c.Rank == Rank.Jack);

                if (spadeQueens == 2 && diamondJacks == 2)
                {
                    return MeldType.DoubleBezique;
                }
            }

            if (cards.Length == TrumpRunCardCount &&
                cards.All(c => c.Suit == trumpSuit) &&
                cards.Any(c => c.Rank == Rank.Ace) &&
                cards.Any(c => c.Rank == Rank.Ten) &&
                cards.Any(c => c.Rank == Rank.King) &&
                cards.Any(c => c.Rank == Rank.Queen) &&
                cards.Any(c => c.Rank == Rank.Jack))
            {
                return MeldType.TrumpRun;
            }

            if (cards.Length == MarriageCardCount &&
                cards.All(c => c.Suit == trumpSuit) &&
                cards.Any(c => c.Rank == Rank.King) &&
                cards.Any(c => c.Rank == Rank.Queen))
            {
                return MeldType.TrumpMarriage;
            }

            if (cards.Length == MarriageCardCount &&
                !cards.Any(c => c.Suit == trumpSuit) &&
                cards[0].Suit == cards[1].Suit &&
                cards.Any(c => c.Rank == Rank.King) &&
                cards.Any(c => c.Rank == Rank.Queen))
            {
                return MeldType.Marriage;
            }

            if (cards.Length == SingleCardCount &&
                cards[0].Suit == trumpSuit &&
                cards[0].Rank == Rank.Seven)
            {
                return MeldType.TrumpSeven;
            }

            if (cards.Length == MarriageCardCount &&
                cards.Any(c => c.Suit == Suit.Spades && c.Rank == Rank.Queen) &&
                cards.Any(c => c.Suit == Suit.Diamonds && c.Rank == Rank.Jack))
            {
                return MeldType.Bezique;
            }

            return MeldType.InvalidMeld;
        }

        public Meld GetBestPossibleMeld(Card[] selectedCards, List<Card> playerHand, Suit trumpSuit)
        {
            if (selectedCards == null || selectedCards.Length == 0)
            {
                return null;
            }

            // Find all possible melds that can be made with the selected cards
            var possibleMelds = new List<Meld>();

            // Check for Double Bezique (4 cards: 2x Q♠ + 2x J♦)
            if (selectedCards.Length == 4)
            {
                var spadeQueens = selectedCards.Where(c => c.Suit == Suit.Spades && c.Rank == Rank.Queen).ToList();
                var diamondJacks = selectedCards.Where(c => c.Suit == Suit.Diamonds && c.Rank == Rank.Jack).ToList();

                if (spadeQueens.Count >= 2 && diamondJacks.Count >= 2)
                {
                    possibleMelds.Add(new Meld
                    {
                        Type = MeldType.DoubleBezique,
                        Cards = spadeQueens.Take(2).Concat(diamondJacks.Take(2)).ToList(),
                        Points = DoubleBeziquePoints
                    });
                }
            }

            // Check for Four of a Kind
            if (selectedCards.Length == 4)
            {
                var ranks = selectedCards.Select(c => c.Rank).Distinct().ToList();
                if (ranks.Count == 1)
                {
                    var rank = ranks.First();
                    MeldType meldType = rank switch
                    {
                        Rank.Ace => MeldType.FourAces,
                        Rank.King => MeldType.FourKings,
                        Rank.Queen => MeldType.FourQueens,
                        Rank.Jack => MeldType.FourJacks,
                        _ => MeldType.InvalidMeld
                    };

                    if (meldType != MeldType.InvalidMeld)
                    {
                        var points = CalculateMeldPoints(new Meld { Type = meldType });
                        possibleMelds.Add(new Meld
                        {
                            Type = meldType,
                            Cards = selectedCards.ToList(),
                            Points = points
                        });
                    }
                }
            }

            // Check for Trump Run (5 cards of trump suit: A, 10, K, Q, J)
            if (selectedCards.Length == 5)
            {
                if (selectedCards.All(c => c.Suit == trumpSuit) &&
                    selectedCards.Any(c => c.Rank == Rank.Ace) &&
                    selectedCards.Any(c => c.Rank == Rank.Ten) &&
                    selectedCards.Any(c => c.Rank == Rank.King) &&
                    selectedCards.Any(c => c.Rank == Rank.Queen) &&
                    selectedCards.Any(c => c.Rank == Rank.Jack))
                {
                    possibleMelds.Add(new Meld
                    {
                        Type = MeldType.TrumpRun,
                        Cards = selectedCards.ToList(),
                        Points = TrumpRunPoints
                    });
                }
            }

            // Check for marriages (2 cards: K + Q of same suit)
            if (selectedCards.Length == 2)
            {
                var hasKing = selectedCards.Any(c => c.Rank == Rank.King);
                var hasQueen = selectedCards.Any(c => c.Rank == Rank.Queen);

                if (hasKing && hasQueen)
                {
                    var suits = selectedCards.Select(c => c.Suit).Distinct().ToList();
                    if (suits.Count == 1)
                    {
                        var suit = suits.First();

                        // Trump Marriage
                        if (suit == trumpSuit)
                        {
                            possibleMelds.Add(new Meld
                            {
                                Type = MeldType.TrumpMarriage,
                                Cards = selectedCards.ToList(),
                                Points = TrumpMarriagePoints
                            });
                        }
                        else
                        {
                            // Regular Marriage
                            possibleMelds.Add(new Meld
                            {
                                Type = MeldType.Marriage,
                                Cards = selectedCards.ToList(),
                                Points = MarriagePoints
                            });
                        }
                    }
                }

                // Check for Bezique (Q♠ + J♦)
                if (selectedCards.Any(c => c.Suit == Suit.Spades && c.Rank == Rank.Queen) &&
                    selectedCards.Any(c => c.Suit == Suit.Diamonds && c.Rank == Rank.Jack))
                {
                    possibleMelds.Add(new Meld
                    {
                        Type = MeldType.Bezique,
                        Cards = selectedCards.ToList(),
                        Points = BeziquePoints
                    });
                }
            }

            // Check for 7 of Trump (single card)
            if (selectedCards.Length == 1 &&
                selectedCards[0].Suit == trumpSuit &&
                selectedCards[0].Rank == Rank.Seven)
            {
                possibleMelds.Add(new Meld
                {
                    Type = MeldType.TrumpSeven,
                    Cards = selectedCards.ToList(),
                    Points = TrumpSevenPoints
                });
            }

            // If no melds found, return null
            if (possibleMelds.Count == 0)
            {
                return null;
            }

            // Return the meld with the highest points
            return possibleMelds.OrderByDescending(m => m.Points).First();
        }
    }
}
