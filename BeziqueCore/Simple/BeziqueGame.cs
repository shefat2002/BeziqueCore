using BeziqueCore.Interfaces;
using BeziqueCore.Models;
using BeziqueCore.Simple.Events;
using BeziqueCore.Notifiers;

namespace BeziqueCore.Simple;

public class BeziqueGame
{
    private readonly IMultiplayerGameAdapter _adapter;
    private readonly GameStateNotifier _notifier;
    private readonly IGameState _gameState;
    private readonly List<Player> _players = new();

    public event EventHandler<CardPlayedEventArgs>? CardPlayed;
    public event EventHandler<MeldDeclaredEventArgs>? MeldDeclared;
    public event EventHandler<MeldSkippedEventArgs>? MeldSkipped;
    public event EventHandler<TrickResolvedEventArgs>? TrickResolved;
    public event EventHandler<PlayerTurnChangedEventArgs>? PlayerTurnChanged;
    public event EventHandler<RoundEndedEventArgs>? RoundEnded;
    public event EventHandler<GameEndedEventArgs>? GameEnded;
    public event EventHandler<GameErrorEventArgs>? Error;

    public bool IsGameOver => _gameState.Winner != null;
    public Player CurrentPlayer => _gameState.CurrentPlayer;
    public string CurrentStateName => _gameState.CurrentStateName;
    public IReadOnlyList<Player> Players => _players;

    internal BeziqueGame(IMultiplayerGameAdapter adapter, GameStateNotifier notifier, IGameState gameState)
    {
        _adapter = adapter;
        _notifier = notifier;
        _gameState = gameState;
        SubscribeToNotifier();
    }

    internal void AddPlayer(Player player)
    {
        _players.Add(player);
        _gameState.AddPlayer(player);
    }

    internal void Initialize()
    {
        _adapter.InitializeGameWithPlayers(_players);
        SubscribeToMultiplayerEvents();
    }

    public void Start()
    {
        _adapter.NotifyGameInitialized();
        _adapter.DealCards();
        _adapter.NotifyCardsDealt();
        _adapter.FlipTrumpCard();
        _adapter.NotifyTrumpDetermined();
    }

    public void PlayCard(int playerIndex, int cardIndex)
    {
        if (playerIndex < 0 || playerIndex >= _players.Count)
            throw new ArgumentOutOfRangeException(nameof(playerIndex));

        var player = _players[playerIndex];

        Task.Run(async () =>
        {
            var result = await _adapter.ExecuteRemoteCommandAsync(new Models.PlayerCommand
            {
                UserId = player.Id,
                Action = BeziqueActions.PlayCard,
                Payload = new { CardIndex = cardIndex }
            });

            if (!result.Success)
                OnError(new GameErrorEventArgs { Message = result.ErrorMessage ?? "Play failed" });
        });
    }

    public void DeclareMeld(int playerIndex, int[] cardIndices)
    {
        if (playerIndex < 0 || playerIndex >= _players.Count)
            throw new ArgumentOutOfRangeException(nameof(playerIndex));

        var player = _players[playerIndex];

        Task.Run(async () =>
        {
            var result = await _adapter.ExecuteRemoteCommandAsync(new Models.PlayerCommand
            {
                UserId = player.Id,
                Action = BeziqueActions.DeclareMeld,
                Payload = new { CardIndices = cardIndices }
            });

            if (!result.Success)
                OnError(new GameErrorEventArgs { Message = result.ErrorMessage ?? "Meld failed" });
        });
    }

    public void SkipMeld(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= _players.Count)
            throw new ArgumentOutOfRangeException(nameof(playerIndex));

        var player = _players[playerIndex];

