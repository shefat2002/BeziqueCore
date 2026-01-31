using BeziqueCore.Interfaces;
using BeziqueCore.Models;
using BeziqueCore.Constants;
using System.Linq;

namespace BeziqueCore.Helpers
{
    public static class MeldHelper
    {
        // Pre-allocated reusable arrays to reduce allocations
        private static readonly Card[] _cardBuffer = new Card[8];
        private static readonly Suit[] _allSuits = Enum.GetValues(typeof(Suit)).Cast<Suit>().ToArray();

        public static List<Meld> FindAllPossibleMelds(Player player, Suit trumpSuit)
        {
            var possibleMelds = new List<Meld>(8);
            var hand = player.Hand;
            int handCount = hand.Count;

            // Pre-compute card counts using efficient array-based approach
            var cardCounts = ComputeCardCounts(hand);

            // Check for Double Bezique (4 cards: 2x Q♠ + 2x J♦)
            int spadeQueenCount = GetCardCount(cardCounts, Suit.Spades, Rank.Queen);
            int diamondJackCount = GetCardCount(cardCounts, Suit.Diamonds, Rank.Jack);

            if (spadeQueenCount >= 2 && diamondJackCount >= 2)
            {
                int bufferIndex = 0;
                CollectCards(hand, _cardBuffer, ref bufferIndex, Suit.Spades, Rank.Queen, 2);
                CollectCards(hand, _cardBuffer, ref bufferIndex, Suit.Diamonds, Rank.Jack, 2);

                possibleMelds.Add(new Meld
                {
                    Type = MeldType.DoubleBezique,
                    Cards = _cardBuffer.Take(4).ToList(),
                    Points = GameConstants.DoubleBeziquePoints
                });
            }
            else if (spadeQueenCount >= 1 && diamondJackCount >= 1)
            {
                var spadeQueen = FindCard(hand, Suit.Spades, Rank.Queen);
                var diamondJack = FindCard(hand, Suit.Diamonds, Rank.Jack);
                if (spadeQueen != null && diamondJack != null)
                {
                    possibleMelds.Add(new Meld
                    {
                        Type = MeldType.Bezique,
                        Cards = new List<Card> { spadeQueen, diamondJack },
                        Points = GameConstants.BeziquePoints
                    });
                }
            }

            // Check for Four of a Kind (with Joker substitution)
            CheckFourOfAKindMelds(hand, possibleMelds, cardCounts);

            // Check for Trump Run (5 cards of trump suit: A, 10, K, Q, J)
            CheckTrumpRunMeld(hand, trumpSuit, possibleMelds, cardCounts);

            // Check for marriages (2 cards: K + Q of same suit)
            CheckMarriageMelds(hand, trumpSuit, possibleMelds, cardCounts);

            // Check for 7 of Trump (single card)
            var sevenOfTrump = FindCard(hand, trumpSuit, Rank.Seven);
            if (sevenOfTrump != null)
            {
                possibleMelds.Add(new Meld
                {
                    Type = MeldType.TrumpSeven,
                    Cards = new List<Card> { sevenOfTrump },
                    Points = GameConstants.SevenOfTrumpBonus
                });
            }

            // Sort by points (highest first) and return
            possibleMelds.Sort((a, b) => b.Points.CompareTo(a.Points));
            return possibleMelds;
        }

        private static int[,] ComputeCardCounts(List<Card> hand)
        {
            var counts = new int[4, 15]; // 4 suits, max rank 14 (Ace)

            foreach (var card in hand)
            {
                if (!card.IsJoker)
                {
                    counts[(int)card.Suit, (int)card.Rank]++;
                }
            }

            return counts;
        }

        private static int GetCardCount(int[,] counts, Suit suit, Rank rank)
        {
            return counts[(int)suit, (int)rank];
        }

        private static Card? FindCard(List<Card> hand, Suit suit, Rank rank)
        {
            for (int i = 0; i < hand.Count; i++)
            {
                var card = hand[i];
                if (card.Suit == suit && card.Rank == rank && !card.IsJoker)
                    return card;
            }
            return null;
        }

        private static void CollectCards(List<Card> hand, Card[] buffer, ref int index, Suit suit, Rank rank, int count)
        {
            int collected = 0;
            for (int i = 0; i < hand.Count && collected < count; i++)
            {
                var card = hand[i];
                if (card.Suit == suit && card.Rank == rank && !card.IsJoker)
                {
                    buffer[index++] = card;
                    collected++;
                }
            }
        }

