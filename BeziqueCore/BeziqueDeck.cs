using BeziqueCore.Interfaces;
using SystemRandom = BeziqueCore.Interfaces.SystemRandom;

namespace BeziqueCore;

public static class BeziqueDeck
{
    public static byte[] CreateDeck()
    {
        var deck = new byte[BeziqueCardChecker.CardCount + BeziqueCardChecker.JokerCount];
        for (var i = 0; i < BeziqueCardChecker.CardCount; i++) deck[i] = (byte)i;
        for (var i = 0; i < BeziqueCardChecker.JokerCount; i++) deck[BeziqueCardChecker.CardCount + i] = BeziqueCardChecker.JokerCardIndex;
        return deck;
    }

    public static byte[] ShuffleDeck(byte[] deck, IRandom? random = null)
    {
        var rnd = random ?? new SystemRandom();
        var n = deck.Length;
        while (n > 1)
        {
            n--;
            var k = rnd.Next(n + 1);
            (deck[k], deck[n]) = (deck[n], deck[k]);
        }
        return deck;
    }
}
