using BeziqueCore.Core.Application.DTOs;
using BeziqueCore.Core.Application.Interfaces;
using BeziqueCore.Core.Domain.Entities;
using BeziqueCore.Core.Domain.Events;

namespace BeziqueCore.Core.Multiplayer.Adapters;

public class MultiplayerGameAdapter : IMultiplayerGameAdapter
{
    private readonly IGameAdapter _baseAdapter;
    private readonly IDeckOperations _deckOps;
    private readonly IPlayerActions _playerActions;
    private readonly IGameStateNotifier _notifier;
    private readonly IGameState _gameState;
    private readonly ITrickResolver _trickResolver;
    private readonly IMeldValidator _meldValidator;
    private readonly object _lock = new();

    private readonly List<IMultiplayerEventHandler> _eventHandlers = new();

    public MultiplayerGameAdapter(
        IGameAdapter baseAdapter,
        IDeckOperations deckOps,
        IPlayerActions playerActions,
        IGameStateNotifier notifier,
        IGameState gameState,
        ITrickResolver trickResolver,
        IMeldValidator meldValidator)
    {
        _baseAdapter = baseAdapter;
        _deckOps = deckOps;
        _playerActions = playerActions;
        _notifier = notifier;
        _gameState = gameState;
        _trickResolver = trickResolver;
        _meldValidator = meldValidator;
    }

    public GameSnapshotDto GetSnapshot()
    {
        lock (_lock)
        {
            return new GameSnapshotDto
            {
                StateName = GetCurrentStateName(),
                Players = _gameState.Players.Select(p => new PlayerStateDto
                {
                    UserId = p.Id,
                    Name = p.Name,
                    Score = p.Score,
                    IsDealer = p.IsDealer,
                    IsBot = p.IsBot,
                    HandCardCount = p.Hand?.Count ?? 0,
                    Hand = null,
                    DeclaredMelds = (p.DeclaredMelds ?? Enumerable.Empty<Meld>()).Select(m => new MeldDto
                    {
                        MeldType = m.Type.ToString(),
                        Cards = m.Cards.Select(c => new CardDto
                        {
                            Suit = c.Suit.ToString(),
                            Rank = c.Rank.ToString(),
                            IsJoker = c.IsJoker
                        }).ToArray(),
                        Points = m.Points
                    }).ToArray(),
                    MeldedCardCount = p.MeldedCards?.Count ?? 0
                }).ToArray(),
                TrumpSuit = _gameState.TrumpSuit.ToString(),
                TrumpCard = _gameState.TrumpCard != null ? new CardDto
                {
                    Suit = _gameState.TrumpCard.Suit.ToString(),
                    Rank = _gameState.TrumpCard.Rank.ToString(),
                    IsJoker = _gameState.TrumpCard.IsJoker
                } : null,
                CurrentTrick = new TrickStateDto
                {
                    PlayedCards = _gameState.CurrentTrick.ToDictionary(
                        kvp => kvp.Key.Id,
                        kvp => new CardDto
                        {
                            Suit = kvp.Value.Suit.ToString(),
                            Rank = kvp.Value.Rank.ToString(),
                            IsJoker = kvp.Value.IsJoker
                        }
                    ),
                    CardsPlayedCount = _gameState.CurrentTrick.Count,
                    IsComplete = _gameState.IsTrickComplete()
                },
                CurrentPlayerUserId = _gameState.CurrentPlayer?.Id ?? string.Empty,
                DealerUserId = _gameState.Players.FirstOrDefault(p => p.IsDealer)?.Id ?? string.Empty,
                DeckCardCount = _deckOps.GetRemainingCardCount(),
                IsLastNineCardsPhase = _baseAdapter.IsLastNineCardsPhase(),
                LeadSuit = _gameState.LeadSuit?.ToString(),
                RoundScores = _gameState.RoundScores.ToDictionary(kvp => kvp.Key.Id, kvp => kvp.Value),
                GameMode = _gameState.Mode.ToString(),
                WinnerUserId = _gameState.Winner?.Id
            };
        }
    }

