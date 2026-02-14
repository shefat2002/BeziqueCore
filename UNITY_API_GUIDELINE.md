# Unity High-Level API Implementation Guideline

## Overview

This document provides a comprehensive guideline for implementing a high-level API that allows Unity developers to consume the BeziqueCore SDK without dealing with FSM internals, event dispatching, or state machine complexities.

---

## Current State Analysis

### What BeziqueCore Currently Exposes

| Class/Interface | Purpose | Public API |
|-----------------|----------|------------|
| `Bezique` | Auto-generated FSM | `Start()`, `DispatchEvent(EventId)`, `stateId` |
| `BeziqueConcrete` | Adapter holding game data | Constructor, `Dealer`, `Player[]`, `Scores[]`, `TrumpCard`, `TrumpSuit` |
| `IBeziqueAdapter` | Adapter interface | Methods called BY FSM, not by Unity devs |
| `CardHelper` | Card ID utilities | Static helper methods |
| `IRandom` | Randomization abstraction | `Next(int)`, `Next(int, int)` |

### Problems for Unity Developers

1. **No Shuffling**: Cards are created in predictable order, no shuffle function exists
2. **FSM Complexity**: Must understand and dispatch FSM events manually
3. **Internal Access**: Game state stored in `public readonly` fields - no encapsulation
4. **Missing Lifecycle**: No `StartGame()`, `PlayCard()`, `DrawCard()` style methods
5. **No Event System**: Unity can't subscribe to game state changes

---

## Proposed High-Level API Design

### Design Principles

1. **Unity-Friendly**: Match Unity coding patterns (MonoBehaviour integration)
2. **Event-Driven**: Use C# events/callbacks for state changes
3. **Immutable State**: Game state returned as DTOs, not direct field access
4. **Simple Actions**: Intuitive methods like `PlayCard()`, not FSM events
5. **Async-Friendly**: Support coroutines and async/await patterns

---

## API Specification

### 1. Main Game Class

```csharp
namespace BeziqueCore.Unity;

/// <summary>
/// High-level API for Unity developers to manage Bezique games.
/// Hides FSM complexity and provides simple game control methods.
/// </summary>
public class BeziqueGame
{
    // ===== PROPERTIES =====

    /// <summary>
    /// Current game state (read-only)
    /// </summary>
    public GameState State { get; }

    /// <summary>
    /// Number of players in this game
    /// </summary>
    public int PlayerCount { get; }

    /// <summary>
    /// Target score to win the game
    /// </summary>
    public int TargetScore { get; set; } = 1500;

    // ===== EVENTS =====

    /// <summary>
    /// Fired when game state changes
    /// </summary>
    public event Action<GameState> OnGameStateChanged;

    /// <summary>
    /// Fired when a card is played
    /// </summary>
    public event Action<CardPlayInfo> OnCardPlayed;

    /// <summary>
    /// Fired when a trick completes
    /// </summary>
    public event Action<TrickResult> OnTrickComplete;

    /// <summary>
    /// Fired when meld points are awarded
    /// </summary>
    public event Action<MeldInfo> OnMeldScored;

    /// <summary>
    /// Fired when round ends
    /// </summary>
    public event Action<RoundResult> OnRoundComplete;

    /// <summary>
    /// Fired when game ends
    /// </summary>
    public event Action<GameResult> OnGameComplete;

    // ===== CONSTRUCTORS =====

    /// <summary>
    /// Create a new Bezique game instance
    /// </summary>
    /// <param name="playerCount">Number of players (2 or 4)</param>
    /// <param name="randomSeed">Optional seed for reproducible games</param>
    public BeziqueGame(int playerCount, int? randomSeed = null);

    // ===== CORE METHODS =====

    /// <summary>
    /// Initialize and start a new game
    /// </summary>
    /// <param name="shuffleDeck">Whether to shuffle the deck (default: true)</param>
    public void StartNewGame(bool shuffleDeck = true);

    /// <summary>
    /// Play a card from the current player's hand
    /// </summary>
    /// <param name="cardId">The card ID to play</param>
    /// <returns>Result of the play action</returns>
    public PlayResult PlayCard(byte cardId);

    /// <summary>
    /// Play a card by its index in the player's hand
    /// </summary>
    /// <param name="handIndex">Index of card in current player's hand</param>
    /// <returns>Result of the play action</returns>
    public PlayResult PlayCardByIndex(int handIndex);

    /// <summary>
    /// Draw a card from the deck (called after trick completion)
    /// </summary>
    /// <returns>Result of the draw action</returns>
    public DrawCardResult DrawCard();

    /// <summary>
    /// Submit a meld for scoring (Phase 1 only)
    /// </summary>
    /// <param name="meld">The meld to score</param>
    /// <returns>Result of the meld action</returns>
    public MeldResult SubmitMeld(MeldDeclaration meld);

    /// <summary>
    /// Swap the trump 7 with the trump card on table
    /// </summary>
    /// <returns>True if swap was successful</returns>
    public bool SwapTrump7();

    /// <summary>
    /// Get the current player's hand
    /// </summary>
    /// <param name="playerIndex">Player index (0-based)</param>
    /// <returns>List of card IDs in player's hand</returns>
    public IReadOnlyList<byte> GetPlayerHand(int playerIndex);

    /// <summary>
    /// Get cards currently on the table (in play)
    /// </summary>
    /// <returns>List of cards played in current trick</returns>
    public IReadOnlyList<byte> GetTableCards();

    /// <summary>
    /// Get scores for all players
    /// </summary>
    /// <returns>Array of scores indexed by player</returns>
    public IReadOnlyList<int> GetScores();

    /// <summary>
    /// Get the current trump card
    /// </summary>
    /// <returns>Trump card ID, or null if not set</returns>
    public byte? GetTrumpCard();

    /// <summary>
    /// Get cards remaining in the deck
    /// </summary>
    /// <returns>Number of cards left in deck</returns>
    public int GetDeckCount();

    /// <summary>
    /// Validate if a card can be played (for UI highlighting)
    /// </summary>
    /// <param name="cardId">Card to validate</param>
    /// <returns>True if card is playable</returns>
    public bool CanPlayCard(byte cardId);
}
```

