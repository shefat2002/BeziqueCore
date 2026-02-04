using BeziqueCore.Interfaces;

namespace BeziqueCore;

static class DeckFactory
{
    private const byte StandardCardCount = 32;
    private const byte JokerCardId = 32;

    public static Card[] CreateDeck(byte deckCount = 4)
    {
        var cards = new List<Card>(StandardCardCount * deckCount + deckCount);

        for (sbyte deckIndex = 0; deckIndex < deckCount; deckIndex++)
        {
            for (byte cardId = 0; cardId < StandardCardCount; cardId++)
            {
                cards.Add(new Card(cardId, deckIndex));
            }

            cards.Add(new Card(JokerCardId, deckIndex));
        }

        return cards.ToArray();
    }

    public static Card[] Shuffled(IRandom random, byte deckCount = 4)
    {
        var deck = CreateDeck(deckCount);

        for (int i = deck.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (deck[i], deck[j]) = (deck[j], deck[i]);
        }

        return deck;
    }

    public static Card CreateCard(byte cardId, sbyte deckIndex = -1) => new(cardId, deckIndex);

    public static Card CreateJoker(sbyte deckIndex = -1) => new(JokerCardId, deckIndex);
}