    public GameSnapshotDto GetSnapshotForPlayer(string userId)
    {
        lock (_lock)
        {
            var snapshot = GetSnapshot();

            var player = _gameState.Players.FirstOrDefault(p => p.Id == userId);
            if (player != null && player.Hand != null)
            {
                var playerIndex = Array.FindIndex(snapshot.Players, p => p.UserId == userId);
                if (playerIndex >= 0)
                {
                    snapshot.Players[playerIndex] = snapshot.Players[playerIndex] with
                    {
                        Hand = player.Hand.Select(c => new CardDto
                        {
                            Suit = c.Suit.ToString(),
                            Rank = c.Rank.ToString(),
                            IsJoker = c.IsJoker
                        }).ToArray()
                    };
                }
            }

            return snapshot;
        }
    }

    public async Task<GameActionResult> ExecuteRemoteCommandAsync(PlayerCommand command)
    {
        lock (_lock)
        {
            try
            {
                if (!CanPlayerAct(command.UserId))
                    return GameActionResult.Error("Not your turn");

                var player = GetPlayerById(command.UserId);
                if (player == null)
                    return GameActionResult.Error("Player not found in game");

                GameActionResult result = command.Action switch
                {
                    BeziqueActions.PlayCard => HandlePlayCardCommand(player, command.Payload),
                    BeziqueActions.DeclareMeld => HandleDeclareMeldCommand(player, command.Payload),
                    BeziqueActions.SwitchSevenOfTrump => HandleSwitchSevenOfTrumpCommand(player),
                    BeziqueActions.SkipMeld => HandleSkipMeldCommand(player),
                    _ => GameActionResult.Error($"Unknown action: {command.Action}")
                };

                if (result.Success)
                {
                    result = result with
                    {
                        GameState = GetCurrentStateName(),
                        NextPlayerUserId = _gameState.CurrentPlayer?.Id
                    };
                }

                return result;
            }
            catch (Exception ex)
            {
                _eventHandlers.ForEach(h => h.OnError(new GameErrorEvent
                {
                    ErrorMessage = ex.Message,
                    PlayerUserId = command.UserId
                }));
                return GameActionResult.Error(ex.Message);
            }
        }
    }

    public string[] GetLegalMoves(string userId)
    {
        lock (_lock)
        {
            var player = _gameState.Players.FirstOrDefault(p => p.Id == userId);
            if (player == null || _gameState.CurrentPlayer?.Id != userId)
                return Array.Empty<string>();

            var moves = new List<string>();

            if (!_gameState.IsTrickComplete())
                moves.Add(BeziqueActions.PlayCard);

            if (CanSwitchSevenOfTrump(userId))
                moves.Add(BeziqueActions.SwitchSevenOfTrump);

            if (CanDeclareMeldInCurrentState())
            {
                moves.Add(BeziqueActions.DeclareMeld);
                moves.Add(BeziqueActions.SkipMeld);
            }

            return moves.ToArray();
        }
    }

    public bool CanPlayerAct(string userId)
    {
        lock (_lock)
        {
            return _gameState.CurrentPlayer?.Id == userId;
        }
    }

    public string? GetCurrentPlayerUserId()
    {
        lock (_lock)
        {
            return _gameState.CurrentPlayer?.Id;
        }
    }

    public Player? GetPlayerById(string userId)
    {
        lock (_lock)
        {
            return _gameState.Players.FirstOrDefault(p => p.Id == userId);
        }
    }

    public void InitializeGameWithPlayers(List<Player> players)
    {
        lock (_lock)
        {
            _gameState.Reset();

            foreach (var player in players)
                _gameState.AddPlayer(player);

            var nonDealer = _gameState.Players.FirstOrDefault(p => !p.IsDealer)
                         ?? _gameState.Players.FirstOrDefault();
            if (nonDealer != null)
                _gameState.CurrentPlayer = nonDealer;

            _gameState.StartNewTrick();
        }
    }

