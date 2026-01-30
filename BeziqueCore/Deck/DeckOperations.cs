using BeziqueCore.Interfaces;
using BeziqueCore.Models;

namespace BeziqueCore.Deck
{
    /// <summary>
    /// Optimized deck operations using array-based storage for better performance.
    /// Array-based approach eliminates Stack<T> overhead and reduces allocations during shuffling.
    /// </summary>
    public class DeckOperations : IDeckOperations
    {
        // Array-based storage for O(1) random access and better cache locality
        private Card[] _deckArray;
        private int _deckIndex;
        private Card? _trumpCard;
        private const int TotalDecks = 4;
        private const int CardsPerDeck = 52; // Standard deck
        private const int JokersPerDeck = 1;
        private const int LastNineCardCount = 9;
        private const int TotalCards = TotalDecks * (CardsPerDeck + JokersPerDeck);
        private readonly Random _random;

        public DeckOperations()
        {
            _deckArray = Array.Empty<Card>();
            _deckIndex = 0;
            _random = new Random();
        }

        public void InitializeDeck()
        {
            // Pre-allocate exact size needed
            _deckArray = new Card[TotalCards];
            _deckIndex = 0;

            // Fill array directly - more efficient than Stack.Push
            for (int deck = 0; deck < TotalDecks; deck++)
            {
                foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                {
                    foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                    {
                        _deckArray[_deckIndex++] = Card.Create(suit, rank);
                    }
                }

                _deckArray[_deckIndex++] = Card.CreateJoker();
            }

            ShuffleDeck();
        }

        public Card? DrawTopCard()
        {
            if (_deckIndex <= 0)
            {
                return null;
            }

            return _deckArray[--_deckIndex];
        }

        public int GetRemainingCardCount()
        {
            return _deckIndex;
        }

        public bool IsLastNineCards()
        {
            return _deckIndex <= LastNineCardCount;
        }

        public Card? FlipTrumpCard()
        {
            _trumpCard = DrawTopCard();
            return _trumpCard;
        }

        public void ShuffleDeck()
        {
            // Fisher-Yates shuffle directly on array - no intermediate allocations
            // Start from current index to only shuffle remaining cards
            for (int i = _deckIndex - 1; i > 0; i--)
            {
                int j = _random.Next(0, i + 1);
                (_deckArray[i], _deckArray[j]) = (_deckArray[j], _deckArray[i]);
            }
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
