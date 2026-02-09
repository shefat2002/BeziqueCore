namespace BeziqueCore;

// Use byte since we have 132 cards (fits in byte)
// Actually: [CCCCDDD] where C=card (0-32, needs 6 bits), D=deck (0-3, needs 2 bits)

public static class CardHelper
{
    public const byte JOKER = 32;
    public const byte CARDS_PER_DECK = 32;
    public const byte DECK_COUNT = 4;

    public static byte CreateCardId(byte cardValue, byte deckIndex)
    {
        return (byte)((deckIndex << 6) | cardValue);  // deck in upper 2 bits, card in lower
    }
    public static byte GetCardValue(byte cardId)
    {
        return (byte)(cardId & 0x3F);  // Lower 6 bits
    }
    
    public static byte GetDeckIndex(byte cardId)
    {
        return (byte)(cardId >> 6);  // Upper 2 bits
    }
    
    public static bool IsJoker(byte cardId)
    {
        return GetCardValue(cardId) == JOKER;
    }   
}