        private static void CheckFourOfAKindMelds(List<Card> hand, List<Meld> possibleMelds, int[,] cardCounts)
        {
            int jokerCount = hand.Count(c => c.IsJoker);

            var fourOfAKindRanks = new[]
            {
                (Rank.Ace, MeldType.FourAces, GameConstants.FourAcesPoints),
                (Rank.King, MeldType.FourKings, GameConstants.FourKingsPoints),
                (Rank.Queen, MeldType.FourQueens, GameConstants.FourQueensPoints),
                (Rank.Jack, MeldType.FourJacks, GameConstants.FourJacksPoints)
            };

            foreach (var (rank, meldType, points) in fourOfAKindRanks)
            {
                int rankCount = 0;
                // Sum across all suits for this rank
                for (int suit = 0; suit < 4; suit++)
                {
                    rankCount += cardCounts[suit, (int)rank];
                }

                int totalCards = rankCount + jokerCount;

                if (totalCards >= 4)
                {
                    int bufferIndex = 0;
                    int cardsNeeded = 4;

                    // Collect non-joker cards first
                    for (int suit = 0; suit < 4 && cardsNeeded > 0; suit++)
                    {
                        int count = cardCounts[suit, (int)rank];
                        for (int i = 0; i < count && cardsNeeded > 0; i++)
                        {
                            _cardBuffer[bufferIndex++] = FindCard(hand, (Suit)suit, rank);
                            cardsNeeded--;
                        }
                    }

                    // Add jokers if needed
                    if (cardsNeeded > 0)
                    {
                        foreach (var card in hand)
                        {
                            if (card.IsJoker && cardsNeeded > 0)
                            {
                                _cardBuffer[bufferIndex++] = card;
                                cardsNeeded--;
                            }
                        }
                    }

                    possibleMelds.Add(new Meld
                    {
                        Type = meldType,
                        Cards = _cardBuffer.Take(4).ToList(),
                        Points = points
                    });
                }
            }
        }

        private static void CheckTrumpRunMeld(List<Card> hand, Suit trumpSuit, List<Meld> possibleMelds, int[,] cardCounts)
        {
            var requiredRanks = new[] { Rank.Ace, Rank.Ten, Rank.King, Rank.Queen, Rank.Jack };
            bool hasAllRequired = true;

            foreach (var rank in requiredRanks)
            {
                if (GetCardCount(cardCounts, trumpSuit, rank) == 0)
                {
                    hasAllRequired = false;
                    break;
                }
            }

            if (hasAllRequired)
            {
                int bufferIndex = 0;
                foreach (var rank in requiredRanks)
                {
                    _cardBuffer[bufferIndex++] = FindCard(hand, trumpSuit, rank);
                }

                possibleMelds.Add(new Meld
                {
                    Type = MeldType.TrumpRun,
                    Cards = _cardBuffer.Take(5).ToList(),
                    Points = GameConstants.TrumpRunPoints
                });
            }
        }

        private static void CheckMarriageMelds(List<Card> hand, Suit trumpSuit, List<Meld> possibleMelds, int[,] cardCounts)
        {
            foreach (var suit in _allSuits)
            {
                bool hasKing = GetCardCount(cardCounts, suit, Rank.King) > 0;
                bool hasQueen = GetCardCount(cardCounts, suit, Rank.Queen) > 0;

                if (hasKing && hasQueen)
                {
                    var king = FindCard(hand, suit, Rank.King);
                    var queen = FindCard(hand, suit, Rank.Queen);
                    if (king != null && queen != null)
                    {
                        bool isTrump = suit == trumpSuit;

                        possibleMelds.Add(new Meld
                        {
                            Type = isTrump ? MeldType.TrumpMarriage : MeldType.Marriage,
                            Cards = new List<Card> { king, queen },
                            Points = isTrump ? GameConstants.TrumpMarriagePoints : GameConstants.MarriagePoints
                        });
                    }
                }
            }
        }

        public static bool HasAnyMeld(Player player, Suit trumpSuit)
        {
            // Early exit optimizations
            if (player.Hand.Count < 2)
                return false;

            return FindAllPossibleMelds(player, trumpSuit).Count > 0;
        }

        public static Meld? GetBestMeld(Player player, Suit trumpSuit)
        {
            var melds = FindAllPossibleMelds(player, trumpSuit);
            return melds.Count > 0 ? melds[0] : null;
        }
    }
}
