# BeziqueCore SDK - Unity API Documentation

## Quick Start

```csharp
using BeziqueCore;

// Create and initialize game
var game = new BeziqueGameController();
var config = new GameConfig {
    PlayerCount = 2,
    TargetScore = 1500,
    Mode = GameMode.Standard,
    DeckCount = 4
};
game.Initialize(config);

// Subscribe to events
game.TrickEnded += OnTrickEnded;
game.PhaseChanged += OnPhaseChanged;
game.MeldDeclared += OnMeldDeclared;
game.RoundEnded += OnRoundEnded;
game.GameEnded += OnGameEnded;
```

---

## Table of Contents

1. [Initialization](#initialization)
2. [Public Properties](#public-properties)
3. [Game Actions](#game-actions)
4. [Query Methods](#query-methods)
5. [Events](#events)
6. [Data Models](#data-models)
7. [Complete Gameplay Flow](#complete-gameplay-flow)
8. [Unity Integration Example](#unity-integration-example)

---

## Initialization

### `void Initialize(GameConfig config)`

Initialize the game with configuration.

**Parameters:**
- `config.PlayerCount` (byte): Number of players (2 or 4)
- `config.TargetScore` (ushort): Winning score (default: 1500)
- `config.Mode` (GameMode): Standard or Advanced
- `config.DeckCount` (byte): Number of decks (always 4)

```csharp
var config = new GameConfig {
    PlayerCount = 2,
    TargetScore = 1500,
    Mode = GameMode.Standard,
    DeckCount = 4
};
game.Initialize(config);
```

**After Initialization:**
- 4 decks (132 cards) are shuffled
- 9 cards dealt to each player (3 sets of 3)
- Trump card selected and displayed
- FSM enters Play state

---

## Public Properties

| Property | Type | Description |
|----------|------|-------------|
| `Players` | `Player[]` | Array of all players |
| `Context` | `GameContext` | Game context (deck, trump, phase, turn) |
| `PlayedCards` | `List<Card>` | Cards played in current trick |
| `PlayerCount` | `byte` | Number of players (2 or 4) |
| `TargetScore` | `ushort` | Score needed to win |
| `CurrentState` | `GameState` | Current FSM state |
| `CurrentPlayer` | `int` | Index of current player (0-based) |
| `LastWinner` | `int` | Index of last trick winner |
| `IsPhase2` | `bool` | True if in Phase 2 (Last 9 cards) |
| `IsGameOver` | `bool` | True if game has ended |

---

## Game Actions

### Card Play

#### `bool PlayCard(Card card)`
Play a card from the current player's hand or table cards.

**Returns:** `true` if card was played successfully

```csharp
var card = game.Players[game.CurrentPlayer].Hand[0];
if (game.PlayCard(card)) {
    // Card played successfully
}
```

---

### Melding

#### `bool DeclareMeld(Card[] cards, MeldType meldType)`
Declare a meld (only trick winner can meld).

**Returns:** `true` if meld was declared successfully

**Meld Types:**
- `MeldType.TrumpRun` (250 pts) - A, 10, K, Q, J of Trump
- `MeldType.DoubleBezique` (500 pts) - 2x Q♠ + 2x J♦
- `MeldType.FourAces` (100 pts) - 4 Aces
- `MeldType.FourKings` (80 pts) - 4 Kings
- `MeldType.FourQueens` (60 pts) - 4 Queens
- `MeldType.FourJacks` (40 pts) - 4 Jacks
- `MeldType.Bezique` (40 pts) - Q♠ + J♦
- `MeldType.TrumpMarriage` (40 pts) - K + Q of Trump
- `MeldType.Marriage` (20 pts) - K + Q of same non-trump suit

```csharp
var bestMeld = game.GetBestMeld();
if (bestMeld != null) {
    game.DeclareMeld(bestMeld.Cards.ToArray(), bestMeld.Type);
}
```

#### `void SkipMeld()`
Skip the meld phase and continue to next trick.

```csharp
if (!game.CanMeld()) {
    game.SkipMeld();
}
```

#### `bool CanMeld()`
Check if the current player is allowed to meld (trick winner only).

**Returns:** `true` if melding is allowed

---

### Trump Seven Swap

#### `bool CanSwapTrumpSeven()`
Check if current player can swap their Trump Seven with the trump card.

**Returns:** `true` if swap is available

#### `bool SwapTrumpSeven()`
Swap the Trump Seven from hand with the face-up trump card.

**Returns:** `true` if swap was successful (+10 points)

```csharp
if (game.CanSwapTrumpSeven()) {
    game.SwapTrumpSeven(); // +10 points
}
```

---

### Drawing Cards

#### `void DrawCards()`
Trigger card drawing after a trick (Phase 1 only).

**Note:** Automatically handled after each trick in Phase 1. No manual call needed in normal gameplay.

---

### Round Management

#### `int EndRound()`
End the current round and calculate scores.

**Returns:** Index of the round winner

```csharp
int winnerId = game.EndRound();
Debug.Log($"Round winner: Player {winnerId + 1}");
```

#### `int CheckWinner()`
Check if any player has reached the target score.

**Returns:** Index of winning player, or `-1` if no winner yet

---

## Query Methods

### `Card[] GetLegalMoves()`
Get all legal cards the current player can play.

**Returns:** Array of playable cards

```csharp
var legalMoves = game.GetLegalMoves();
foreach (var card in legalMoves) {
    // Highlight card in UI
}
```

---

### `MeldOpportunity? GetBestMeld()`
Get the best available meld for the current player.

**Returns:** `MeldOpportunity` with cards, type, and points, or `null`

```csharp
var bestMeld = game.GetBestMeld();
if (bestMeld != null) {
    Debug.Log($"Best meld: {bestMeld.Type} for {bestMeld.Points} points");
}
```

---

### `bool CheckPhaseTransition()`
Check if the game should transition to Phase 2.

**Returns:** `true` when deck has `PlayerCount` cards left

---

### `bool IsPlayerTurn(int playerId)`
Check if it's the specified player's turn.

---

## Events

### `event EventHandler<TrickEndedEventArgs> TrickEnded`

Fired when a trick ends.

```csharp
void OnTrickEnded(object sender, TrickEndedEventArgs e)
{
    int winnerId = e.WinnerId;
    bool isFinalTrick = e.IsFinalTrick;
    Debug.Log($"Player {winnerId + 1} wins trick! Final: {isFinalTrick}");
}
```

---

### `event EventHandler<PhaseChangedEventArgs> PhaseChanged`

Fired when game phase changes (Normal → Last 9).

```csharp
void OnPhaseChanged(object sender, PhaseChangedEventArgs e)
{
    GamePhase newPhase = e.NewPhase;
    Debug.Log($"Phase changed to: {newPhase}");
}
```

---

### `event EventHandler<MeldDeclaredEventArgs> MeldDeclared`

Fired when a player successfully declares a meld.

```csharp
void OnMeldDeclared(object sender, MeldDeclaredEventArgs e)
{
    int playerId = e.PlayerId;
    MeldType meldType = e.MeldType;
    int points = e.Points;
    Debug.Log($"Player {playerId + 1} melded {meldType} (+{points} pts)");
}
```

---

### `event EventHandler<RoundEndedEventArgs> RoundEnded`

Fired when a round ends.

```csharp
void OnRoundEnded(object sender, RoundEndedEventArgs e)
{
    int winnerId = e.WinnerId;
    int[] scores = e.Scores;
    Debug.Log($"Round winner: Player {winnerId + 1}");
}
```

---

### `event EventHandler<GameEndedEventArgs> GameEnded`

Fired when the game ends (target score reached).

```csharp
void OnGameEnded(object sender, GameEndedEventArgs e)
{
    int winnerId = e.WinnerId;
    Debug.Log($"Game over! Winner: Player {winnerId + 1}");
}
```

---

## Data Models

### `Player`

```csharp
public class Player
{
    public int PlayerID { get; }              // Player index (0-based)
    public List<Card> Hand { get; }           // Cards in hand
    public List<Card> TableCards { get; }      // Melded cards on table
    public List<Card> WonPile { get; }        // Cards won from tricks
    public int RoundScore { get; set; }       // Score this round
    public int TotalScore { get; set; }       // Cumulative score
    public bool HasSwappedSeven { get; set; }  // Already swapped trump seven?
    public Dictionary<MeldType, List<Card>> MeldHistory { get; }
}
```

---

### `Card`

```csharp
public struct Card
{
    public byte DeckIndex { get; }    // 0-3 (which deck)
    public Suit Suit { get; }         // Spades, Hearts, Diamonds, Clubs
    public Rank Rank { get; }         // Seven(7) through Ace(14)
    public bool IsJoker { get; }
    public int CardId { get; }        // Unique identifier
}
```

---

### `GameContext`

```csharp
public class GameContext
{
    public Stack<Card> DrawDeck { get; set; }
    public Card TrumpCard { get; set; }
    public Suit TrumpSuit { get; set; }
    public GamePhase CurrentPhase { get; set; }
    public int CurrentTurnPlayer { get; set; }
    public int LastTrickWinner { get; set; }
    public GameMode GameMode { get; set; }
}
```

---

## Enums

```csharp
enum GameState { Deal, Play, Meld, NewTrick, L9Play, L9NewTrick, RoundEnd, GameOver }

enum GamePhase { Phase1_Normal, Phase2_Last9 }

enum GameMode { Standard, Advanced }

enum Suit { None, Spades, Hearts, Diamonds, Clubs }

enum Rank { Seven=7, Eight=8, Nine=9, Ten=13, Jack=10, Queen=11, King=12, Ace=14 }
```

---

## Complete Gameplay Flow

```
1. Initialize()
   ├─ FSM deals 9 cards to each player (3 sets of 3)
   ├─ Trump card selected
   └─ FSM enters Play state

2. Phase 1 Loop (Until deck has PlayerCount cards left)
   ├─ PlayCard() for each player
   ├─ Trick ends → ResolveTrickInternal()
   ├─ Winner can: DeclareMeld() or SkipMeld()
   ├─ DrawCards() called automatically
   └─ Repeat

3. Phase 2 Transition (Deck == PlayerCount cards)
   ├─ Winner draws hidden card
   ├─ Others draw trump card
   ├─ All TableCards return to Hand
   └─ Phase changes to Phase2_Last9

4. Phase 2 Loop (Until hands empty)
   ├─ PlayCard() for each player (strict follow-suit rules)
   ├─ Trick ends → ResolveTrickInternal()
   ├─ NO DRAWING
   └─ Repeat until hands empty

5. Round Ends
   ├─ Calculate scores (Standard mode) or + Ace/Ten bonus (Advanced mode)
   ├─ Check if TargetScore reached
   ├─ If yes → GameOver
   └─ If no → Start new round (redeal)
```

---

## Unity Integration Example

```csharp
using BeziqueCore;
using UnityEngine;

public class BeziqueGameManager : MonoBehaviour
{
    private BeziqueGameController game;

    [Header("Game Configuration")]
    [SerializeField] private int playerCount = 2;
    [SerializeField] private int targetScore = 1500;
    [SerializeField] private GameMode gameMode = GameMode.Standard;

    [Header("UI References")]
    [SerializeField] private CardUI cardUI;
    [SerializeField] private ScoreUI scoreUI;
    [SerializeField] private PhaseUI phaseUI;

    void Start()
    {
        game = new BeziqueGameController();

        // Subscribe to events
        game.TrickEnded += OnTrickEnded;
        game.PhaseChanged += OnPhaseChanged;
        game.MeldDeclared += OnMeldDeclared;
        game.RoundEnded += OnRoundEnded;
        game.GameEnded += OnGameEnded;

        // Initialize game
        var config = new GameConfig {
            PlayerCount = (byte)playerCount,
            TargetScore = (ushort)targetScore,
            Mode = gameMode,
            DeckCount = 4
        };
        game.Initialize(config);

        // Update initial UI
        UpdateAllUI();
    }

    void Update()
    {
        if (game.IsGameOver)
        {
            ShowGameOverScreen();
            return;
        }

        // Poll for state updates
        UpdateAllUI();
    }

    // Called from UI when player clicks a card
    public void OnCardClicked(int cardIndex)
    {
        if (!IsPlayerTurn()) return;

        var player = game.Players[game.CurrentPlayer];
        if (cardIndex < 0 || cardIndex >= player.Hand.Count) return;

        var card = player.Hand[cardIndex];
        if (game.PlayCard(card))
        {
            // Card played successfully
            UpdateAllUI();
        }
    }

    // Called from UI when player wants to meld
    public void OnMeldClicked()
    {
        if (!game.CanMeld()) return;

        var bestMeld = game.GetBestMeld();
        if (bestMeld != null)
        {
            game.DeclareMeld(bestMeld.Cards.ToArray(), bestMeld.Type);
        }
        else
        {
            game.SkipMeld();
        }
    }

    // Check if it's the local player's turn
    public bool IsPlayerTurn()
    {
        return game.CurrentPlayer == 0; // Assuming player 0 is human
    }

    public bool IsPlayerTurn(int playerId)
    {
        return game.IsPlayerTurn(playerId);
    }

    private void UpdateAllUI()
    {
        cardUI.UpdateCards(game.Players, game.PlayedCards);
        scoreUI.UpdateScores(game.Players);
        phaseUI.UpdatePhase(game.Context.CurrentPhase);
        phaseUI.UpdateTrump(game.Context.TrumpCard);

        // Highlight legal moves
        var legalMoves = game.GetLegalMoves();
        cardUI.HighlightLegalMoves(legalMoves);

        // Show meld button if available
        var bestMeld = game.GetBestMeld();
        cardUI.ShowMeldButton(bestMeld != null);
    }

    private void OnTrickEnded(object sender, TrickEndedEventArgs e)
    {
        Debug.Log($"Player {e.WinnerId + 1} wins trick!");

        if (!e.IsFinalTrick && game.Context.DrawDeck.Count > 0)
        {
            // Cards will be drawn automatically via FSM
            Debug.Log("Drawing cards...");
        }
    }

    private void OnPhaseChanged(object sender, PhaseChangedEventArgs e)
    {
        Debug.Log($"Phase changed to: {e.NewPhase}");

        if (e.NewPhase == GamePhase.Phase2_Last9)
        {
            // Phase 2 started - no more melding
            phaseUI.HideMeldButton();
        }
    }

    private void OnMeldDeclared(object sender, MeldDeclaredEventArgs e)
    {
        Debug.Log($"Player {e.PlayerId + 1} melded {e.MeldType} (+{e.Points} pts)");
    }

    private void OnRoundEnded(object sender, RoundEndedEventArgs e)
    {
        Debug.Log($"Round ended! Winner: Player {e.WinnerId + 1}");
        scoreUI.ShowRoundSummary(e.Scores);
    }

    private void OnGameEnded(object sender, GameEndedEventArgs e)
    {
        Debug.Log($"GAME OVER! Winner: Player {e.WinnerId + 1}");
        ShowGameOverScreen();
    }
}
```

---

## Quick Reference Card Play Flow

```
┌─────────────────────────────────────────────────────────────┐
│  PHASE 1 (Normal Play)                                      │
│  ┌─────────────┐    ┌─────────┐    ┌──────────────────┐   │
│  │ Play Cards  │ -> │ Trick   │ -> │   Meld Phase     │   │
│  │ (all legal) │    │ Ends    │    │ (Winner only)    │   │
│  └─────────────┘    └─────────┘    └──────────────────┘   │
│                                                  ↓             │
│                                          Draw Cards              │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│  PHASE 2 (Last 9 Cards)                                   │
│  ┌─────────────┐    ┌─────────┐                              │
│  │ Play Cards  │ -> │ Trick   │ -> L9NewTrick -> Loop        │
│  │ (follow suit)│    │ Ends    │                              │
│  └─────────────┘    └─────────┘                              │
│  (No drawing, no melding)                                      │
└─────────────────────────────────────────────────────────────┘
```

---

## Important Rules Summary

1. **9-Card Hand:** Players maintain 9 cards throughout Phase 1
2. **Phase 2:** No drawing, no melding, cards reduce each trick
3. **Melded Cards:** Can be played from table cards
4. **Trump Seven:** Winner can swap with face-up trump card (+10 pts)
5. **Winning Score:** First to reach 1500 points wins
6. **Deck Size:** Always 4 decks (132 cards total)
