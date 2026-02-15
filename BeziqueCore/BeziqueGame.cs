using BeziqueCore.Interfaces;

namespace BeziqueCore;

public class BeziqueGame : IBeziqueGame
{
    private Bezique? _stateMachine;
    private BeziqueConcrete? _adapter;
    private GamePhase _currentPhase = GamePhase.Dealing;
    private int _playerCount;

    public event Action<int, IReadOnlyList<IGameCard>>? CardsDealt;
    public event Action<IGameCard>? TrumpCardFlipped;
    public event Action<int>? DealerBonusPoints;
    public event Action<int, IGameCard>? CardDrawn;
    public event Action<GamePhase>? PhaseChanged;
    public event Action<int>? TrickComplete;
    public event Action<int, int>? MeldPointsAwarded;
    public event Action<int>? RoundEnd;
    public event Action<int>? GameEnd;

    public GamePhase CurrentPhase => _currentPhase;

    public IReadOnlyList<IPlayerHand> Players
    {
        get
        {
            if (_adapter == null) return Array.Empty<IPlayerHand>();
            return _adapter.Player
                .Select((hand, idx) => new PlayerHand(idx, hand, _adapter.Scores[idx]))
                .ToList<IPlayerHand>();
        }
    }

    public IGameCard? TrumpCard
    {
        get
        {
            if (_adapter?.TrumpCard == null) return null;
            return new GameCard(_adapter.TrumpCard.Value);
        }
    }

    public byte? TrumpSuit => _adapter?.TrumpSuit;
    public int DeckCount => _adapter?.Dealer.Count ?? 0;
    public int CurrentPlayerIndex => _adapter?.DealOrder ?? 0;
    public int DealerIndex => _adapter?.DealerIndex ?? 0;

    public void Initialize(int playerCount)
    {
        _playerCount = playerCount;
        _adapter = new BeziqueConcrete(playerCount);
        _stateMachine = new Bezique();
        _stateMachine.SetAdapter(_adapter);
        _stateMachine.Start();
        _currentPhase = GamePhase.Dealing;
    }

    public void StartDealing()
    {
        if (_stateMachine == null || _adapter == null)
            throw new InvalidOperationException("Game not initialized. Call Initialize first.");

        var evt = _playerCount == 2
            ? Bezique.EventId.TWOPLAYERGAME
            : Bezique.EventId.FOURPLAYERGAME;

        _stateMachine.DispatchEvent(evt);
    }

    public bool DealNextSet()
    {
        if (_adapter == null || _stateMachine == null)
            return false;

        if (_adapter.SetsDealtToCurrentPlayer >= BeziqueConcrete.SetsPerPlayer)
            return false;

        int cardsPerDeal = 3;
        for (int i = 0; i < _playerCount * cardsPerDeal; i++)
        {
            var playerIndex = _adapter.DealOrder;
            var cardsToDeal = _adapter.Dealer.Take(cardsPerDeal).ToList();

            _stateMachine.DispatchEvent(Bezique.EventId.COMPLETE);

            if (CardsDealt != null && cardsToDeal.Count > 0)
            {
                var gameCards = cardsToDeal.Select(id => new GameCard(id)).ToList<IGameCard>();
                CardsDealt.Invoke(playerIndex, gameCards);
            }
        }

        if (_adapter.SetsDealtToCurrentPlayer >= BeziqueConcrete.SetsPerPlayer)
        {
            _currentPhase = GamePhase.TrumpFlip;
            PhaseChanged?.Invoke(_currentPhase);
            return false;
        }

        return true;
    }

    public void CompleteDealing()
    {
        if (_stateMachine == null || _adapter == null)
            throw new InvalidOperationException("Game not initialized.");

        _stateMachine.DispatchEvent(Bezique.EventId.DEALCOMPLETE);

        if (_adapter.IsTrump7())
        {
            _adapter.Add10PointsToDealer();
            DealerBonusPoints?.Invoke(_adapter.DealerIndex);
            _stateMachine.DispatchEvent(Bezique.EventId.TRUMP7);
        }
        else
        {
            _stateMachine.DispatchEvent(Bezique.EventId.OTHERCARD);
        }

        if (_adapter.TrumpCard.HasValue && TrumpCardFlipped != null)
        {
            TrumpCardFlipped.Invoke(new GameCard(_adapter.TrumpCard.Value));
        }

        _currentPhase = GamePhase.Phase1_Playing;
        PhaseChanged?.Invoke(_currentPhase);
    }

    public void PlayCard(int playerIndex, byte cardId)
    {
        if (_adapter == null || _stateMachine == null)
            throw new InvalidOperationException("Game not initialized.");

        // Actually remove the card from player's hand
        _adapter.PlayCard(playerIndex, cardId);

        if (playerIndex == 0)
            _stateMachine.DispatchEvent(Bezique.EventId.PLAYERTURN);
        else
            _stateMachine.DispatchEvent(Bezique.EventId.OPPONENTTURN);
    }

    public void DrawCard(int playerIndex)
    {
        if (_adapter == null || _stateMachine == null)
            throw new InvalidOperationException("Game not initialized.");

        var result = _adapter.DrawCardFromDeck();

        if (result == DrawResult.DrawComplete)
            _stateMachine.DispatchEvent(Bezique.EventId.DRAWCARDTOALL);
        else
            _stateMachine.DispatchEvent(Bezique.EventId.MOREPLAYERSTODRAW);
    }

    public void CreateMeld(int playerIndex, MeldType meldType, byte[] cardIds)
    {
        if (_adapter == null || _stateMachine == null)
            throw new InvalidOperationException("Game not initialized.");

        _adapter.SetPendingMeld(meldType);
        _stateMachine.DispatchEvent(Bezique.EventId.MELDPOINTGIVEN);
    }

    public bool IsPlayerTurn(int playerIndex)
    {
        return CurrentPlayerIndex == playerIndex;
    }

    public IReadOnlyList<byte> GetPlayableCards(int playerIndex)
    {
        if (_adapter == null)
            return Array.Empty<byte>();

        return _adapter.Player[playerIndex].ToList();
    }

    public void StartNewRound()
    {
        if (_stateMachine == null)
            throw new InvalidOperationException("Game not initialized.");

        _stateMachine.DispatchEvent(Bezique.EventId.WINNINGSCORENOTREACHED);
        _currentPhase = GamePhase.Dealing;
        PhaseChanged?.Invoke(_currentPhase);
    }
}
