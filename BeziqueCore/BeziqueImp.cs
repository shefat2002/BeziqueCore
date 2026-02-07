using BeziqueCore.Interfaces;

namespace BeziqueCore;

public partial class Bezique
{
    protected IBeziqueAdapter? _adapter;

    public void SetAdapter(IBeziqueAdapter adapter) => _adapter = adapter;
}

internal class BeziqueAdapter : IBeziqueAdapter
{
    private readonly BeziqueGameController _controller;
    private readonly Bezique _stateMachine;

    public BeziqueAdapter(BeziqueGameController controller)
    {
        _controller = controller;
        _stateMachine = new Bezique();
        _stateMachine.SetAdapter(this);
    }

    public Bezique.StateId CurrentFsmStateId => _stateMachine.stateId;

    public void StartStateMachine()
    {
        _stateMachine.Start();

        // Dispatch COMPLETE events to progress through deal states to reach Play state
        // DealFirst → DealMid → DealLast → SelectTrump → Play
        _stateMachine.DispatchEvent(Bezique.EventId.COMPLETE);
        _stateMachine.DispatchEvent(Bezique.EventId.COMPLETE);
        _stateMachine.DispatchEvent(Bezique.EventId.COMPLETE);
        _stateMachine.DispatchEvent(Bezique.EventId.COMPLETE);
    }

    public void DealFirstSet()
    {
        DealCardsToAllPlayers(cardsPerPlayer: 3);
    }

    public void DealMidSet()
    {
        DealCardsToAllPlayers(cardsPerPlayer: 3);
    }

    public void DealLastSet()
    {
        DealCardsToAllPlayers(cardsPerPlayer: 3);
    }

    public void SelectTrump()
    {
        if (_controller.Context.DrawDeck.Count > 0)
        {
            var trumpCard = _controller.Context.DrawDeck.Pop();
            _controller.Context.TrumpCard = trumpCard;
            _controller.Context.TrumpSuit = trumpCard.IsJoker ? Suit.Diamonds : trumpCard.Suit;

            if (trumpCard.Rank == Rank.Seven && !trumpCard.IsJoker)
            {
                _controller.Players[_controller.PlayerCount - 1].RoundScore += 10;
            }
        }
    }

    private void DealCardsToAllPlayers(int cardsPerPlayer)
    {
        for (int i = 0; i < _controller.PlayerCount; i++)
        {
            for (int j = 0; j < cardsPerPlayer; j++)
            {
                if (_controller.Context.DrawDeck.Count > 0)
                {
                    _controller.Players[i].Hand.Add(_controller.Context.DrawDeck.Pop());
                }
            }
        }
    }

    // Play Phase Methods
    public void PlayFirstCard()
    {
        // First player plays - FSM entry point
    }

    public void PlayMidCard()
    {
        // Advance turn after first/middle player
        _controller.Context.CurrentTurnPlayer = TurnManager.AdvanceTurn(_controller.Context.CurrentTurnPlayer, _controller.PlayerCount);
    }

    public void PlayLastCard()
    {
        // For 2-player games: advance to second player's turn before they play
        // For 4-player games: advance to last player's turn (but card was already played by this point)
        // The trick is already complete by the time we're called - just resolve it
        _cardsPlayedThisTrick++;
        ResolveTrickInternal();
    }

    // Meld Phase Methods
    public void TryMeld()
    {
        // State machine enters TryMelded state - awaiting external meld declaration
    }

    public void MeldSuccess()
    {
        // Meld was successfully declared, now dispatch COMPLETE to move to NewTrick
        _stateMachine.DispatchEvent(Bezique.EventId.COMPLETE);
    }

    public void MeldFailed()
    {
        // Meld was skipped or failed, dispatch COMPLETE to move to NewTrick
        _stateMachine.DispatchEvent(Bezique.EventId.COMPLETE);
    }

    // Trick Transition Methods
    public void StartNewTrick()
    {
        _controller.PlayedCards.Clear();
        _controller.Context.CurrentTurnPlayer = _controller.Context.LastTrickWinner;
        _cardsPlayedThisTrick = 0;
    }

    public void StartL9NewTrick()
    {
        _controller.PlayedCards.Clear();
        _controller.Context.CurrentTurnPlayer = _controller.Context.LastTrickWinner;
        _cardsPlayedThisTrick = 0;
    }

    public void DrawCardsForAll()
    {
        int winnerId = _controller.Context.LastTrickWinner;

        // EXECUTE LOGIC HERE
        bool transitioned = PhaseTransitionManager.ExecuteDraw(
            _controller.Players,
            winnerId,
            _controller.Context.TrumpCard,
            _controller.Context.DrawDeck,
            _controller.PlayerCount
        );

        // DECIDE NEXT STATE
        if (transitioned)
        {
            _controller.Context.CurrentPhase = GamePhase.Phase2_Last9;
            _controller.OnPhaseChanged(GamePhase.Phase2_Last9);

            // Transition FSM: AddOneCardToAll -> L9Play
            _stateMachine.DispatchEvent(Bezique.EventId.DECKEMPTY);
        }
        else
        {
            // Transition FSM: AddOneCardToAll -> Play
            _stateMachine.DispatchEvent(Bezique.EventId.COMPLETE);
        }
    }