### 2. Data Transfer Objects (DTOs)

```csharp
namespace BeziqueCore.Unity;

/// <summary>
/// Snapshot of the complete game state
/// </summary>
public class GameState
{
    public GamePhase Phase { get; set; }
    public int CurrentPlayerIndex { get; set; }
    public int DealerIndex { get; set; }
    public int LeadPlayerIndex { get; set; }
    public byte? TrumpCard { get; set; }
    public byte? TrumpSuit { get; set; }
    public int TrickNumber { get; set; }
    public bool IsMeldAllowed { get; set; }
    public IReadOnlyList<int> Scores { get; set; }
    public IReadOnlyList<int> MeldScores { get; set; }
    public IReadOnlyList<IReadOnlyList<byte>> PlayerHands { get; set; }
    public IReadOnlyList<byte> TableCards { get; set; }
    public IReadOnlyList<MeldedCard> ActiveMelds { get; set; }
}

public enum GamePhase
{
    NotStarted,
    Dealing,
    Phase1_NormalPlay,
    Phase1_Drawing,
    Phase2_Last9,
    RoundEnd,
    GameEnd
}

/// <summary>
/// Information about a played card
/// </summary>
public class CardPlayInfo
{
    public int PlayerIndex { get; set; }
    public byte CardId { get; set; }
    public int PositionInTrick { get; set; }
}

/// <summary>
/// Result of a trick completion
/// </summary>
public class TrickResult
{
    public int WinnerIndex { get; set; }
    public IReadOnlyList<byte> CardsPlayed { get; set; }
    public int PointsAwarded { get; set; }
}

/// <summary>
/// Information about a meld that was scored
/// </summary>
public class MeldInfo
{
    public int PlayerIndex { get; set; }
    public MeldType MeldType { get; set; }
    public int Points { get; set; }
    public IReadOnlyList<byte> Cards { get; set; }
}

/// <summary>
/// Result of a play card action
/// </summary>
public class PlayResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public GameState NewState { get; set; }
}

/// <summary>
/// Result of a draw card action
/// </summary>
public class DrawCardResult
{
    public bool Success { get; set; }
    public byte? DrawnCard { get; set; }
    public bool MoreCardsToDraw { get; set; }
}

/// <summary>
/// Result of a meld action
/// </summary>
public class MeldResult
{
    public bool Success { get; set; }
    public int PointsAwarded { get; set; }
    public string ErrorMessage { get; set; }
}

/// <summary>
/// Result of a round
/// </summary>
public class RoundResult
{
    public IReadOnlyList<int> FinalScores { get; set; }
    public int WinnerIndex { get; set; }
    public bool GameEnded { get; set; }
}

/// <summary>
/// Result of the entire game
/// </summary>
public class GameResult
{
    public IReadOnlyList<int> FinalScores { get; set; }
    public IReadOnlyList<int> WinnerIndices { get; set; }
    public int TotalRounds { get; set; }
}

/// <summary>
/// Declaration of a meld by a player
/// </summary>
public class MeldDeclaration
{
    public MeldType Type { get; set; }
    public IReadOnlyList<byte> CardIds { get; set; }
}

public enum MeldType
{
    TrumpRun,        // 250 pts: A, 10, K, Q, J of trump
    TrumpMarriage,   // 40 pts: K, Q of trump
    Marriage,         // 20 pts: K, Q of same suit
    Bezique,         // 40 pts: Q Spades + J Diamonds
    DoubleBezique,   // 500 pts: 2x Q Spades + 2x J Diamonds
    FourAces,        // 100 pts
    FourKings,       // 80 pts
    FourQueens,       // 60 pts
    FourJacks        // 40 pts
}

/// <summary>
/// A card that has been melded to the table
/// </summary>
public class MeldedCard
{
    public byte CardId { get; set; }
    public MeldType MeldType { get; set; }
    public int PlayerIndex { get; set; }
}
```

