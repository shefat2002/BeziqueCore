using BeziqueCore.Adapters;
using BeziqueCore.Interfaces;

public partial class BeziqueGameFlow
{
    protected IGameAdapter gameAdapter;

    public BeziqueGameFlow(IGameAdapter adapter)
    {
        gameAdapter = adapter;
    }

    public void DispatchGameInitialized()
    {
        DispatchEvent(EventId.INITIALIZED);
    }

    public void DispatchCardsDealt()
    {
        DispatchEvent(EventId.CARDSDEALT);
    }

    public void DispatchTrumpDetermined()
    {
        DispatchEvent(EventId.TRUMPDETERMINED);
    }

    public void DispatchCardPlayed()
    {
        DispatchEvent(EventId.CARDPLAYED);
    }

    public void DispatchAllPlayersResponded()
    {
        DispatchEvent(EventId.ALLPLAYERSRESPONDED);
    }

    public void DispatchTrickResolved()
    {
        DispatchEvent(EventId.TRICKRESOLVED);
    }

    public void DispatchMeldDeclared()
    {
        DispatchEvent(EventId.MELDDECLARED);
    }

    public void DispatchMeldSkipped()
    {
        DispatchEvent(EventId.MELDSKIPPED);
    }

    public void DispatchMeldScored()
    {
        DispatchEvent(EventId.MELDSCORED);
    }

    public void DispatchCardsDrawn()
    {
        DispatchEvent(EventId.CARDSDRAWN);
    }

    public void DispatchMoreCardsAvailable()
    {
        DispatchEvent(EventId.MORECARDSAVAILABLE);
    }

    public void DispatchLastNineReached()
    {
        DispatchEvent(EventId.LASTNINEREACHED);
    }

    public void DispatchFinalTrickResolved()
    {
        DispatchEvent(EventId.FINALTRICKRESOLVED);
    }

    public void DispatchContinueLastNine()
    {
        DispatchEvent(EventId.CONTINUELASTNINE);
    }

    public void DispatchDeckEmpty()
    {
        DispatchEvent(EventId.DECKEMPTY);
    }

    public void DispatchContinueGame()
    {
        DispatchEvent(EventId.CONTINUEGAME);
    }

    public void DispatchWinningScoreReached()
    {
        DispatchEvent(EventId.WINNINGSCOREREACHED);
    }

    public void DispatchTimerExpired()
    {
        DispatchEvent(EventId.TIMEREXPIRED);
    }

    public void DispatchTimerReset()
    {
        DispatchEvent(EventId.TIMERRESET);
    }

    public void CheckAndDispatchLastNineReached()
    {
        if (gameAdapter.IsLastNineCardsPhase())
        {
            DispatchEvent(EventId.LASTNINEREACHED);
        }
        else
        {
            DispatchEvent(EventId.MORECARDSAVAILABLE);
        }
    }

    public void CheckAndDispatchDeckEmpty()
    {
        if (gameAdapter.IsDeckEmpty())
        {
            DispatchEvent(EventId.DECKEMPTY);
        }
        else
        {
            DispatchEvent(EventId.MORECARDSAVAILABLE);
        }
    }
}