    // L9 Play Phase Methods (Last 9 cards, no drawing)
    public void L9PlayFirstCard()
    {
        _controller.Context.CurrentPhase = GamePhase.Phase2_Last9;
        _cardsPlayedThisTrick = 1;

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
        // Last card played - trick complete, resolve it
        _cardsPlayedThisTrick++;
        ResolveTrickInternal();
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

        // Dispatch event to state machine based on current FSM state and cards played
        int cardsInTrick = _controller.PlayedCards.Count;
        var currentState = _stateMachine.stateId;

        // The FSM advances through states based on COMPLETE events
        // For 2-player: PlayFirst (stay) -> PlayFirst (stay) -> PlayLast -> resolve
        // For 4-player: PlayFirst -> PlayMid -> PlayMid -> PlayLast -> resolve
        //
        // We dispatch COMPLETE to move to the NEXT state when enough cards have been played

        if (_controller.Context.CurrentPhase == GamePhase.Phase2_Last9)
        {
            // Phase 2 logic
            if (currentState == Bezique.StateId.L9PLAYFIRST)
            {
                // Advance turn for next player (FSM entry method doesn't do this for us)
                _controller.Context.CurrentTurnPlayer = TurnManager.AdvanceTurn(_controller.Context.CurrentTurnPlayer, _controller.PlayerCount);

                if (_controller.PlayerCount == 2)
                {
                    // 2-player: Stay in L9PlayFirst until 2 cards played
                    if (cardsInTrick == 2)
                    {
                        _stateMachine.DispatchEvent(Bezique.EventId.COMPLETE); // L9PlayFirst -> L9PlayLast
                    }
                    // else: stay in L9PlayFirst, don't dispatch
                }
                else
                {
                    // 4-player: Move to L9PlayMid after first card
                    _stateMachine.DispatchEvent(Bezique.EventId.HASMID);
                }
            }
            else if (currentState == Bezique.StateId.L9PLAYMID)
            {
                if (cardsInTrick < _controller.PlayerCount)
                {
                    _stateMachine.DispatchEvent(Bezique.EventId.COMPLETE); // L9PlayMid -> L9PlayMid
                }
                else
                {
                    _stateMachine.DispatchEvent(Bezique.EventId.COMPLETE); // L9PlayMid -> L9PlayLast
                }
            }
            else if (currentState == Bezique.StateId.L9PLAYLAST)
            {
                _stateMachine.DispatchEvent(Bezique.EventId.COMPLETE); // L9PlayLast -> resolve trick
            }
        }
        else
        {
            // Phase 1 logic
            if (currentState == Bezique.StateId.PLAYFIRST)
            {
                // Advance turn for next player (FSM entry method doesn't do this for us)
                _controller.Context.CurrentTurnPlayer = TurnManager.AdvanceTurn(_controller.Context.CurrentTurnPlayer, _controller.PlayerCount);

                if (_controller.PlayerCount == 2)
                {
                    // 2-player: Stay in PlayFirst until 2 cards played
                    if (cardsInTrick == 2)
                    {
                        _stateMachine.DispatchEvent(Bezique.EventId.COMPLETE); // PlayFirst -> PlayLast
                    }
                    // else: stay in PlayFirst, don't dispatch
                }
                else
                {
                    // 4-player: Move to PlayMid after first card
                    _stateMachine.DispatchEvent(Bezique.EventId.HASMID);
                }
            }
            else if (currentState == Bezique.StateId.PLAYMID)
            {
                if (cardsInTrick < _controller.PlayerCount)
                {
                    _stateMachine.DispatchEvent(Bezique.EventId.COMPLETE); // PlayMid -> PlayMid
                }
                else
                {
                    _stateMachine.DispatchEvent(Bezique.EventId.COMPLETE); // PlayMid -> PlayLast
                }
            }
            else if (currentState == Bezique.StateId.PLAYLAST)
            {
                _stateMachine.DispatchEvent(Bezique.EventId.COMPLETE); // PlayLast -> resolve trick
            }
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

        int points = _controller.Players[currentPlayer].RoundScore - beforeScore;
        _controller.OnMeldDeclared(currentPlayer, meldType, points);

        // Dispatch SUCCESS to move FSM from TryMelded -> Melded
        // FSM will then call MeldSuccess() which dispatches COMPLETE to move to NewTrick
        _stateMachine.DispatchEvent(Bezique.EventId.SUCCESS);

        return true;
    }

    public void SkipMeld()
    {
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

    public int EndRound()
    {
        int winnerId = RoundEndHandler.EndRound(_controller.Players, _controller.Context.GameMode, _controller.PlayerCount);
        _controller.OnRoundEnded(winnerId, _controller.Players.Select(p => p.TotalScore).ToArray());

        // Notify state machine that round ended
        _stateMachine.DispatchEvent(Bezique.EventId.COMPLETE);

        return winnerId;
    }

    public void DrawCards()
    {
        // Validation: Can only draw if deck has cards
        if (_controller.Context.DrawDeck.Count == 0) return;

        // ACTION: Tell FSM to move from 'NewTrick' to 'AddOneCardToAll'.
        // The FSM will then automatically call 'DrawCardsForAll()' below.
        _stateMachine.DispatchEvent(Bezique.EventId.COMPLETE);
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

        // Dispatch appropriate FSM event based on game state
        if (isFinalTrick)
        {
            // Final trick of the round - transition to RoundEnd
            _stateMachine.DispatchEvent(Bezique.EventId.NOCARD);
        }
        else
        {
            // Dispatch COMPLETE to transition from PlayLast to Meld (Phase 1) or L9PlayLast to L9NewTrick (Phase 2)
            _stateMachine.DispatchEvent(Bezique.EventId.COMPLETE);
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