### 3. Unity Integration Helper

```csharp
namespace BeziqueCore.Unity;

using UnityEngine;

/// <summary>
/// MonoBehaviour wrapper for easy Unity integration
/// Attach this component to a GameObject in your scene
/// </summary>
public class BeziqueGameComponent : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int playerCount = 2;
    [SerializeField] private int targetScore = 1500;
    [SerializeField] private bool useFixedSeed;
    [SerializeField] private int fixedSeed;

    private BeziqueGame _game;

    // ===== UNITY EVENTS =====
    // Assign these in the Inspector

    [Header("Events")]
    public UnityEvent onGameStart = new();
    public UnityEvent<int> onTurnStart = new();          // Player index
    public UnityEvent onTurnEnd = new();
    public UnityEvent<int> onTrickComplete = new();      // Winner index
    public UnityEvent<int, int> onScoreChanged = new(); // Player, new score
    public UnityEvent<int> onRoundComplete = new();      // Winner index
    public UnityEvent onGameComplete = new();

    // ===== PUBLIC API =====

    public BeziqueGame Game => _game;

    /// <summary>
    /// Start a new game (call from button or initialization)
    /// </summary>
    public void StartGame()
    {
        int? seed = useFixedSeed ? fixedSeed : null;
        _game = new BeziqueGame(playerCount, seed);
        _game.TargetScore = targetScore;

        // Wire up internal events to Unity events
        _game.OnGameComplete += HandleGameComplete;
        _game.OnTrickComplete += HandleTrickComplete;
        _game.OnRoundComplete += HandleRoundComplete;

        _game.StartNewGame();
        onGameStart.Invoke();
    }

    /// <summary>
    /// Play a card from UI button click
    /// </summary>
    public void PlayCard(byte cardId)
    {
        var result = _game.PlayCard(cardId);
        if (!result.Success)
        {
            Debug.LogWarning($"Play failed: {result.ErrorMessage}");
        }
    }

    /// <summary>
    /// Submit a meld from UI
    /// </summary>
    public void SubmitMeld(MeldDeclaration meld)
    {
        var result = _game.SubmitMeld(meld);
        if (!result.Success)
        {
            Debug.LogWarning($"Meld failed: {result.ErrorMessage}");
        }
    }

    // ===== EVENT HANDLERS =====

    private void HandleGameComplete(GameResult result)
    {
        Debug.Log($"Game Over! Winner: Player {result.WinnerIndices[0]}");
        onGameComplete.Invoke();
    }

    private void HandleTrickComplete(TrickResult result)
    {
        Debug.Log($"Trick won by Player {result.WinnerIndex}");
        onTrickComplete.Invoke(result.WinnerIndex);
    }

    private void HandleRoundComplete(RoundResult result)
    {
        Debug.Log($"Round complete! Winner: Player {result.WinnerIndex}");
        onRoundComplete.Invoke(result.WinnerIndex);
    }
}
```

---

## Implementation Plan

### Phase 1: Core Game Wrapper (Foundation)

**File**: `BeziqueGame.cs`

