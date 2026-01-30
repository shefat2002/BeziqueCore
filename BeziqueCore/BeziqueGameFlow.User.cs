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

    public void DispatchContinueLastNine()
    {
        DispatchEvent(EventId.CONTINUELASTNINE);
    }

    public void DispatchDeckEmpty()
    {
        DispatchEvent(EventId.DECKEMPTY);
    }

    public void DispatchAllHandsEmpty()
    {
        DispatchEvent(EventId.ALLHANDSEMPTY);
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

    public void DispatchMorePlayersNeedToPlay()
    {
        DispatchEvent(EventId.MOREPLAYERSNEEDTOPLAY);
    }

    public void DispatchTrickComplete()
    {
        DispatchEvent(EventId.TRICKCOMPLETE);
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

    public void CheckAndDispatchL9TrickComplete()
    {
        if (gameAdapter.AreAllHandsEmpty())
        {
            // All players have no cards left - round ends
            DispatchEvent(EventId.ALLHANDSEMPTY);
        }
        else
        {
            // Players still have cards - continue last 9 cards phase
            DispatchEvent(EventId.CONTINUELASTNINE);
        }
    }

    public void CheckAndDispatchTrickComplete()
    {
        if (gameAdapter.IsTrickComplete())
        {
            // All players have played their cards - proceed to trick resolution
            DispatchEvent(EventId.TRICKCOMPLETE);
        }
        else
        {
            // Not all players have played yet - need another card play
            // Continue to next player's turn
            DispatchEvent(EventId.MOREPLAYERSNEEDTOPLAY);
        }
    }

    /// <summary>
    /// Checks if any player has reached the winning score after trick resolution.
    /// If so, dispatches WinningScoreReached event to transition to GAME_OVER.
    /// This ensures the game ends immediately when someone wins.
    /// </summary>
    public void CheckAndDispatchWinningScoreAfterTrick()
    {
        if (gameAdapter.HasPlayerReachedWinningScore())
        {
            DispatchEvent(EventId.WINNINGSCOREREACHED);
        }
        else
        {
            // No one has won yet, continue to meld opportunity
            DispatchEvent(EventId.TRICKRESOLVED);
        }
    }

    /// <summary>
    /// Checks if any player has reached the winning score after meld scoring.
    /// If so, dispatches WinningScoreReached event to transition to GAME_OVER.
    /// This ensures the game ends immediately when someone wins from meld points.
    /// </summary>
    public void CheckAndDispatchWinningScoreAfterMeld()
    {
        if (gameAdapter.HasPlayerReachedWinningScore())
        {
            DispatchEvent(EventId.WINNINGSCOREREACHED);
        }
        else
        {
            // No one has won yet, continue to card draw
            DispatchEvent(EventId.MELDSCORED);
        }
    }

    /// <summary>
    /// Checks if any player has reached the winning score during last 9 cards phase.
    /// If so, dispatches WinningScoreReached event to transition to GAME_OVER.
    /// </summary>
    public void CheckAndDispatchL9WinningScore()
    {
        if (gameAdapter.HasPlayerReachedWinningScore())
        {
            DispatchEvent(EventId.WINNINGSCOREREACHED);
        }
        else
        {
            // No one has won yet, continue last 9 cards phase
            DispatchEvent(EventId.TRICKRESOLVED);
        }
    }
}