    public bool CanPlayCard(string userId, int cardIndex)
    {
        lock (_lock)
        {
            if (!CanPlayerAct(userId))
                return false;

            var player = GetPlayerById(userId);
            if (player == null || player.Hand == null)
                return false;

            return cardIndex >= 0 && cardIndex < player.Hand.Count;
        }
    }

    public bool CanDeclareMeld(string userId, int[] cardIndices)
    {
        lock (_lock)
        {
            if (!CanPlayerAct(userId))
                return false;

            if (!CanDeclareMeldInCurrentState())
                return false;

            var player = GetPlayerById(userId);
            if (player == null || player.Hand == null)
                return false;

            foreach (var index in cardIndices)
            {
                if (index < 0 || index >= player.Hand.Count)
                    return false;
            }

            var cards = cardIndices.Select(i => player.Hand[i]).ToArray();
            return _meldValidator.IsValidMeld(cards, _gameState.TrumpSuit);
        }
    }

    public bool CanSwitchSevenOfTrump(string userId)
    {
        lock (_lock)
        {
            if (!CanPlayerAct(userId))
                return false;

            var player = GetPlayerById(userId);
            if (player == null || player.Hand == null)
                return false;

            return player.Hand.Any(c => c.Rank == Rank.Seven && c.Suit == _gameState.TrumpSuit)
                && _deckOps.GetTrumpCard() != null;
        }
    }

    public string GetCurrentStateName()
    {
        lock (_lock)
        {
            return _gameState.CurrentStateName;
        }
    }

    public void SubscribeToEvents(IMultiplayerEventHandler eventHandler)
    {
        lock (_lock)
        {
            _eventHandlers.Add(eventHandler);
        }
    }

    public void InitializeGame() => _baseAdapter.InitializeGame();
    public void NotifyGameInitialized() => _baseAdapter.NotifyGameInitialized();
    public void DealCards() => _baseAdapter.DealCards();
    public void NotifyCardsDealt() => _baseAdapter.NotifyCardsDealt();

    public void FlipTrumpCard()
    {
        _baseAdapter.FlipTrumpCard();
        FireTrumpDeterminedEvent();
    }

    public void NotifyTrumpDetermined() => _baseAdapter.NotifyTrumpDetermined();
    public void StartPlayerTimer() => _baseAdapter.StartPlayerTimer();
    public void StopPlayerTimer() => _baseAdapter.StopPlayerTimer();
    public void ResetPlayerTimer() => _baseAdapter.ResetPlayerTimer();
    public void DeductTimeoutPoints() => _baseAdapter.DeductTimeoutPoints();
    public void ProcessOpponentResponses() => _baseAdapter.ProcessOpponentResponses();

    public void ResolveTrick()
    {
        _baseAdapter.ResolveTrick();
        FireTrickResolvedEvent();
    }

    public void ProcessMeldOpportunity() => _baseAdapter.ProcessMeldOpportunity();
    public void ScoreMeld() => _baseAdapter.ScoreMeld();
    public void DrawCards() => _baseAdapter.DrawCards();
    public void CheckDeck() => _baseAdapter.CheckDeck();
    public void ProcessL9OpponentResponses() => _baseAdapter.ProcessL9OpponentResponses();
    public void ResolveL9Trick() => _baseAdapter.ResolveL9Trick();
    public void CheckL9TrickComplete() => _baseAdapter.CheckL9TrickComplete();
    public void CalculateL9FinalScores() => _baseAdapter.CalculateL9FinalScores();
    public void CalculateRoundScores() => _baseAdapter.CalculateRoundScores();
    public void CalculateAcesAndTens() => _baseAdapter.CalculateAcesAndTens();

    public void NotifyRoundEnded()
    {
        _baseAdapter.NotifyRoundEnded();
        FireRoundEndedEvent();
    }

    public void DeclareWinner() => _baseAdapter.DeclareWinner();

    public void NotifyGameOver()
    {
        _baseAdapter.NotifyGameOver();
        FireGameEndedEvent();
    }