| Task | Description |
|------|-------------|
| 1.1 | Create `BeziqueGame` class with constructor accepting player count and seed |
| 1.2 | Initialize `BeziqueConcrete` and `Bezique` FSM internally |
| 1.3 | Implement `StartNewGame()` with shuffle logic using `IRandom` |
| 1.4 | Dispatch `TWOPLAYERGAME` or `FOURPLAYERGAME` event based on player count |
| 1.5 | Implement FSM state tracking and conversion to `GameState` DTO |

**Acceptance Criteria**:
- Calling `StartNewGame()` results in fully initialized game
- Cards are shuffled when shuffle=true
- FSM reaches `FLIPTRUMP` state automatically
- GameState reflects accurate initial conditions

### Phase 2: Card Play Mechanics

**File**: `BeziqueGame.cs` (continued)

| Task | Description |
|------|-------------|
| 2.1 | Implement `PlayCard()` and `PlayCardByIndex()` |
| 2.2 | Add validation: check if it's player's turn and card is in hand |
| 2.3 | Set `BeziqueConcrete.PlayerCardIndex` before dispatching FSM event |
| 2.4 | Dispatch `PLAYERTURN` event and handle FSM transitions |
| 2.5 | Return `PlayResult` with new state or error message |
| 2.6 | Implement `CanPlayCard()` for UI validation |

**Acceptance Criteria**:
- Invalid plays return error without changing state
- Valid plays advance FSM correctly
- State updates are reflected in returned `PlayResult`

### Phase 3: Drawing & Trick Resolution

**File**: `BeziqueGame.cs` (continued)

| Task | Description |
|------|-------------|
| 3.1 | Implement `DrawCard()` method |
| 3.2 | Handle `ALLPLAYERPLAYED` FSM event dispatch |
| 3.3 | Track trick completion and determine winner |
| 3.4 | Fire `OnTrickComplete` event with winner info |
| 3.5 | Handle transition between drawing for winner vs other players |

**Acceptance Criteria**:
- Winner draws first, then other players in order
- `OnTrickComplete` fires with correct winner
- GameState updates after each draw

### Phase 4: Melding System

**File**: `BeziqueGame.cs`, `MeldValidator.cs`, `MeldScorer.cs`

| Task | Description |
|------|-------------|
| 4.1 | Create `MeldValidator` class to validate meld combinations |
| 4.2 | Create `MeldScorer` class to calculate meld points |
| 4.3 | Implement `SubmitMeld()` in `BeziqueGame` |
| 4.4 | Handle Trump 7 swap logic in `SwapTrump7()` |
| 4.5 | Fire `OnMeldScored` event with meld info |
| 4.6 | Track melded cards and prevent reuse |

**Acceptance Criteria**:
- Invalid melds return appropriate errors
- Valid melds award correct points per game rules
- Trump 7 swap works correctly
- Melded cards tracked properly

### Phase 5: Phase 2 (Last 9 Cards)

**File**: `BeziqueGame.cs` (continued)

| Task | Description |
|------|-------------|
| 5.1 | Detect Phase 2 transition (deck has `PlayerCount` cards left) |
| 5.2 | Handle special last card distribution (winner gets hidden, loser gets trump) |
| 5.3 | Implement Phase 2 strict validation logic |
| 5.4 | Enforce follow-suit rules in `CanPlayCard()` for Phase 2 |
| 5.5 | Handle last trick bonus (+10 or +20 for trump 7) |

**Acceptance Criteria**:
- Phase 2 triggers at correct deck count
- Final card distribution follows rules
- Phase 2 validation enforces all strict rules
- Last trick bonus awarded correctly

### Phase 6: Round & Game Completion

**File**: `BeziqueGame.cs` (continued), `ScoreCalculator.cs`

| Task | Description |
|------|-------------|
| 6.1 | Create `ScoreCalculator` for end-of-round scoring |
| 6.2 | Count Aces and Tens in won tricks |
| 6.3 | Implement advanced mode threshold rules |
| 6.4 | Check target score and determine game end |
| 6.5 | Fire `OnRoundComplete` and `OnGameComplete` events |
| 6.6 | Handle multi-round games with score accumulation |

**Acceptance Criteria**:
- Round scoring matches rules (10 pts per Ace/Ten)
- Advanced mode thresholds enforced
- Game ends when target score reached
- Events fire with complete result information

### Phase 7: Event System

**File**: `BeziqueGame.cs` (continued)

| Task | Description |
|------|-------------|
| 7.1 | Add all event declarations to `BeziqueGame` |
| 7.2 | Fire events at appropriate points in game flow |
| 7.3 | Ensure events include relevant context (state, cards, scores) |
| 7.4 | Add thread-safety for event invocation |

