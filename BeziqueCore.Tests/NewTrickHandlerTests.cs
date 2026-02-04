using BeziqueCore;

public class NewTrickHandlerTests
{
    [Fact]
    public void StartNewTrick_ClearsPlayedCards()
    {
        var playedCards = new List<Card>
        {
            new Card((byte)0, 0),
            new Card((byte)1, 0)
        };
        int currentTurnPlayer = 1;

        NewTrickHandler.StartNewTrick(playedCards, 0, ref currentTurnPlayer);

        Assert.Empty(playedCards);
    }

    [Fact]
    public void StartNewTrick_SetsCurrentTurnToWinner()
    {
        var playedCards = new List<Card>();
        int currentTurnPlayer = 1;

        NewTrickHandler.StartNewTrick(playedCards, 3, ref currentTurnPlayer);

        Assert.Equal(3, currentTurnPlayer);
    }

    [Fact]
    public void StartNewTrick_WinnerZero_SetsTurnToZero()
    {
        var playedCards = new List<Card>();
        int currentTurnPlayer = 2;

        NewTrickHandler.StartNewTrick(playedCards, 0, ref currentTurnPlayer);

        Assert.Equal(0, currentTurnPlayer);
    }

    [Fact]
    public void StartNewTrick_MultiplePlayers_CorrectWinnerLeads()
    {
        var playedCards = new List<Card>
        {
            new Card((byte)20, 0),
            new Card((byte)21, 1),
            new Card((byte)22, 2),
            new Card((byte)23, 3)
        };
        int currentTurnPlayer = 0;

        NewTrickHandler.StartNewTrick(playedCards, 2, ref currentTurnPlayer);

        Assert.Empty(playedCards);
        Assert.Equal(2, currentTurnPlayer);
    }

    [Fact]
    public void StartNewTrick_EmptyPlayedCards_RemainsEmpty()
    {
        var playedCards = new List<Card>();
        int currentTurnPlayer = 0;

        NewTrickHandler.StartNewTrick(playedCards, 1, ref currentTurnPlayer);

        Assert.Empty(playedCards);
        Assert.Equal(1, currentTurnPlayer);
    }

    [Fact]
    public void StartNewTrick_SameWinnerAsCurrent_NoChangeInTurn()
    {
        var playedCards = new List<Card>
        {
            new Card((byte)0, 0)
        };
        int currentTurnPlayer = 1;

        NewTrickHandler.StartNewTrick(playedCards, 1, ref currentTurnPlayer);

        Assert.Empty(playedCards);
        Assert.Equal(1, currentTurnPlayer);
    }
}
