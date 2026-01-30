namespace BeziqueCore.Models
{
    /// <summary>
    /// Represents a playing card with value equality and comparison support.
    /// Card is immutable for thread safety and predictability.
    /// </summary>
    public sealed class Card : IEquatable<Card>, IComparable<Card>
    {
        public Suit Suit { get; }
        public Rank Rank { get; }
        public bool IsJoker { get; }

        // Private constructor for normal cards
        private Card(Suit suit, Rank rank, bool isJoker)
        {
            Suit = suit;
            Rank = rank;
            IsJoker = isJoker;
        }

        /// <summary>
        /// Creates a normal playing card.
        /// </summary>
        public static Card Create(Suit suit, Rank rank)
        {
            return new Card(suit, rank, false);
        }

        /// <summary>
        /// Creates a Joker card. Suit and rank are ignored for jokers.
        /// </summary>
        public static Card CreateJoker(Suit suit = Suit.Spades, Rank rank = Rank.Ace)
        {
            return new Card(suit, rank, true);
        }

        public bool Equals(Card? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Suit == other.Suit && Rank == other.Rank && IsJoker == other.IsJoker;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Card);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Suit, Rank, IsJoker);
        }

        public int CompareTo(Card? other)
        {
            if (other is null) return 1;
            if (IsJoker && !other.IsJoker) return 1;
            if (!IsJoker && other.IsJoker) return -1;
            if (IsJoker && other.IsJoker) return 0;

            var rankComparison = Rank.CompareTo(other.Rank);
            return rankComparison != 0 ? rankComparison : Suit.CompareTo(other.Suit);
        }

        public static bool operator ==(Card? left, Card? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(Card? left, Card? right)
        {
            return !(left == right);
        }

        public static bool operator <(Card? left, Card? right)
        {
            return left is null ? right is not null : left.CompareTo(right) < 0;
        }

        public static bool operator >(Card? left, Card? right)
        {
            return left is null ? false : left.CompareTo(right) > 0;
        }

        public override string ToString()
        {
            return IsJoker ? "Joker" : $"{Rank} of {Suit}";
        }
    }

    public enum Suit
    {
        Clubs,
        Diamonds,
        Spades,
        Hearts
    }

    public enum Rank
    {
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Jack = 10,
        Queen = 11,
        King = 12,
        Ten = 13,
        Ace = 14
    }
}

namespace BeziqueCore.Interfaces
{
    using BeziqueCore.Models;

    public interface IPlayerActions
    {
        void PlayCard(Player player, Card card);
        void DeclareMeld(Player player, Meld meld);
        void DrawCard(Player player);
        void SwitchSevenOfTrump(Player player);
        void SkipMeld(Player player);
    }

    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
        public List<Card> Hand { get; set; }
        public bool IsDealer { get; set; }
        public bool IsBot { get; set; }
        public List<Meld> DeclaredMelds { get; set; }
        /// <summary>
        /// Tracks cards that have been used in melds and cannot be used again.
        /// A card can only be part of one meld declaration.
        /// </summary>
        public List<Card> MeldedCards { get; set; }
    }

    public class Meld
    {
        public MeldType Type { get; set; }
        public List<Card> Cards { get; set; }
        public int Points { get; set; }
    }

    public enum MeldType
    {
        TrumpRun,
        TrumpSeven,
        TrumpMarriage,
        Marriage,
        FourAces,
        FourKings,
        FourQueens,
        FourJacks,
        Bezique,
        DoubleBezique,
        InvalidMeld
    }
}
