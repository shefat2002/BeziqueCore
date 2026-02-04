using BeziqueCore.Interfaces;

namespace BeziqueCore;

public partial class Bezique
{
    protected IBeziqueAdapter? _adapter;

    public void SetAdapter(IBeziqueAdapter adapter) => _adapter = adapter;
}

public class BeziqueAdapter : IBeziqueAdapter
{
    private readonly BeziqueGameController _controller;
    private readonly Bezique? _stateMachine;
    private int _cardsPlayedThisTrick = 0;
    private bool _meldDeclared = false;

    public BeziqueAdapter(BeziqueGameController controller)
    {
        _controller = controller;
    }

    public void SetStateMachine(Bezique stateMachine)
    {
        // stateMachine will set its own adapter reference
    }

    // IBeziqueAdapter - State Machine Behaviors
    public void DealFirstSet() { }
    public void DealMidSet() { }
    public void DealLastSet() { }
    public void SelectTrump() { }

    public void PlayFirstCard() { }

    public void PlayMidCard() { }

    public void PlayLastCard() { }

    public void TryMeld() => _meldDeclared = false;

    public void MeldSuccess() { }

    public void MeldFailed() { }

    public void StartNewTrick()
    {
        _controller.PlayedCards.Clear();
        _controller.Context.CurrentTurnPlayer = _controller.Context.LastTrickWinner;
        _cardsPlayedThisTrick = 0;
        _meldDeclared = false;
    }

    public void DrawCardsForAll()
    {
        for (int i = 0; i < _controller.Players.Length; i++)
        {
            if (_controller.Context.DrawDeck.Count > 0)
            {
                _controller.Players[i].Hand.Add(_controller.Context.DrawDeck.Pop());
            }
        }
    }

    public void L9PlayFirstCard()
    {
        _controller.Context.CurrentPhase = GamePhase.Phase2_Last9;
        _cardsPlayedThisTrick = 0;
    }

    public void L9PlayMidCard() { }

    public void L9PlayLastCard() { }

    public void StartL9NewTrick()
    {
        _controller.PlayedCards.Clear();
        _controller.Context.CurrentTurnPlayer = _controller.Context.LastTrickWinner;
        _cardsPlayedThisTrick = 0;
    }

    void IBeziqueAdapter.EndRound()
    {
        for (int i = 0; i < _controller.Players.Length; i++)
        {
            _controller.Players[i].TotalScore += _controller.Players[i].RoundScore;
            _controller.Players[i].RoundScore = 0;
        }
    }

    public void CalculatePoints() { }

    public void GameOver() { }

    // Public API - Delegated from Controller
    public bool PlayCard(Card card)
    {
        int currentPlayer = _controller.Context.CurrentTurnPlayer;

        if (_controller.Context.CurrentPhase == GamePhase.Phase1_Normal)
        {
            if (!PlayStateHandler.ValidateAndPlayCardPhase1(_controller.Players[currentPlayer], card, _controller.PlayedCards, currentPlayer))
                return false;
        }
        else if (_controller.Context.CurrentPhase == GamePhase.Phase2_Last9)
        {
            var leadSuit = _controller.PlayedCards.Count > 0 ? _controller.PlayedCards[0].Suit : Suit.None;
            var currentWinner = _controller.PlayedCards.Count > 0 ? _controller.PlayedCards[0] : (Card?)null;

            if (!L9PlayStateHandler.ValidateAndPlayCardPhase2(_controller.Players[currentPlayer], card, _controller.PlayedCards, currentPlayer, leadSuit, currentWinner, _controller.Context.TrumpSuit))
                return false;
        }
        else
        {
            return false;
        }

        _cardsPlayedThisTrick++;

        if (_cardsPlayedThisTrick == _controller.PlayerCount)
        {
            ResolveTrickInternal();
        }
        else
        {
            _controller.Context.CurrentTurnPlayer = TurnManager.AdvanceTurn(_controller.Context.CurrentTurnPlayer, _controller.PlayerCount);
        }

        return true;
    }

    public bool DeclareMeld(Card[] cards, MeldType meldType)
    {
        int currentPlayer = _controller.Context.CurrentTurnPlayer;

        if (!MeldStateHandler.DeclareMeld(_controller.Players[currentPlayer], cards, meldType, _controller.Context.TrumpSuit))
            return false;

        _meldDeclared = true;
        return true;
    }

    public void SkipMeld()
    {
        _meldDeclared = false;
    }

    public int CheckWinner()
    {
        for (int i = 0; i < _controller.Players.Length; i++)
        {
            if (_controller.Players[i].TotalScore >= _controller.TargetScore)
            {
                return i;
            }
        }
        return -1;
    }

    public void ResolveTrick() => ResolveTrickInternal();

    public int EndRound()
    {
        int highestScore = int.MinValue;
        int winnerId = -1;

        for (int i = 0; i < _controller.Players.Length; i++)
        {
            if (_controller.Players[i].RoundScore > highestScore)
            {
                highestScore = _controller.Players[i].RoundScore;
                winnerId = i;
            }
        }

        return winnerId;
    }

    // IBeziqueAdapter explicit interface for EndRound is above

    // Internal
    private void ResolveTrickInternal()
    {
        var playerIndices = new int[_controller.PlayerCount];
        for (int i = 0; i < _controller.PlayerCount; i++) playerIndices[i] = i;

        bool isFinalTrick = _controller.Context.DrawDeck.Count == 0 && AllHandsEmpty();
        int winnerId = TrickResolverHandler.ResolveTrick(_controller.PlayedCards, _controller.Players, playerIndices, _controller.Context.TrumpSuit, isFinalTrick);

        _controller.Context.LastTrickWinner = winnerId;

        if (isFinalTrick)
        {
            _controller.SetState(GameState.RoundEnd);
        }
        else if (_controller.Context.DrawDeck.Count == 0)
        {
            _controller.Context.CurrentPhase = GamePhase.Phase2_Last9;
            _controller.SetState(GameState.L9NewTrick);
        }
        else
        {
            _controller.SetState(GameState.NewTrick);
        }
    }

    private bool AllHandsEmpty()
    {
        for (int i = 0; i < _controller.Players.Length; i++)
        {
            if (_controller.Players[i].Hand.Count > 0 || _controller.Players[i].TableCards.Count > 0)
                return false;
        }
        return true;
    }
}
