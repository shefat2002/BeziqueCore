using BeziqueCore.Interfaces;

namespace BeziqueCore;

public class BeziqueGameController
{
    public Player[] Players { get; private set; }
    public GameContext Context { get; private set; }
    public List<Card> PlayedCards { get; private set; }

    private GameState _currentState = GameState.Play;
    public GameState CurrentState
    {
        get => _currentState;
        set => _currentState = value;
    }

    public byte PlayerCount => (byte)Players.Length;
    public ushort TargetScore { get; private set; }

    private readonly BeziqueAdapter _adapter;

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
        _currentState = GameState.Play;

        // Set first turn to player after dealer (player 1)
        Context.CurrentTurnPlayer = 1 % PlayerCount;
    }

    // Public API
    public bool PlayCard(Card card) => _adapter.PlayCard(card);

    public bool DeclareMeld(Card[] cards, MeldType meldType) => _adapter.DeclareMeld(cards, meldType);

    public void SkipMeld() => _adapter.SkipMeld();

    public void ResolveTrick() => _adapter.ResolveTrick();

    public void StartNewTrick() => _adapter.StartNewTrick();

    public int EndRound() => _adapter.EndRound();

    public int CheckWinner() => _adapter.CheckWinner();

    // Internal access for adapter
    internal void SetState(GameState state) => _currentState = state;
}