    public bool IsLastNineCardsPhase() => _baseAdapter.IsLastNineCardsPhase();
    public bool IsDeckEmpty() => _baseAdapter.IsDeckEmpty();
    public bool AreAllHandsEmpty() => _baseAdapter.AreAllHandsEmpty();
    public bool HasPlayerReachedWinningScore() => _baseAdapter.HasPlayerReachedWinningScore();
    public bool IsTrickComplete() => _baseAdapter.IsTrickComplete();
    public bool MorePlayersNeedToPlay() => _baseAdapter.MorePlayersNeedToPlay();

    private GameActionResult HandlePlayCardCommand(Player player, object payload)
    {
        try
        {
            int cardIndex = ExtractCardIndex(payload);

            if (!CanPlayCard(player.Id, cardIndex))
                return GameActionResult.Error("Cannot play this card");

            var card = player.Hand[cardIndex];
            _playerActions.PlayCard(player, card);

            FireCardPlayedEvent(player, card, cardIndex);
            FirePlayerTurnChangedEvent();

            return GameActionResult.Ok(new { CardIndex = cardIndex });
        }
        catch (Exception ex)
        {
            return GameActionResult.Error(ex.Message);
        }
    }

    private GameActionResult HandleDeclareMeldCommand(Player player, object payload)
    {
        try
        {
            int[] cardIndices = ExtractCardIndices(payload);

            if (!CanDeclareMeld(player.Id, cardIndices))
                return GameActionResult.Error("Cannot declare this meld");

            var cards = cardIndices.Select(i => player.Hand[i]).ToList();
            var meldType = _meldValidator.DetermineMeldType(cards.ToArray(), _gameState.TrumpSuit);

            var meld = new Meld
            {
                Type = meldType,
                Cards = cards
            };

            _playerActions.DeclareMeld(player, meld);

            FireMeldDeclaredEvent(player, meld, cardIndices);
            FirePlayerTurnChangedEvent();

            return GameActionResult.Ok(new { CardIndices = cardIndices, MeldType = meldType.ToString() });
        }
        catch (Exception ex)
        {
            return GameActionResult.Error(ex.Message);
        }
    }

    private GameActionResult HandleSwitchSevenOfTrumpCommand(Player player)
    {
        try
        {
            if (!CanSwitchSevenOfTrump(player.Id))
                return GameActionResult.Error("Cannot switch seven of trump");

            _playerActions.SwitchSevenOfTrump(player);

            FireSevenOfTrumpSwitchedEvent(player);
            FirePlayerTurnChangedEvent();

            return GameActionResult.Ok();
        }
        catch (Exception ex)
        {
            return GameActionResult.Error(ex.Message);
        }
    }

    private GameActionResult HandleSkipMeldCommand(Player player)
    {
        try
        {
            _playerActions.SkipMeld(player);

            FireMeldSkippedEvent(player);
            FirePlayerTurnChangedEvent();

            return GameActionResult.Ok();
        }
        catch (Exception ex)
        {
            return GameActionResult.Error(ex.Message);
        }
    }

    private void FireCardPlayedEvent(Player player, Card card, int cardIndex)
    {
        _eventHandlers.ForEach(h => h.OnCardPlayed(new CardPlayedEvent
        {
            PlayerUserId = player.Id,
            Card = new CardDto
            {
                Suit = card.Suit.ToString(),
                Rank = card.Rank.ToString(),
                IsJoker = card.IsJoker
            },
            CardIndex = cardIndex
        }));
    }

    private void FireMeldDeclaredEvent(Player player, Meld meld, int[] cardIndices)
    {
        _eventHandlers.ForEach(h => h.OnMeldDeclared(new MeldDeclaredEvent
        {
            PlayerUserId = player.Id,
            Meld = new MeldDto
            {
                MeldType = meld.Type.ToString(),
                Cards = meld.Cards.Select(c => new CardDto
                {
                    Suit = c.Suit.ToString(),
                    Rank = c.Rank.ToString(),
                    IsJoker = c.IsJoker
                }).ToArray(),
                Points = meld.Points
            },
            CardIndices = cardIndices,
            Points = meld.Points
        }));
    }

    private void FireMeldSkippedEvent(Player player)
    {
        _eventHandlers.ForEach(h => h.OnMeldSkipped(new MeldSkippedEvent
        {
            PlayerUserId = player.Id
        }));
    }

