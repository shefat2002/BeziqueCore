using BeziqueCore.Interfaces;

namespace BeziqueCore;

public class BeziqueConcrete : IBeziqueAdapter
{
    public readonly List<byte> Dealer;
    public readonly List<byte>[] Player;
    public readonly int[] Scores; 
    public byte DealOrder;
    public byte DealerIndex; 
    public byte SetsDealtToCurrentPlayer;
    public const byte SetsPerPlayer = 3;  
    public byte? TrumpCard;  
    public byte? TrumpSuit;
    public const byte RANK_7_OFFSET = 0; 
    public const byte TRUMP_7_BONUS = 10; 

    public BeziqueConcrete(int player)
    {
        Player = new List<byte>[player];
        Scores = new int[player];
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
        if (Dealer.Count == 0) return; 

        TrumpCard = Dealer[0]; 
        TrumpSuit = CardHelper.GetDeckIndex(TrumpCard.Value);
        Dealer.RemoveAt(0);
    }

    public bool IsTrump7()
    {
        return TrumpCard.HasValue && CardHelper.GetCardValue(TrumpCard.Value) == RANK_7_OFFSET;
    }

    public void Add10PointsToDealer()
    {
        Scores[DealerIndex] += TRUMP_7_BONUS;
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