        Task.Run(async () =>
        {
            var result = await _adapter.ExecuteRemoteCommandAsync(new Models.PlayerCommand
            {
                UserId = player.Id,
                Action = BeziqueActions.SkipMeld,
                Payload = new object()
            });

            if (!result.Success)
                OnError(new GameErrorEventArgs { Message = result.ErrorMessage ?? "Skip failed" });
        });
    }

    public void SwitchSevenOfTrump(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= _players.Count)
            throw new ArgumentOutOfRangeException(nameof(playerIndex));

        var player = _players[playerIndex];

        Task.Run(async () =>
        {
            var result = await _adapter.ExecuteRemoteCommandAsync(new Models.PlayerCommand
            {
                UserId = player.Id,
                Action = BeziqueActions.SwitchSevenOfTrump,
                Payload = new object()
            });

            if (!result.Success)
                OnError(new GameErrorEventArgs { Message = result.ErrorMessage ?? "Switch failed" });
        });
    }

    public Models.GameSnapshotDto GetSnapshot()
    {
        return _adapter.GetSnapshot();
    }

    public Models.GameSnapshotDto GetSnapshotForPlayer(string playerId)
    {
        return _adapter.GetSnapshotForPlayer(playerId);
    }

    public bool CanPlayCard(int playerIndex, int cardIndex)
    {
        if (playerIndex < 0 || playerIndex >= _players.Count)
            return false;

        return _adapter.CanPlayCard(_players[playerIndex].Id, cardIndex);
    }

    public bool CanDeclareMeld(int playerIndex, int[] cardIndices)
    {
        if (playerIndex < 0 || playerIndex >= _players.Count)
            return false;

        return _adapter.CanDeclareMeld(_players[playerIndex].Id, cardIndices);
    }

    public bool CanSwitchSevenOfTrump(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= _players.Count)
            return false;

        return _adapter.CanSwitchSevenOfTrump(_players[playerIndex].Id);
    }

    public string[] GetLegalMoves(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= _players.Count)
            return Array.Empty<string>();

        return _adapter.GetLegalMoves(_players[playerIndex].Id);
    }

    private void SubscribeToNotifier()
    {
        _notifier.OnPlayerTurn += (player) =>
        {
            var index = _players.FindIndex(p => p.Id == player.Id);
            if (index >= 0)
                OnPlayerTurnChanged(new PlayerTurnChangedEventArgs
                {
                    PlayerName = player.Name,
                    PlayerId = player.Id
                });
        };

        _notifier.OnCardPlayed += (player, card) =>
        {
            var index = _players.FindIndex(p => p.Id == player.Id);
            if (index >= 0)
                OnCardPlayed(new CardPlayedEventArgs
                {
                    PlayerName = player.Name,
                    PlayerId = player.Id,
                    Card = $"{card.Suit} {card.Rank}",
                    CardIndex = index
                });
        };

        _notifier.OnMeldDeclared += (player, meld, points) =>
        {
            var index = _players.FindIndex(p => p.Id == player.Id);
            if (index >= 0)
                OnMeldDeclared(new MeldDeclaredEventArgs
                {
                    PlayerName = player.Name,
                    PlayerId = player.Id,
                    MeldType = meld.Type.ToString(),
                    Points = points
                });
        };

        _notifier.OnTrickWon += (winner, cards, points) =>
        {
            var index = _players.FindIndex(p => p.Id == winner.Id);
            if (index >= 0)
                OnTrickResolved(new TrickResolvedEventArgs
                {
                    WinnerName = winner.Name,
                    WinnerId = winner.Id,
                    Points = points
                });
        };

        _notifier.OnRoundEnded += (scores) =>
        {
            OnRoundEnded(new RoundEndedEventArgs
            {
                RoundScores = _players.ToDictionary(p => p.Id, p => p.Score)
            });
        };

        _notifier.OnGameOver += (winner) =>
        {
            var index = _players.FindIndex(p => p.Id == winner.Id);
            if (index >= 0)
                OnGameEnded(new GameEndedEventArgs
                {
                    WinnerName = winner.Name,
                    WinnerId = winner.Id,
                    FinalScores = _players.ToDictionary(p => p.Id, p => p.Score)
                });
        };
    }

    private void SubscribeToMultiplayerEvents()
    {
        _adapter.SubscribeToEvents(new MultiplayerEventHandler(this));
    }

    private void OnError(GameErrorEventArgs e)
    {
        Error?.Invoke(this, e);
    }

    private class MultiplayerEventHandler : Interfaces.IMultiplayerEventHandler
    {
        private readonly BeziqueGame _game;

        public MultiplayerEventHandler(BeziqueGame game)
        {
            _game = game;
        }

        public void OnCardPlayed(Models.Events.CardPlayedEvent gameEvent) { }
        public void OnMeldDeclared(Models.Events.MeldDeclaredEvent gameEvent) { }
        public void OnTrickResolved(Models.Events.TrickResolvedEvent gameEvent) { }
        public void OnSevenOfTrumpSwitched(Models.Events.SevenOfTrumpSwitchedEvent gameEvent) { }
        public void OnCardsDrawn(Models.Events.CardsDrawnEvent gameEvent) { }

        public void OnMeldSkipped(Models.Events.MeldSkippedEvent gameEvent)
        {
            var index = _game._players.FindIndex(p => p.Id == gameEvent.PlayerUserId);
            if (index >= 0)
                _game.OnMeldSkipped(new MeldSkippedEventArgs
                {
                    PlayerName = _game._players[index].Name,
                    PlayerId = gameEvent.PlayerUserId
                });
        }

        public void OnRoundEnded(Models.Events.RoundEndedEvent gameEvent)
        {
            _game.OnRoundEnded(new RoundEndedEventArgs
            {
                RoundScores = gameEvent.RoundScores
            });
        }

        public void OnGameEnded(Models.Events.GameEndedEvent gameEvent)
        {
            var index = _game._players.FindIndex(p => p.Id == gameEvent.WinnerUserId);
            if (index >= 0)
                _game.OnGameEnded(new GameEndedEventArgs
                {
                    WinnerName = _game._players[index].Name,
                    WinnerId = gameEvent.WinnerUserId,
                    FinalScores = gameEvent.FinalScores
                });
        }

        public void OnLastNineCardsStarted(Models.Events.LastNineCardsStartedEvent gameEvent) { }
        public void OnTrumpDetermined(Models.Events.TrumpDeterminedEvent gameEvent) { }

        public void OnPlayerTurnChanged(Models.Events.PlayerTurnChangedEvent gameEvent)
        {
            var index = _game._players.FindIndex(p => p.Id == gameEvent.PlayerUserId);
            if (index >= 0)
                _game.OnPlayerTurnChanged(new PlayerTurnChangedEventArgs
                {
                    PlayerName = _game._players[index].Name,
                    PlayerId = gameEvent.PlayerUserId
                });
        }

        public void OnError(Models.Events.GameErrorEvent gameEvent)
        {
            _game.OnError(new GameErrorEventArgs { Message = gameEvent.ErrorMessage });
        }
    }

    protected virtual void OnCardPlayed(CardPlayedEventArgs e)
    {
        CardPlayed?.Invoke(this, e);
    }

    protected virtual void OnMeldDeclared(MeldDeclaredEventArgs e)
    {
        MeldDeclared?.Invoke(this, e);
    }

    protected virtual void OnMeldSkipped(MeldSkippedEventArgs e)
    {
        MeldSkipped?.Invoke(this, e);
    }

    protected virtual void OnTrickResolved(TrickResolvedEventArgs e)
    {
        TrickResolved?.Invoke(this, e);
    }

    protected virtual void OnPlayerTurnChanged(PlayerTurnChangedEventArgs e)
    {
        PlayerTurnChanged?.Invoke(this, e);
    }

    protected virtual void OnRoundEnded(RoundEndedEventArgs e)
    {
        RoundEnded?.Invoke(this, e);
    }

    protected virtual void OnGameEnded(GameEndedEventArgs e)
    {
        GameEnded?.Invoke(this, e);
    }
}

public static class BeziqueActions
{
    public const string PlayCard = "PlayCard";
    public const string DeclareMeld = "DeclareMeld";
    public const string SkipMeld = "SkipMeld";
    public const string SwitchSevenOfTrump = "SwitchSevenOfTrump";
}
