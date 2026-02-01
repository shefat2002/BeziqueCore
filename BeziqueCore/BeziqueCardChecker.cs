namespace BeziqueCore;

static class BeziqueCardChecker
{
    public const int SuitCount = 4;
    public const int RankCount = 8;
    public const int JokerCount = 4;
    public const int CardCount = SuitCount * RankCount;

    private static readonly string[] Ranks = ["A", "7", "8", "9", "10", "J", "Q", "K"];
    private static readonly string[] Suits = ["Hearts", "Spades", "Clubs", "Diamonds"];
    private const string JokerName = "Joker";
    public const int JokerCardIndex = 32;

    public static bool TryCreateCard(int suitIndex, int rankIndex, out byte cardId)
    {
        if (suitIndex < 0 || suitIndex >= SuitCount || rankIndex < 0 || rankIndex >= RankCount)
        {
            cardId = default;
            return false;
        }
        cardId = (byte)(suitIndex * RankCount + rankIndex);
        return true;
    }

    public static bool TryGetSuitIndex(this byte cardId, out byte index)
    {
        if (cardId >= CardCount)
        {
            index = 0;
            return false;
        }
        index = (byte)(cardId / RankCount);
        return true;
    }

    public static bool TryGetRankIndex(this byte cardId, out byte index)
    {
        if (cardId >= CardCount)
        {
            index = 0;
            return false;
        }
        index = (byte)(cardId % RankCount);
        return true;
    }

    public static bool TryGetSuitName(int suitIndex, out string? name)
    {
        if (suitIndex < 0 || suitIndex >= SuitCount)
        {
            name = default;
            return false;
        }
        name = Suits[suitIndex];
        return true;
    }

    public static bool TryGetRankName(int rankIndex, out string? name)
    {
        if (rankIndex < 0 || rankIndex >= RankCount)
        {
            name = default;
            return false;
        }
        name = Ranks[rankIndex];
        return true;
    }

    public static bool TryGetCardName(this byte cardId, out string? name)
    {
        if (cardId == JokerCardIndex)
        {
            name = JokerName;
            return true;
        }

        if (!cardId.TryGetSuitIndex(out var suitIndex) || !cardId.TryGetRankIndex(out var rankIndex))
        {
            name = default;
            return false;
        }

        if (!TryGetRankName(rankIndex, out var rankName) || !TryGetSuitName(suitIndex, out var suitName))
        {
            name = default;
            return false;
        }

        name = $"{rankName} of {suitName}";
        return true;
    }

    public static bool TryCompareCards(this byte card1, byte card2, int trumpSuit, out int result)
    {
        if (card1 == JokerCardIndex)
        {
            result = 1;
            return true;
        }

        if (card2 == JokerCardIndex)
        {
            result = 0;
            return true;
        }

        if (!card1.TryGetSuitIndex(out var suit1) || !card1.TryGetRankIndex(out var rank1))
        {
            result = 0;
            return false;
        }

        if (!card2.TryGetSuitIndex(out var suit2) || !card2.TryGetRankIndex(out var rank2))
        {
            result = 0;
            return false;
        }

        if (suit1 == trumpSuit && suit2 != trumpSuit)
        {
            result = 1;
            return true;
        }

        if (suit2 == trumpSuit && suit1 != trumpSuit)
        {
            result = 0;
            return true;
        }

        if (suit1 == suit2)
        {
            var rankValues = new[] { 7, 0, 1, 2, 3, 4, 5, 6 }; // 7 is lowest in Bezique
            result = rankValues[rank1].CompareTo(rankValues[rank2]);
            return true;
        }

        result = 0;
        return true;
    }

    public static bool TryGetCardValue(this byte cardId, out int value)
    {
        if (cardId == JokerCardIndex)
        {
            value = 0;
            return true;
        }

        if (!cardId.TryGetRankIndex(out var rankIndex))
        {
            value = default;
            return false;
        }

        value = rankIndex switch
        {
            0 => 11, // A
            1 => 0,  // 7
            2 => 0,  // 8
            3 => 0,  // 9
            4 => 10, // 10
            5 => 2,  // J
            6 => 3,  // Q
            7 => 4,  // K
            _ => 0
        };
        return true;
    }

    // Is methods (keep as-is, already return bool)
    public static bool IsValidCard(byte cardId) => cardId <= JokerCardIndex;

    public static bool IsJoker(byte cardId) => cardId == JokerCardIndex;
}
