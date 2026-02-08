using BeziqueCore.Interfaces;

namespace BeziqueCore;

public class BaBa : IBeziqueAdapter
{
    public readonly List<byte> Dealer;
    public readonly List<byte>[] Player;
    public byte DealOrder;

    public BaBa(int player)
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

    public void DealFirstSet()
    {
        var range = Dealer.GetRange(0, 3);
        Dealer.RemoveRange(0, 3);
        Player[DealOrder].AddRange(range);
        RotatePlayer();
    }

    public void DealMidSet()
    {
        var range = Dealer.GetRange(0, 3);
        Dealer.RemoveRange(0, 3);
        Player[DealOrder].AddRange(range);
        RotatePlayer();
    }

    private void RotatePlayer()
    {
        DealOrder = (byte)((DealOrder + 1) % Player.Length);
    }

    public void DealLastSet()
    {
        throw new NotImplementedException();
    }

    public void SelectTrump()
    {
        throw new NotImplementedException();
    }

    public void PlayFirstCard()
    {
        throw new NotImplementedException();
    }

    public void PlayMidCard()
    {
        throw new NotImplementedException();
    }

    public void PlayLastCard()
    {
        throw new NotImplementedException();
    }

    public void TryMeld()
    {
        throw new NotImplementedException();
    }

    public void MeldSuccess()
    {
        throw new NotImplementedException();
    }

    public void MeldFailed()
    {
        throw new NotImplementedException();
    }

    public void StartNewTrick()
    {
        throw new NotImplementedException();
    }

    public void DrawCardsForAll()
    {
        throw new NotImplementedException();
    }

    public void L9PlayFirstCard()
    {
        throw new NotImplementedException();
    }

    public void L9PlayMidCard()
    {
        throw new NotImplementedException();
    }

    public void L9PlayLastCard()
    {
        throw new NotImplementedException();
    }

    public void StartL9NewTrick()
    {
        throw new NotImplementedException();
    }

    public void EndRound()
    {
        throw new NotImplementedException();
    }

    public void CalculatePoints()
    {
        throw new NotImplementedException();
    }

    public void GameOver()
    {
        throw new NotImplementedException();
    }
}