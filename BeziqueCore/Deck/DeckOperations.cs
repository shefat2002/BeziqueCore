using BeziqueCore.Interfaces;
using BeziqueCore.Models;

namespace BeziqueCore.Deck
{
    public class DeckOperations : IDeckOperations
    {
        private Stack<Card> _deck;
        private Card? _trumpCard;
        private const int TotalDecks = 4;
        private const int LastNineCardCount = 9;
        private readonly Random _random;

        public DeckOperations()
        {
            _deck = new Stack<Card>();
            _random = new Random();
        }

        public void InitializeDeck()
        {
            _deck.Clear();

            for (int deck = 0; deck < TotalDecks; deck++)
            {
                foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                {
                    foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                    {
                        _deck.Push(new Card
                        {
                            Suit = suit,
                            Rank = rank,
                            IsJoker = false
                        });
                    }
                }

                _deck.Push(new Card
                {
                    Suit = Suit.Spades,
                    Rank = Rank.Ace,
                    IsJoker = true
                });
            }

            ShuffleDeck();
        }

        public Card? DrawTopCard()
        {
            if (_deck.Count == 0)
            {
                return null;
            }

            return _deck.Pop();
        }

        public int GetRemainingCardCount()
        {
            return _deck.Count;
        }

        public bool IsLastNineCards()
        {
            return _deck.Count <= LastNineCardCount;
        }

        public Card? FlipTrumpCard()
        {
            _trumpCard = DrawTopCard();
            return _trumpCard;
        }

        public void ShuffleDeck()
        {
            var cards = _deck.ToList();

            for (int i = cards.Count - 1; i > 0; i--)
            {
                int j = _random.Next(0, i + 1);
                (cards[i], cards[j]) = (cards[j], cards[i]);
            }

            _deck = new Stack<Card>(cards);
        }

        public Card? GetTrumpCard()
        {
            return _trumpCard;
        }

        public Card? TakeTrumpCard()
        {
            var card = _trumpCard;
            _trumpCard = null;
            return card;
        }
    }
}
