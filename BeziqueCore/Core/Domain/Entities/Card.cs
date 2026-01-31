namespace BeziqueCore.Core.Domain.Entities;

public sealed class Card : IEquatable<Card>, IComparable<Card>
{
    public Suit Suit { get; }
    public Rank Rank { get; }
    public bool IsJoker { get; }

    private Card(Suit suit, Rank rank, bool isJoker)
    {
        Suit = suit;
        Rank = rank;
        IsJoker = isJoker;
    }

    public static Card Create(Suit suit, Rank rank)
    {
        return new Card(suit, rank, false);
    }

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