    private void FireTrickResolvedEvent()
    {
        var winner = _gameState.LastTrickWinner;
        if (winner == null) return;

        _eventHandlers.ForEach(h => h.OnTrickResolved(new TrickResolvedEvent
        {
            WinnerUserId = winner.Id,
            PlayedCards = _gameState.CurrentTrick.ToDictionary(
                kvp => kvp.Key.Id,
                kvp => new CardDto
                {
                    Suit = kvp.Value.Suit.ToString(),
                    Rank = kvp.Value.Rank.ToString(),
                    IsJoker = kvp.Value.IsJoker
                }
            ),
            Points = 0
        }));
    }

    private void FireSevenOfTrumpSwitchedEvent(Player player)
    {
        _eventHandlers.ForEach(h => h.OnSevenOfTrumpSwitched(new SevenOfTrumpSwitchedEvent
        {
            PlayerUserId = player.Id
        }));
    }

    private void FireRoundEndedEvent()
    {
        _eventHandlers.ForEach(h => h.OnRoundEnded(new RoundEndedEvent
        {
            RoundScores = _gameState.RoundScores.ToDictionary(kvp => kvp.Key.Id, kvp => kvp.Value)
        }));
    }

    private void FireGameEndedEvent()
    {
        var winner = _gameState.Winner;
        if (winner == null) return;

        _eventHandlers.ForEach(h => h.OnGameEnded(new GameEndedEvent
        {
            WinnerUserId = winner.Id,
            FinalScores = _gameState.Players.ToDictionary(p => p.Id, p => p.Score)
        }));
    }

    private void FireTrumpDeterminedEvent()
    {
        if (_gameState.TrumpCard == null) return;

        _eventHandlers.ForEach(h => h.OnTrumpDetermined(new TrumpDeterminedEvent
        {
            TrumpSuit = _gameState.TrumpSuit.ToString(),
            TrumpCard = new CardDto
            {
                Suit = _gameState.TrumpCard.Suit.ToString(),
                Rank = _gameState.TrumpCard.Rank.ToString(),
                IsJoker = _gameState.TrumpCard.IsJoker
            }
        }));
    }

    private void FirePlayerTurnChangedEvent()
    {
        if (_gameState.CurrentPlayer == null) return;

        _eventHandlers.ForEach(h => h.OnPlayerTurnChanged(new PlayerTurnChangedEvent
        {
            PlayerUserId = _gameState.CurrentPlayer.Id,
            CanDeclareMeld = CanDeclareMeldInCurrentState()
        }));
    }

    private bool CanDeclareMeldInCurrentState()
    {
        var state = GetCurrentStateName();
        return state.Contains("MELD") || state.Contains("PLAYER_TURN");
    }

    private int ExtractCardIndex(object payload)
    {
        if (payload is IDictionary<string, object> dict)
        {
            if (dict.ContainsKey("CardIndex"))
                return Convert.ToInt32(dict["CardIndex"]);
            if (dict.ContainsKey("cardIndex"))
                return Convert.ToInt32(dict["cardIndex"]);
        }
        throw new ArgumentException("Invalid payload format for card index");
    }

    private int[] ExtractCardIndices(object payload)
    {
        if (payload is IDictionary<string, object> dict)
        {
            if (dict.ContainsKey("CardIndices"))
            {
                var indices = dict["CardIndices"];
                if (indices is int[] arr) return arr;
            }
            if (dict.ContainsKey("cardIndices"))
            {
                var indices = dict["cardIndices"];
                if (indices is int[] arr) return arr;
                if (indices is List<object> list)
                    return list.Cast<int>().ToArray();
            }
        }
        throw new ArgumentException("Invalid payload format for card indices");
    }
}

public static class BeziqueActions
{
    public const string PlayCard = "PlayCard";
    public const string DeclareMeld = "DeclareMeld";
    public const string SkipMeld = "SkipMeld";
    public const string SwitchSevenOfTrump = "SwitchSevenOfTrump";
}
