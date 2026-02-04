namespace BeziqueCore;

public readonly struct Card : IEquatable<Card>
{
    public byte CardId { get; }
    public sbyte DeckIndex { get; }

    public Card(byte cardId, sbyte deckIndex = -1)
    {
        CardId = cardId;
        DeckIndex = deckIndex;
    }

    public bool IsJoker => CardId == 32;
    public Suit Suit => IsJoker ? Suit.Diamonds : (Suit)(CardId % 4);
    public Rank Rank => IsJoker ? (Rank)15 : (Rank)(7 + CardId / 4);

    public bool Equals(Card other) => CardId == other.CardId && DeckIndex == other.DeckIndex;
    public override bool Equals(object obj) => obj is Card other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(CardId, DeckIndex);
    public static bool operator ==(Card left, Card right) => left.Equals(right);
    public static bool operator !=(Card left, Card right) => !left.Equals(right);
}
