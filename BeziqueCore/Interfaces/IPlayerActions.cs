namespace BeziqueCore.Models
{
    public class Card : IEquatable<Card>
    {
        public Suit Suit { get; set; }
        public Rank Rank { get; set; }
        public bool IsJoker { get; set; }

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

        public static bool operator ==(Card? left, Card? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(Card? left, Card? right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return IsJoker ? "Joker" : $"{Rank} of {Suit}";
        }
    }

    public enum Suit
    {
        Hearts,
        Diamonds,
        Clubs,
        Spades
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
        public List<Meld> DeclaredMelds { get; set; }
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
