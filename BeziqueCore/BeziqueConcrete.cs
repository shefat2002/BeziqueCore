using BeziqueCore.Interfaces;

namespace BeziqueCore;

public class BeziqueConcrete : IBeziqueAdapter
{
    public readonly List<byte> Dealer;
    public readonly List<byte>[] Player;
    public byte DealOrder;
    public byte SetsDealtToCurrentPlayer;  // Track sets dealt to current player (0, 1, 2)
    public const byte SetsPerPlayer = 3;   // 3 sets of 3 cards = 9 cards per player
    public byte? TrumpCard;  // The flipped trump card on table
    public const byte RANK_7_OFFSET = 0;  // Card value 0 is rank 7

    public BeziqueConcrete(int player)
    {
        Player = new List<byte>[player];
        for (int i = 0; i < player; i++)
        {
            Player[i] = new List<byte>();
        }
        Dealer = new List<byte>();
        for (int deck = 0; deck < 4; deck++)
        {
            for (int card = 0; card < 32; card++)
            {
                Dealer.Add(CardHelper.CreateCardId((byte)card, (byte)deck));
            }
            Dealer.Add(CardHelper.CreateCardId(CardHelper.JOKER, (byte)deck));
        }
    }


    public void DealThreeCards()
    {
        var range = Dealer.GetRange(0, 3);
        Dealer.RemoveRange(0, 3);
        Player[DealOrder].AddRange(range);

        RotatePlayer();

        if (DealOrder == 0)
        {
            SetsDealtToCurrentPlayer++;  
        }
    }

    public void FlipCard()
    {
        if (Dealer.Count == 0) return;  // No cards to flip

        TrumpCard = Dealer[0];  // Flip the top card (it stays visible on table/dealer deck)
        Dealer.RemoveAt(0);  // Remove from deck
    }

    public void Add10PointsToDealer()
    {
        throw new NotImplementedException();
    }

    public void CheckDeckCardCount()
    {
        throw new NotImplementedException();
    }

    public void DrawCardFromDeck()
    {
        throw new NotImplementedException();
    }

    public void AddMeldPoint()
    {
        throw new NotImplementedException();
    }

    public void AllPlayerPlayed()
    {
        throw new NotImplementedException();
    }

    public void DetermineWinner()
    {
        throw new NotImplementedException();
    }

    public void CheckCardsOnHand()
    {
        throw new NotImplementedException();
    }

    private void RotatePlayer()
    {
        DealOrder = (byte)((DealOrder + 1) % Player.Length);
    }

}