**Acceptance Criteria**:
- All defined events fire correctly
- Event payloads contain accurate information
- Multiple subscribers can be added/removed safely

### Phase 8: Unity Integration Layer

**File**: `BeziqueGameComponent.cs`

| Task | Description |
|------|-------------|
| 8.1 | Create MonoBehaviour wrapper class |
| 8.2 | Add Inspector-visible settings (player count, seed, etc.) |
| 8.3 | Expose UnityEvents for common game events |
| 8.4 | Implement wrapper methods for core API |
| 8.5 | Add debug logging for common scenarios |

**Acceptance Criteria**:
- Component can be added to GameObject
- Settings configurable in Inspector
- UnityEvents fire correctly from game events
- No direct FSM knowledge required

### Phase 9: Validation & Testing

**File**: `BeziqueGameValidator.cs`, Test project

| Task | Description |
|------|-------------|
| 9.1 | Create state validator for game invariants |
| 9.2 | Add unit tests for all public methods |
| 9.3 | Add integration tests for complete game flow |
| 9.4 | Test edge cases (empty deck, invalid moves, etc.) |
| 9.5 | Performance test for long games |

**Acceptance Criteria**:
- All unit tests pass
- Integration tests cover complete game scenarios
- No performance regressions vs direct FSM access

### Phase 10: Documentation & Examples

**File**: README.md, Examples/

| Task | Description |
|------|-------------|
| 10.1 | Write API documentation with XML comments |
| 10.2 | Create simple example scene for Unity |
| 10.3 | Add "Getting Started" guide |
| 10.4 | Document all events and their payloads |
| 10.5 | Create troubleshooting guide |

**Acceptance Criteria**:
- All public APIs have XML documentation
- Example scene runs without errors
- README allows developers to start in <5 minutes

---

## Implementation Checklist

### New Files to Create

- [ ] `BeziqueGame.cs` - Main high-level API
- [ ] `BeziqueGameComponent.cs` - Unity MonoBehaviour wrapper
- [ ] `MeldValidator.cs` - Meld validation logic
- [ ] `MeldScorer.cs` - Meld point calculation
- [ ] `ScoreCalculator.cs` - End-of-round scoring
- [ ] `GameState.cs` - DTOs and enums
- [ ] `CardPlayValidator.cs` - Card play validation (Phase 2)
- [ ] `BeziqueGameValidator.cs` - Game state validation
- [ ] `Unity/BeziqueUnity.asmdef` - Assembly definition
- [ ] `Examples/BeziqueExampleScene.unity` - Example scene

### Files to Modify

- [ ] `BeziqueConcrete.cs` - Add internal properties for tracking
- [ ] `CardHelper.cs` - Add utility methods if needed
- [ ] `AdapterResult.cs` - Add any missing result types

---

## Usage Example (After Implementation)

```csharp
using BeziqueCore.Unity;
using UnityEngine;

public class ExampleGameManager : MonoBehaviour
{
    private BeziqueGameComponent gameComponent;

    void Start()
    {
        gameComponent = GetComponent<BeziqueGameComponent>();

        // Subscribe to events
        gameComponent.onTrickComplete.AddListener(OnTrickComplete);
        gameComponent.onGameComplete.AddListener(OnGameComplete);

        // Start the game
        gameComponent.StartGame();
    }

    // Called from UI when player clicks a card
    public void OnCardClick(byte cardId)
    {
        gameComponent.PlayCard(cardId);
    }

    private void OnTrickComplete(int winnerIndex)
    {
        Debug.Log($"Player {winnerIndex} won the trick!");
        UpdateUI();
    }

    private void OnGameComplete()
    {
        Debug.Log("Game over!");
        ShowGameOverScreen();
    }
}
```

---

## Notes

1. **Thread Safety**: The underlying FSM is not thread-safe. The high-level API should document this limitation.
2. **Serialization**: Consider adding `ToJson()`/`FromJson()` methods for save/load functionality.
3. **AI Integration**: The API should support AI players by allowing programmatic card selection.
4. **Networking**: While multiplayer isn't supported yet, design the API to be network-friendly (state snapshots, deterministic events).
5. **Error Messages**: Provide clear, actionable error messages for common mistakes.

---

## Next Steps

1. Review this guideline with the team
2. Prioritize phases based on Unity developer feedback
3. Create feature branches for each phase
4. Establish code review process
5. Set up CI/CD for automated testing
