using BeziqueCore;
using BeziqueCLI.UI;

namespace BeziqueCLI;

/// <summary>
/// Main game controller that orchestrates the Bezique card game
/// </summary>
public class BeziqueGame
{
    private readonly BeziqueGameController _controller;
    private readonly GameRenderer _renderer;
    private readonly PlayerInputHandler _inputHandler;

    public BeziqueGame(GameRenderer renderer)
    {
        _controller = new BeziqueGameController();
        _renderer = renderer;
        _inputHandler = new PlayerInputHandler();

        // Subscribe to game events
        _controller.TrickEnded += OnTrickEnded!;
        _controller.PhaseChanged += OnPhaseChanged!;
        _controller.MeldDeclared += OnMeldDeclared!;
        _controller.RoundEnded += OnRoundEnded!;
        _controller.GameEnded += OnGameEnded!;
    }

    public void Start()
    {
        // Get game configuration from user
        var config = GetGameConfiguration();

        // Initialize game
        _controller.Initialize(config);
        _renderer.ShowGameStart(_controller);

        // Main game loop
        while (!_controller.IsGameOver)
        {
            PlayTurn();
        }

        _renderer.ShowGameOver(_controller);
    }

    private GameConfig GetGameConfiguration()
    {
        _renderer.ShowConfigHeader();

        int playerCount = _inputHandler.GetPlayerCount();
        var mode = _inputHandler.GetGameMode();
        ushort targetScore = _inputHandler.GetTargetScore();

        return new GameConfig
        {
            PlayerCount = (byte)playerCount,
            Mode = mode,
            TargetScore = targetScore,
            DeckCount = 4
        };
    }

    private void PlayTurn()
    {
        int currentPlayer = _controller.CurrentPlayer;

        // Show current game state
        _renderer.ShowGameState(_controller);

        // Check if we're in Phase 2 and show warning
        if (_controller.IsPhase2 && _controller.Context.CurrentPhase == GamePhase.Phase2_Last9)
        {
            _renderer.ShowPhase2Warning();
        }

        // Get legal moves
        var legalMoves = _controller.GetLegalMoves();

        if (legalMoves.Length == 0)
        {
            _renderer.ShowError("No legal moves available!");
            return;
        }

        // Get player input for card selection
        Card selectedCard = _inputHandler.SelectCard(
            _controller.Players[currentPlayer],
            legalMoves,
            currentPlayer
        );

        // Play the card
        if (!_controller.PlayCard(selectedCard))
        {
            _renderer.ShowError("Failed to play card!");
            return;
        }

        _renderer.ShowCardPlayed(currentPlayer, selectedCard, _controller.PlayedCards.Count, _controller.PlayerCount);

        // Check if trick is complete
        if (_controller.PlayedCards.Count == _controller.PlayerCount)
        {
            CompleteTrick();
        }
    }

    private void CompleteTrick()
    {
        // Show all played cards
        _renderer.ShowTrickCards(_controller.PlayedCards, _controller.PlayerCount);

        // Resolve the trick
        _controller.ResolveTrick();

        int winner = _controller.LastWinner;
        _renderer.ShowTrickWinner(winner);

        // Handle melding (only winner can meld in Phase 1)
        if (!_controller.IsPhase2 && _controller.CanMeld())
        {
            HandleMelding(winner);
        }
        else if (!_controller.IsPhase2)
        {
            _controller.SkipMeld();
        }

        // Draw cards
        _controller.DrawCards();

        // Start new trick
        _controller.StartNewTrick();

        // Brief pause for readability
        Thread.Sleep(1500);
    }

    private void HandleMelding(int winnerId)
    {
        var player = _controller.Players[winnerId];
        var bestMeld = _controller.GetBestMeld();

        if (bestMeld != null)
        {
            _renderer.ShowMeldOpportunity(winnerId, bestMeld);

            bool shouldMeld = _inputHandler.AskToMeld(winnerId, bestMeld);

            if (shouldMeld)
            {
                if (_controller.DeclareMeld(bestMeld.Cards, bestMeld.Type))
                {
                    _renderer.ShowMeldSuccess(winnerId, bestMeld);
                }
                else
                {
                    _renderer.ShowMeldFailed();
                }
            }
            else
            {
                _controller.SkipMeld();
                _renderer.ShowMeldSkipped(winnerId);
            }
        }
        else
        {
            _controller.SkipMeld();
        }
    }

    // Event Handlers
    private void OnTrickEnded(object? sender, TrickEndedEventArgs e)
    {
        if (e.IsFinalTrick)
        {
            _renderer.ShowFinalTrickBonus();
        }
    }

    private void OnPhaseChanged(object? sender, PhaseChangedEventArgs e)
    {
        _renderer.ShowPhaseChange(e.NewPhase);
    }

    private void OnMeldDeclared(object? sender, MeldDeclaredEventArgs e)
    {
        _renderer.ShowMeldDeclared(e.PlayerId, e.MeldType, e.Points);
    }

    private void OnRoundEnded(object? sender, RoundEndedEventArgs e)
    {
        _renderer.ShowRoundEnded(e.WinnerId, e.Scores);
    }

    private void OnGameEnded(object? sender, GameEndedEventArgs e)
    {
        _renderer.ShowGameWinner(e.WinnerId);
    }
}
