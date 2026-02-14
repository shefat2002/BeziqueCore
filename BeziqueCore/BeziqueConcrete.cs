using BeziqueCore.Interfaces;

namespace BeziqueCore;

public class BeziqueConcrete : IBeziqueAdapter
{
    public readonly List<byte> Dealer;
    public readonly List<byte>[] Player;
    public readonly int[] Scores;
    public byte DealOrder;
    public byte DealerIndex;
    public byte DrawOrder;
    public byte SetsDealtToCurrentPlayer;
    public const byte SetsPerPlayer = 3;
    public byte? TrumpCard;
    public byte? TrumpSuit;
    private MeldType _pendingMeld;
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

    public DeckCheckResult CheckDeckCardCount()
    {
        return Dealer.Count == 0 ? DeckCheckResult.Empty : DeckCheckResult.AvailableCards;
    }

    public DrawResult DrawCardFromDeck()
    {
        if (Dealer.Count == 0) return DrawResult.DrawComplete;

        Player[DrawOrder].Add(Dealer[0]);
        Dealer.RemoveAt(0);

        byte nextPlayer = (byte)((DrawOrder + 1) % Player.Length);
        bool moreToDraw = nextPlayer != 0;

        DrawOrder = nextPlayer;

        return moreToDraw ? DrawResult.MoreToDraw : DrawResult.DrawComplete;
    }

    public void AddMeldPoint()
    {
        int points = _pendingMeld switch
        {
            MeldType.Bezique => 40,
            MeldType.DoubleBezique => 500,
            MeldType.FourJacks => 40,
            MeldType.FourQueens => 60,
            MeldType.FourKings => 80,
            MeldType.FourAces => 100,
            MeldType.CommonMarriage => 20,
            MeldType.TrumpMarriage => 40,
            MeldType.TrumpRun => 150,
            _ => 0
        };
        Scores[DealOrder] += points;
    }

    public void SetPendingMeld(MeldType meldType)
    {
        _pendingMeld = meldType;
    }

    public void DetermineWinner()
    {
        int winnerIndex = -1;
        int highestScore = -1;

        for (int i = 0; i < Scores.Length; i++)
        {
            if (Scores[i] > highestScore)
            {
                highestScore = Scores[i];
                winnerIndex = i;
            }
        }

        DealOrder = (byte)winnerIndex;
    }

    public HandCheckResult CheckCardsOnHand()
    {
        return Player[DealOrder].Count == 0 ? HandCheckResult.Empty : HandCheckResult.MoreCards;
    }

    public void AllPlayerPlayed()
    {
        // Called by FSM when all players have played in a trick
        // Currently a no-op - logic handled by FSM state transitions
    }

    private void RotatePlayer()
    {
        DealOrder = (byte)((DealOrder + 1) % Player.Length);
    }
}
