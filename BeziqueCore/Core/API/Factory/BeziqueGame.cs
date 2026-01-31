using BeziqueCore.Core.API.Events;
using BeziqueCore.Core.Application.DTOs;
using BeziqueCore.Core.Application.Interfaces;
using BeziqueCore.Core.Domain.Entities;
using BeziqueCore.Core.Domain.Events;
using BeziqueCore.Core.Infrastructure.Notifications;

namespace BeziqueCore.Core.API.Factory;

public class BeziqueGame
{
    private readonly IMultiplayerGameAdapter _adapter;
    private readonly GameStateNotifier _notifier;
    private readonly IGameState _gameState;
    private readonly List<Player> _players = new();
    private readonly EventSubscriptionManager _events = new();

    public event EventHandler<CardPlayedEventArgs>? CardPlayed;
    public event EventHandler<MeldDeclaredEventArgs>? MeldDeclared;
    public event EventHandler<MeldSkippedEventArgs>? MeldSkipped;
    public event EventHandler<TrickResolvedEventArgs>? TrickResolved;
    public event EventHandler<PlayerTurnChangedEventArgs>? PlayerTurnChanged;
    public event EventHandler<RoundEndedEventArgs>? RoundEnded;
    public event EventHandler<GameEndedEventArgs>? GameEnded;
    public event EventHandler<GameErrorEventArgs>? Error;

    public EventSubscriptionManager Events => _events;

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
    }

    internal void Initialize()
    {
        _adapter.InitializeGameWithPlayers(_players);
        SubscribeToMultiplayerEvents();
    }

    public void Start()
    {
        _adapter.InitializeGame();
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
            var result = await _adapter.ExecuteRemoteCommandAsync(new PlayerCommand
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
            var result = await _adapter.ExecuteRemoteCommandAsync(new PlayerCommand
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
            var result = await _adapter.ExecuteRemoteCommandAsync(new PlayerCommand
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
            var result = await _adapter.ExecuteRemoteCommandAsync(new PlayerCommand
            {
                UserId = player.Id,
                Action = BeziqueActions.SwitchSevenOfTrump,
                Payload = new object()
            });

            if (!result.Success)
                OnError(new GameErrorEventArgs { Message = result.ErrorMessage ?? "Switch failed" });
        });
    }

    public GameSnapshotDto GetSnapshot()
    {
        return _adapter.GetSnapshot();
    }

    public GameSnapshotDto GetSnapshotForPlayer(string playerId)
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
        _events.Publish(this, e);
    }

    private class MultiplayerEventHandler : IMultiplayerEventHandler
    {
        private readonly BeziqueGame _game;

        public MultiplayerEventHandler(BeziqueGame game)
        {
            _game = game;
        }

        public void OnCardPlayed(CardPlayedEvent gameEvent) { }
        public void OnMeldDeclared(MeldDeclaredEvent gameEvent) { }
        public void OnTrickResolved(TrickResolvedEvent gameEvent) { }
        public void OnSevenOfTrumpSwitched(SevenOfTrumpSwitchedEvent gameEvent) { }
        public void OnCardsDrawn(CardsDrawnEvent gameEvent) { }

        public void OnMeldSkipped(MeldSkippedEvent gameEvent)
        {
            var index = _game._players.FindIndex(p => p.Id == gameEvent.PlayerUserId);
            if (index >= 0)
                _game.OnMeldSkipped(new MeldSkippedEventArgs
                {
                    PlayerName = _game._players[index].Name,
                    PlayerId = gameEvent.PlayerUserId
                });
        }

        public void OnRoundEnded(RoundEndedEvent gameEvent)
        {
            _game.OnRoundEnded(new RoundEndedEventArgs
            {
                RoundScores = gameEvent.RoundScores
            });
        }

        public void OnGameEnded(GameEndedEvent gameEvent)
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

        public void OnLastNineCardsStarted(LastNineCardsStartedEvent gameEvent) { }
        public void OnTrumpDetermined(TrumpDeterminedEvent gameEvent) { }

        public void OnPlayerTurnChanged(PlayerTurnChangedEvent gameEvent)
        {
            var index = _game._players.FindIndex(p => p.Id == gameEvent.PlayerUserId);
            if (index >= 0)
                _game.OnPlayerTurnChanged(new PlayerTurnChangedEventArgs
                {
                    PlayerName = _game._players[index].Name,
                    PlayerId = gameEvent.PlayerUserId
                });
        }

        public void OnError(GameErrorEvent gameEvent)
        {
            _game.OnError(new GameErrorEventArgs { Message = gameEvent.ErrorMessage });
        }
    }

    protected virtual void OnCardPlayed(CardPlayedEventArgs e)
    {
        CardPlayed?.Invoke(this, e);
        _events.Publish(this, e);
    }

    protected virtual void OnMeldDeclared(MeldDeclaredEventArgs e)
    {
        MeldDeclared?.Invoke(this, e);
        _events.Publish(this, e);
    }

    protected virtual void OnMeldSkipped(MeldSkippedEventArgs e)
    {
        MeldSkipped?.Invoke(this, e);
        _events.Publish(this, e);
    }

    protected virtual void OnTrickResolved(TrickResolvedEventArgs e)
    {
        TrickResolved?.Invoke(this, e);
        _events.Publish(this, e);
    }

    protected virtual void OnPlayerTurnChanged(PlayerTurnChangedEventArgs e)
    {
        PlayerTurnChanged?.Invoke(this, e);
        _events.Publish(this, e);
    }

    protected virtual void OnRoundEnded(RoundEndedEventArgs e)
    {
        RoundEnded?.Invoke(this, e);
        _events.Publish(this, e);
    }

    protected virtual void OnGameEnded(GameEndedEventArgs e)
    {
        GameEnded?.Invoke(this, e);
        _events.Publish(this, e);
    }

    public void BeginEventBatch()
    {
        _events.BeginBatch();
    }

    public void EndEventBatch()
    {
        _events.EndBatch(this);
    }

    public void FlushEventBatch()
    {
        _events.FlushBatch(this);
    }
}

public static class BeziqueActions
{
    public const string PlayCard = "PlayCard";
    public const string DeclareMeld = "DeclareMeld";
    public const string SkipMeld = "SkipMeld";
    public const string SwitchSevenOfTrump = "SwitchSevenOfTrump";
}
