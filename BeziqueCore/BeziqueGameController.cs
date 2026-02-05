using BeziqueCore.Interfaces;

namespace BeziqueCore;

public class BeziqueGameController
{
    public Player[] Players { get; private set; }
    public GameContext Context { get; private set; }
    public List<Card> PlayedCards { get; private set; }

    public byte PlayerCount => (byte)Players.Length;
    public ushort TargetScore { get; private set; }

    private readonly BeziqueAdapter _adapter;

    public event EventHandler<TrickEndedEventArgs>? TrickEnded;
    public event EventHandler<PhaseChangedEventArgs>? PhaseChanged;
    public event EventHandler<MeldDeclaredEventArgs>? MeldDeclared;
    public event EventHandler<RoundEndedEventArgs>? RoundEnded;
    public event EventHandler<GameEndedEventArgs>? GameEnded;

    public BeziqueGameController()
    {
        _adapter = new BeziqueAdapter(this);
    }

    public void Initialize(GameConfig config)
    {
        TargetScore = config.TargetScore;
        GameInitializer.InitializeGame(config, out var players, out var context);
        Players = players;
        Context = context;
        PlayedCards = new List<Card>();

        Context.CurrentTurnPlayer = 0;

        // Start the state machine - it will manage all state transitions
        _adapter.StartStateMachine();
    }

    // Core Game Actions
    public bool PlayCard(Card card) => _adapter.PlayCard(card);

    public bool DeclareMeld(Card[] cards, MeldType meldType) => _adapter.DeclareMeld(cards, meldType);

    public bool CanSwapTrumpSeven() => _adapter.CanSwapTrumpSeven();

    public bool SwapTrumpSeven() => _adapter.SwapTrumpSeven();

    public void SkipMeld() => _adapter.SkipMeld();

    public int EndRound() => _adapter.EndRound();

    public int CheckWinner() => _adapter.CheckWinner();

    // Game Flow Methods
    public void DrawCards()
    {
        _adapter.DrawCards();
    }

    public bool CanMeld() => _adapter.CanMeld();

    public bool CheckPhaseTransition() => _adapter.CheckPhaseTransition();

    public Card[] GetLegalMoves() => _adapter.GetLegalMoves();

    public MeldOpportunity? GetBestMeld() => _adapter.GetBestMeld();

    // Query Helper Methods
    public bool IsPlayerTurn(int playerId) => Context.CurrentTurnPlayer == playerId;

    public bool IsPhase2 => Context.CurrentPhase == GamePhase.Phase2_Last9;

    public bool IsGameOver => CheckWinner() >= 0;

    public int CurrentPlayer => Context.CurrentTurnPlayer;

    public int LastWinner => Context.LastTrickWinner;

    /// <summary>
    /// Maps the FSM state ID to the legacy GameState enum.
    /// This provides backward compatibility while using FSM as the single source of truth.
    /// </summary>
    public GameState CurrentState => MapFsmState(_adapter.CurrentFsmStateId);

    private GameState MapFsmState(Bezique.StateId id)
    {
        return id switch
        {
            // Deal states
            Bezique.StateId.DEAL => GameState.Deal,
            Bezique.StateId.DEALFIRST => GameState.Deal,
            Bezique.StateId.DEALMID => GameState.Deal,
            Bezique.StateId.DEALLAST => GameState.Deal,
            Bezique.StateId.SELECTTRUMP => GameState.Deal,

            // Play states (Phase 1)
            Bezique.StateId.PLAY => GameState.Play,
            Bezique.StateId.PLAYFIRST => GameState.Play,
            Bezique.StateId.PLAYMID => GameState.Play,
            Bezique.StateId.PLAYLAST => GameState.Play,

            // Meld states
            Bezique.StateId.MELD => GameState.Meld,
            Bezique.StateId.TRYMELDED => GameState.Meld,
            Bezique.StateId.MELDED => GameState.Meld,
            Bezique.StateId.NOMELD => GameState.Meld,

            // New Trick and Draw states
            Bezique.StateId.NEWTRICK => GameState.NewTrick,
            Bezique.StateId.ADDONECARDTOALL => GameState.NewTrick,

            // Phase 2 (Last 9 cards) states
            Bezique.StateId.L9PLAY => GameState.L9Play,
            Bezique.StateId.L9PLAYFIRST => GameState.L9Play,
            Bezique.StateId.L9PLAYMID => GameState.L9Play,
            Bezique.StateId.L9PLAYLAST => GameState.L9Play,
            Bezique.StateId.L9NEWTRICK => GameState.L9NewTrick,

            // Round End states
            Bezique.StateId.ROUNDEND => GameState.RoundEnd,
            Bezique.StateId.CALCULATEPOINT => GameState.RoundEnd,

            // Game Over
            Bezique.StateId.GAMEOVER => GameState.GameOver,

            _ => GameState.Play // Default fallback
        };
    }

    internal void OnTrickEnded(int winnerId, bool isFinalTrick)
    {
        TrickEnded?.Invoke(this, new TrickEndedEventArgs(winnerId, isFinalTrick));
    }

    internal void OnPhaseChanged(GamePhase newPhase)
    {
        PhaseChanged?.Invoke(this, new PhaseChangedEventArgs(newPhase));
    }

    internal void OnMeldDeclared(int playerId, MeldType meldType, int points)
    {
        MeldDeclared?.Invoke(this, new MeldDeclaredEventArgs(playerId, meldType, points));
    }

    internal void OnRoundEnded(int winnerId, int[] scores)
    {
        RoundEnded?.Invoke(this, new RoundEndedEventArgs(winnerId, scores));
    }

    internal void OnGameEnded(int winnerId)
    {
        GameEnded?.Invoke(this, new GameEndedEventArgs(winnerId));
    }
}

// Event Args Classes
public class TrickEndedEventArgs : EventArgs
{
    public int WinnerId { get; }
    public bool IsFinalTrick { get; }

    public TrickEndedEventArgs(int winnerId, bool isFinalTrick)
    {
        WinnerId = winnerId;
        IsFinalTrick = isFinalTrick;
    }
}

public class PhaseChangedEventArgs : EventArgs
{
    public GamePhase NewPhase { get; }

    public PhaseChangedEventArgs(GamePhase newPhase)
    {
        NewPhase = newPhase;
    }
}

public class MeldDeclaredEventArgs : EventArgs
{
    public int PlayerId { get; }
    public MeldType MeldType { get; }
    public int Points { get; }

    public MeldDeclaredEventArgs(int playerId, MeldType meldType, int points)
    {
        PlayerId = playerId;
        MeldType = meldType;
        Points = points;
    }
}

public class RoundEndedEventArgs : EventArgs
{
    public int WinnerId { get; }
    public int[] Scores { get; }

    public RoundEndedEventArgs(int winnerId, int[] scores)
    {
        WinnerId = winnerId;
        Scores = scores;
    }
}

public class GameEndedEventArgs : EventArgs
{
    public int WinnerId { get; }

    public GameEndedEventArgs(int winnerId)
    {
        WinnerId = winnerId;
    }
}
