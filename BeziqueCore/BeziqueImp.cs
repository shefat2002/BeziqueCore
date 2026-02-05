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
    private readonly Bezique _stateMachine;
    private int _cardsPlayedThisTrick = 0;
    private bool _meldDeclared = false;
    private int _dealPhase = 0; // 0=first 3 cards, 1=second 3 cards, 2=last 3 cards

    public BeziqueAdapter(BeziqueGameController controller)
    {
        _controller = controller;
        _stateMachine = new Bezique();
        _stateMachine.SetAdapter(this);
    }

 
    public void StartStateMachine()
    {
        _stateMachine.Start();

        _stateMachine.DispatchEvent(Bezique.EventId.COMPLETE); // DealFirst → DealMid
        _stateMachine.DispatchEvent(Bezique.EventId.COMPLETE); // DealMid → DealLast
        _stateMachine.DispatchEvent(Bezique.EventId.COMPLETE); // DealLast → SelectTrump
        _stateMachine.DispatchEvent(Bezique.EventId.COMPLETE); // SelectTrump → Play

    }


    public void DealFirstSet()
    {
        // Cards already dealt - no-op
    }

    public void DealMidSet()
    {
        // Cards already dealt - no-op
    }

    public void DealLastSet()
    {
        // Cards already dealt - no-op
    }

    public void SelectTrump()
    {
        // Trump already selected by GameInitializer - no-op
    }

    // Play Phase Methods
    public void PlayFirstCard()
    {
        // Reset for new trick
        _cardsPlayedThisTrick = 1;
        _meldDeclared = false;
    }

    public void PlayMidCard()
    {
        // Advance turn after first/middle player
        _controller.Context.CurrentTurnPlayer = TurnManager.AdvanceTurn(_controller.Context.CurrentTurnPlayer, _controller.PlayerCount);
        _cardsPlayedThisTrick++;
    }

    public void PlayLastCard()
    {
        // Last card played - trick complete
        _cardsPlayedThisTrick++;
    }

    // Meld Phase Methods
    public void TryMeld()
    {
        _meldDeclared = false;
        // State machine enters TryMelded state - awaiting external meld declaration
    }

    public void MeldSuccess()
    {
        _meldDeclared = true;
    }

    public void MeldFailed()
    {
        _meldDeclared = false;
    }

    // Trick Transition Methods
    public void StartNewTrick()
    {
        _controller.PlayedCards.Clear();
        _controller.Context.CurrentTurnPlayer = _controller.Context.LastTrickWinner;
        _cardsPlayedThisTrick = 0;
        _meldDeclared = false;
    }

    public void DrawCardsForAll()
    {
        int winnerId = _controller.Context.LastTrickWinner;
        int playerCount = _controller.PlayerCount;

        // Winner draws first (top card)
        if (_controller.Context.DrawDeck.Count > 0)
        {
            _controller.Players[winnerId].Hand.Add(_controller.Context.DrawDeck.Pop());
        }

        // Others draw in order
        for (int i = 1; i < playerCount; i++)
        {
            int playerId = (winnerId + i) % playerCount;
            if (_controller.Context.DrawDeck.Count > 0)
            {
                _controller.Players[playerId].Hand.Add(_controller.Context.DrawDeck.Pop());
            }
        }
    }

    // L9 Play Phase Methods (Last 9 cards, no drawing)
    public void L9PlayFirstCard()
    {
        _controller.Context.CurrentPhase = GamePhase.Phase2_Last9;
        _cardsPlayedThisTrick = 1;
        _meldDeclared = false;

        // Return all table cards to hand
        PhaseTransitionManager.ReturnAllTableCardsToHand(_controller.Players);
        _controller.OnPhaseChanged(GamePhase.Phase2_Last9);
    }

    public void L9PlayMidCard()
    {
        _controller.Context.CurrentTurnPlayer = TurnManager.AdvanceTurn(_controller.Context.CurrentTurnPlayer, _controller.PlayerCount);
        _cardsPlayedThisTrick++;
    }

    public void L9PlayLastCard()
    {
        _cardsPlayedThisTrick++;
    }

    public void StartL9NewTrick()
    {
        _controller.PlayedCards.Clear();
        _controller.Context.CurrentTurnPlayer = _controller.Context.LastTrickWinner;
        _cardsPlayedThisTrick = 0;
        _meldDeclared = false;
    }

    // Round End Methods
    void IBeziqueAdapter.EndRound()
    {
        int winnerId = RoundEndHandler.EndRound(_controller.Players, _controller.Context.GameMode, _controller.PlayerCount);
        _controller.OnRoundEnded(winnerId, _controller.Players.Select(p => p.TotalScore).ToArray());
    }

    public void CalculatePoints()
    {
        // Points are calculated during EndRound, this is a state machine notification
    }

    // Game End Method
    public void GameOver()
    {
        // Find and announce winner
        int winnerId = -1;
        int highestScore = -1;

        for (int i = 0; i < _controller.Players.Length; i++)
        {
            if (_controller.Players[i].TotalScore > highestScore)
            {
                highestScore = _controller.Players[i].TotalScore;
                winnerId = i;
            }
        }

        if (winnerId >= 0)
        {
            _controller.OnGameEnded(winnerId);
        }
    }

    // ============================================================
    // Public API - Methods Called from BeziqueGameController
    // ============================================================

    public bool PlayCard(Card card)
    {
        int currentPlayer = _controller.Context.CurrentTurnPlayer;
        var player = _controller.Players[currentPlayer];

        // Validate card is in hand or table
        bool cardInHand = PlayCardValidator.ContainsCard(player.Hand, card);
        bool cardOnTable = PlayCardValidator.ContainsCard(player.TableCards, card);

        if (!cardInHand && !cardOnTable)
            return false;

        // Validate move based on phase
        if (_controller.Context.CurrentPhase == GamePhase.Phase2_Last9)
        {
            var leadSuit = _controller.PlayedCards.Count > 0 ? _controller.PlayedCards[0].Suit : Suit.None;
            var currentWinner = _controller.PlayedCards.Count > 0 ? _controller.PlayedCards[0] : (Card?)null;

            if (!Phase2MoveValidator.IsLegalMove(player.Hand.Concat(player.TableCards).ToList(), card, leadSuit, currentWinner, _controller.Context.TrumpSuit))
            {
                return false;
            }
        }

        // Play the card
        _controller.PlayedCards.Add(card);
        PlayCardValidator.TryRemoveCard(player.Hand, card);

        // Dispatch event to state machine based on position in trick
        int cardsInTrick = _controller.PlayedCards.Count;

        if (cardsInTrick == 1)
        {
            // First card of trick
            if (_controller.Context.CurrentPhase == GamePhase.Phase2_Last9)
            {
                _stateMachine.DispatchEvent(Bezique.EventId.COMPLETE); // Move to L9PlayFirst
            }
            else
            {
                _stateMachine.DispatchEvent(Bezique.EventId.COMPLETE); // Move to PlayFirst
            }
        }
        else if (cardsInTrick < _controller.PlayerCount)
        {
            // Middle card(s) of trick
            _stateMachine.DispatchEvent(Bezique.EventId.COMPLETE); // Move to PlayMid or L9PlayMid
        }
        else
        {
            // Last card of trick - trick complete
            if (_controller.Context.CurrentPhase == GamePhase.Phase2_Last9)
            {
                _stateMachine.DispatchEvent(Bezique.EventId.COMPLETE); // Move to L9PlayLast
            }
            else
            {
                _stateMachine.DispatchEvent(Bezique.EventId.COMPLETE); // Move to PlayLast
            }

            // Automatically resolve trick after last card
            ResolveTrickInternal();
        }

        return true;
    }

    public bool DeclareMeld(Card[] cards, MeldType meldType)
    {
        int currentPlayer = _controller.Context.CurrentTurnPlayer;

        // Only trick winner can meld
        if (currentPlayer != _controller.Context.LastTrickWinner)
            return false;

        // Must be in Phase 1
        if (_controller.Context.CurrentPhase != GamePhase.Phase1_Normal)
            return false;

        // Validate meld
        var bestMeld = MeldValidator.FindBestMeld(_controller.Players[currentPlayer], _controller.Context.TrumpSuit);
        if (bestMeld == null || bestMeld.Type != meldType)
            return false;

        int beforeScore = _controller.Players[currentPlayer].RoundScore;
        if (!MeldStateHandler.DeclareMeld(_controller.Players[currentPlayer], cards, meldType, _controller.Context.TrumpSuit))
            return false;

        _meldDeclared = true;
        int points = _controller.Players[currentPlayer].RoundScore - beforeScore;
        _controller.OnMeldDeclared(currentPlayer, meldType, points);

        // Dispatch success event to state machine
        _stateMachine.DispatchEvent(Bezique.EventId.SUCCESS);

        return true;
    }

    public void SkipMeld()
    {
        _meldDeclared = false;
        // Dispatch failed event to state machine to continue flow
        _stateMachine.DispatchEvent(Bezique.EventId.FAILED);
    }

    public bool CanSwapTrumpSeven()
    {
        int currentPlayer = _controller.Context.CurrentTurnPlayer;
        return MeldStateHandler.CanSwapTrumpSeven(_controller.Players[currentPlayer], _controller.Context.TrumpCard, _controller.Context.TrumpSuit);
    }

    public bool SwapTrumpSeven()
    {
        int currentPlayer = _controller.Context.CurrentTurnPlayer;
        var trumpCard = _controller.Context.TrumpCard;
        bool success = MeldStateHandler.SwapTrumpSeven(_controller.Players[currentPlayer], ref trumpCard, _controller.Context.TrumpSuit);
        if (success)
        {
            _controller.Context.TrumpCard = trumpCard;
        }
        return success;
    }

    public int CheckWinner()
    {
        for (int i = 0; i < _controller.Players.Length; i++)
        {
            if (_controller.Players[i].TotalScore >= _controller.TargetScore)
            {
                // Dispatch winning score event to state machine
                _stateMachine.DispatchEvent(Bezique.EventId.WINNINGSCORE);
                return i;
            }
        }
        return -1;
    }

    public void ResolveTrick()
    {
        // This is called externally - resolution happens automatically after last card played
        // No action needed here as it's handled in PlayCard
    }

    public int EndRound()
    {
        int winnerId = RoundEndHandler.EndRound(_controller.Players, _controller.Context.GameMode, _controller.PlayerCount);
        _controller.OnRoundEnded(winnerId, _controller.Players.Select(p => p.TotalScore).ToArray());

        // Notify state machine that round ended
        _stateMachine.DispatchEvent(Bezique.EventId.COMPLETE);

        return winnerId;
    }

    // Gateway Methods
    public void DrawCards()
    {
        int winnerId = _controller.Context.LastTrickWinner;

        if (_controller.Context.DrawDeck.Count == 0)
        {
            // Deck empty - dispatch event to transition to L9 phase
            _stateMachine.DispatchEvent(Bezique.EventId.DECKEMPTY);
            return;
        }

        // Execute draw
        bool transitioned = PhaseTransitionManager.ExecuteDraw(_controller.Players, winnerId, _controller.Context.TrumpCard, _controller.Context.DrawDeck, _controller.PlayerCount);

        if (transitioned)
        {
            _controller.Context.CurrentPhase = GamePhase.Phase2_Last9;
            _controller.OnPhaseChanged(GamePhase.Phase2_Last9);
            // Dispatch deck empty event to transition state machine
            _stateMachine.DispatchEvent(Bezique.EventId.DECKEMPTY);
        }
    }

    public bool CanMeld()
    {
        if (_controller.Context.CurrentPhase != GamePhase.Phase1_Normal)
            return false;

        int currentPlayer = _controller.Context.CurrentTurnPlayer;
        return currentPlayer == _controller.Context.LastTrickWinner;
    }

    public bool CheckPhaseTransition()
    {
        return _controller.Context.DrawDeck.Count == _controller.PlayerCount;
    }

    public Card[] GetLegalMoves()
    {
        int currentPlayer = _controller.Context.CurrentTurnPlayer;
        var player = _controller.Players[currentPlayer];

        if (_controller.Context.CurrentPhase == GamePhase.Phase1_Normal)
        {
            return player.Hand.Concat(player.TableCards).ToArray();
        }
        else if (_controller.Context.CurrentPhase == GamePhase.Phase2_Last9)
        {
            var leadSuit = _controller.PlayedCards.Count > 0 ? _controller.PlayedCards[0].Suit : Suit.None;
            var currentWinner = _controller.PlayedCards.Count > 0 ? _controller.PlayedCards[0] : (Card?)null;

            var legalMoves = new List<Card>();
            foreach (var card in player.Hand.Concat(player.TableCards))
            {
                if (Phase2MoveValidator.IsLegalMove(player.Hand.Concat(player.TableCards).ToList(), card, leadSuit, currentWinner, _controller.Context.TrumpSuit))
                {
                    legalMoves.Add(card);
                }
            }
            return legalMoves.ToArray();
        }

        return Array.Empty<Card>();
    }

    public MeldOpportunity? GetBestMeld()
    {
        int currentPlayer = _controller.Context.CurrentTurnPlayer;
        return MeldValidator.FindBestMeld(_controller.Players[currentPlayer], _controller.Context.TrumpSuit);
    }

    // ============================================================
    // Internal Implementation
    // ============================================================

    private void ResolveTrickInternal()
    {
        var playerIndices = new int[_controller.PlayerCount];
        for (int i = 0; i < _controller.PlayerCount; i++) playerIndices[i] = i;

        bool isFinalTrick = _controller.Context.DrawDeck.Count == 0 && AllHandsEmpty();
        int winnerId = TrickResolverHandler.ResolveTrick(_controller.PlayedCards, _controller.Players, playerIndices, _controller.Context.TrumpSuit, isFinalTrick);

        _controller.Context.LastTrickWinner = winnerId;
        _controller.OnTrickEnded(winnerId, isFinalTrick);

        // Update controller state
        if (isFinalTrick)
        {
            _controller.SetState(GameState.RoundEnd);
        }
        else if (_controller.Context.DrawDeck.Count == 0)
        {
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
