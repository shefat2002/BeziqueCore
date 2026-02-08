using BeziqueCore.Interfaces;

namespace BeziqueCore;

public class BeziqueConcruite : IBeziqueAdapter
{
    public readonly List<byte> Dealer;
    public readonly List<byte>[] Player;
    public byte DealOrder;

    public BeziqueConcruite(int player)
    {
        Player = new List<byte>[player];
        for (int i = 0; i < player; i++)
        {
            Player[i] = new List<byte>();
        }
        Dealer = new List<byte>();
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 32; j++)
            {
                Dealer.Add((byte)j);
            }
        }

        var joker = 32;
        for (int j = 0; j < 4; j++)
        {
            Dealer.Add((byte)joker);
        }
    }

    public void CheckPlayerCount()
    {
        throw new NotImplementedException();
    }

    public void DealThreeCards()
    {
        var range = Dealer.GetRange(0, 3);
        Dealer.RemoveRange(0, 3);
        Player[DealOrder].AddRange(range);
        RotatePlayer();
    }

    public void FlipCard()
    {
        throw new NotImplementedException();
    }

    public void Add10PointsToDealer()
    {
        throw new NotImplementedException();
    }
    
    
    
    

    private void RotatePlayer()
    {
        DealOrder = (byte)((DealOrder + 1) % Player.Length);
    }

}