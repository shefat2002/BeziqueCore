# BeziqueCore SDK - Complete API Reference

## Primary Entry Point

### **BeziqueGameFlow** (State Machine)
**File:** `BeziqueCore/BeziqueGameFlow.cs` + `BeziqueGameFlow.User.cs`

```csharp
BeziqueGameFlow(IGameAdapter adapter)

void Start()
void DispatchEvent(EventId eventId)
```

**Dispatch Methods (19 total):**

| Method | Purpose |
|--------|---------|
| `DispatchGameInitialized()` | Start game |
| `DispatchCardsDealt()` | After dealing |
| `DispatchTrumpDetermined()` | After trump selection |
| `DispatchCardPlayed()` | Player plays card |
| `DispatchTrickComplete()` | All players played |
| `DispatchTrickResolved()` | After trick resolution |
| `DispatchMeldDeclared()` | Player declares meld |
| `DispatchMeldSkipped()` | Player skips meld |
| `DispatchMeldScored()` | After meld scoring |
| `DispatchCardsDrawn()` | After drawing cards |
| `DispatchDeckEmpty()` | Deck is empty |
| `DispatchAllHandsEmpty()` | Round ends |
| `DispatchContinueGame()` | Start new round |
| `DispatchWinningScoreReached()` | Someone won |
| `DispatchTimerExpired()` | Timer ran out |
| `DispatchMorePlayersNeedToPlay()` | More players to act |
| `DispatchLastNineCardsStarted()` | L9 phase begins |

**Helper Check Methods:**

| Method | Purpose |
|--------|---------|
| `CheckAndDispatchDeckEmpty()` | Check deck state |
| `CheckAndDispatchTrickComplete()` | Check if trick complete |
| `CheckAndDispatchL9TrickComplete()` | Check L9 trick |
| `CheckAndDispatchWinningScoreAfterTrick()` | Check win after trick |
| `CheckAndDispatchWinningScoreAfterMeld()` | Check win after meld |
| `CheckAndDispatchL9WinningScore()` | Check win in L9 |
| `CheckAndDispatchWinningScoreAfterRound()` | Check win after round |

---

## Multiplayer Support

### **IMultiplayerGameAdapter** (Network-Aware Adapter)
**File:** `Interfaces/IMultiplayerGameAdapter.cs`

Extends `IGameAdapter` with multiplayer-specific functionality for online games.

| Category | Methods |
|----------|---------|
| **Snapshots** | `GameSnapshotDto GetSnapshot()`, `GetSnapshotForPlayer(string userId)` |
| **Commands** | `Task<GameActionResult> ExecuteRemoteCommandAsync(PlayerCommand command)` |
| **Validation** | `string[] GetLegalMoves(string userId)`, `bool CanPlayerAct(string userId)`, `bool CanPlayCard(string userId, int cardIndex)`, `bool CanDeclareMeld(string userId, int[] cardIndices)`, `bool CanSwitchSevenOfTrump(string userId)` |
| **Players** | `string? GetCurrentPlayerUserId()`, `Player? GetPlayerById(string userId)`, `void InitializeGameWithPlayers(List<Player> players)` |
| **State** | `string GetCurrentStateName()`, `void SubscribeToEvents(IMultiplayerEventHandler eventHandler)` |

### **MultiplayerGameAdapter**
**File:** `Multiplayer/MultiplayerGameAdapter.cs`

Wraps `IGameAdapter` with network functionality:
- Thread-safe operations with locks
- Event firing to `IMultiplayerEventHandler` subscribers
- Player command execution (PlayCard, DeclareMeld, SkipMeld, SwitchSevenOfTrump)
- Game snapshot generation for network transmission

### **IMultiplayerEventHandler** (Event Interface)
**File:** `Interfaces/IMultiplayerEventHandler.cs`

Subscribe to multiplayer game events for real-time updates.

| Events | Description |
|--------|-------------|
| `OnCardPlayed(CardPlayedEvent)` | Card was played |
| `OnMeldDeclared(MeldDeclaredEvent)` | Meld was declared |
| `OnMeldSkipped(MeldSkippedEvent)` | Meld was skipped |
| `OnTrickResolved(TrickResolvedEvent)` | Trick was resolved |
| `OnSevenOfTrumpSwitched(SevenOfTrumpSwitchedEvent)` | 7 of trump switched |
| `OnCardsDrawn(CardsDrawnEvent)` | Cards were drawn |
| `OnRoundEnded(RoundEndedEvent)` | Round ended |
| `OnGameEnded(GameEndedEvent)` | Game ended |
| `OnLastNineCardsStarted(LastNineCardsStartedEvent)` | L9 phase started |
| `OnTrumpDetermined(TrumpDeterminedEvent)` | Trump was determined |
| `OnPlayerTurnChanged(PlayerTurnChangedEvent)` | Turn changed |
| `OnError(GameErrorEvent)` | Error occurred |

### **Multiplayer Models**
**Files:** `Models/PlayerCommand.cs`, `Models/GameActionResult.cs`, `Models/GameSnapshotDto.cs`

**PlayerCommand:**
```csharp
string UserId { get; init; }
string Action { get; init; }
object Payload { get; init; }
string? CommandId { get; init; }
DateTime Timestamp { get; init; }
```

**GameActionResult:**
```csharp
bool Success { get; init; }
string? ErrorMessage { get; init; }
object? Data { get; init; }
string? GameState { get; init; }
string? NextPlayerUserId { get; init; }

static GameActionResult Ok(object? data = null)
static GameActionResult Error(string errorMessage)
```

**GameSnapshotDto:**
```csharp
string StateName { get; init; }
PlayerStateDto[] Players { get; init; }
string TrumpSuit { get; init; }
CardDto? TrumpCard { get; init; }
TrickStateDto CurrentTrick { get; init; }
string CurrentPlayerUserId { get; init; }
string DealerUserId { get; init; }
int DeckCardCount { get; init; }
bool IsLastNineCardsPhase { get; init; }
string? LeadSuit { get; init; }
Dictionary<string, int> RoundScores { get; init; }
string GameMode { get; init; }
string? WinnerUserId { get; init; }
```

### **Multiplayer Events**
**File:** `Models/Events/MultiplayerEvents.cs`

Event types for real-time game updates:
- `GameEvent` (base)
- `CardPlayedEvent`
- `MeldDeclaredEvent`
- `MeldSkippedEvent`
- `TrickResolvedEvent`
- `SevenOfTrumpSwitchedEvent`
- `CardsDrawnEvent`
- `RoundEndedEvent`
- `GameEndedEvent`
- `LastNineCardsStartedEvent`
- `TrumpDeterminedEvent`
- `PlayerTurnChangedEvent`
- `GameErrorEvent`

### **BeziqueActions** (Constants)
**File:** `Multiplayer/MultiplayerGameAdapter.cs`

```csharp
public const string PlayCard = "PlayCard"
public const string DeclareMeld = "DeclareMeld"
public const string SkipMeld = "SkipMeld"
public const string SwitchSevenOfTrump = "SwitchSevenOfTrump"
```

---

## Core Interfaces

### **IGameAdapter** (Game Orchestration)
**File:** `Interfaces/IGameAdapter.cs`

| Category | Methods |
|----------|---------|
| **Initialization** | `InitializeGame()`, `NotifyGameInitialized()`, `DealCards()`, `NotifyCardsDealt()`, `FlipTrumpCard()`, `NotifyTrumpDetermined()` |
| **Timers** | `StartPlayerTimer()`, `StopPlayerTimer()`, `ResetPlayerTimer()`, `DeductTimeoutPoints()` |
| **Gameplay** | `ProcessOpponentResponses()`, `ResolveTrick()`, `ProcessMeldOpportunity()`, `ScoreMeld()`, `DrawCards()`, `CheckDeck()` |
| **Last 9** | `ProcessL9OpponentResponses()`, `ResolveL9Trick()`, `CheckL9TrickComplete()`, `CalculateL9FinalScores()` |
| **Round End** | `CalculateRoundScores()`, `CalculateAcesAndTens()`, `NotifyRoundEnded()`, `DeclareWinner()`, `NotifyGameOver()` |
| **Queries** | `IsLastNineCardsPhase()`, `IsDeckEmpty()`, `AreAllHandsEmpty()`, `HasPlayerReachedWinningScore()`, `IsTrickComplete()`, `MorePlayersNeedToPlay()` |

### **IGameState** (State Access)
**File:** `Interfaces/IGameState.cs`

| Properties | Description |
|------------|-------------|
| `List<Player> Players` | All players |
| `Player CurrentPlayer` | Active player |
| `Player Winner` | Game winner |
| `Suit TrumpSuit` | Trump suit |
| `Card TrumpCard` | Trump card |
| `Dictionary<Player, Card> CurrentTrick` | Current trick |
| `GameMode Mode` | Standard/Advanced |
| `Dictionary<Player, int> RoundScores` | Scores |
| `string CurrentStateName` | State machine state |

| Methods | Description |
|--------|-------------|
| `Reset()`, `AddPlayer()`, `StartNewTrick()`, `AddCardToTrick()`, `IsTrickComplete()` | State operations |

### **IPlayerActions** (Player Actions)
**File:** `Interfaces/IPlayerActions.cs`

| Methods | Description |
|--------|-------------|
| `PlayCard(Player, Card)` | Play a card |
| `DeclareMeld(Player, Meld)` | Declare meld |
| `DrawCard(Player)` | Draw card |
| `SwitchSevenOfTrump(Player)` | Switch 7 of trump |
| `SkipMeld(Player)` | Skip meld |

### **IDeckOperations** (Deck Management)
**File:** `Interfaces/IDeckOperations.cs`

| Methods | Description |
|--------|-------------|
| `InitializeDeck()` | Create/shuffle deck |
| `Card? DrawTopCard()` | Draw top card |
| `int GetRemainingCardCount()` | Cards remaining |
| `bool IsLastNineCards()` | L9 phase check |
| `Card? FlipTrumpCard()` | Flip trump |
| `void ShuffleDeck()` | Shuffle deck |
| `Card? GetTrumpCard()` | Get trump card |
| `Card? TakeTrumpCard()` | Take trump card |

### **ITrickResolver** (Trick Resolution)
**File:** `Interfaces/ITrickResolver.cs`

| Methods | Description |
|--------|-------------|
| `Player DetermineTrickWinner(...)` | Find trick winner |
| `bool IsValidPlay(...)` | Validate card play |
| `int CalculateTrickPoints(List<Card>)` | Calculate points |
| `bool HasJoker(...)` | Check for joker |
| `Suit GetLeadSuit(...)` | Get lead suit |

### **IMeldValidator** (Meld Validation)
**File:** `Interfaces/IMeldValidator.cs`

| Methods | Description |
|--------|-------------|
| `bool IsValidMeld(Card[], Suit)` | Validate meld |
| `int CalculateMeldPoints(Meld)` | Get meld points |
| `MeldType DetermineMeldType(Card[], Suit)` | Identify meld type |

### **IBeziqueBot** (AI Interface)
**File:** `AI/IBeziqueBot.cs`

| Methods | Description |
|--------|-------------|
| `Card SelectCardToPlay(...)` | AI selects card |
| `Meld? DecideMeld(Player, Suit)` | AI decides meld |
| `string BotName` | Bot name |

---

## SDK Classes

### **GameAdapter**
**File:** `Adapters/GameAdapter.cs`
Implements `IGameAdapter` - main game orchestration

### **DeckOperations**
**File:** `Deck/DeckOperations.cs`
Implements `IDeckOperations` - array-based optimized deck

### **PlayerActions**
**File:** `Actions/PlayerActions.cs`
Implements `IPlayerActions` - executes player actions

### **BeziqueBot**
**File:** `AI/BeziqueBot.cs`
Implements `IBeziqueBot` - maximum difficulty AI

### **MeldHelper** (Static Utility)
**File:** `Helpers/MeldHelper.cs`

| Static Methods | Description |
|----------------|-------------|
| `List<Meld> FindAllPossibleMelds(Player, Suit)` | Find all melds |
| `bool HasAnyMeld(Player, Suit)` | Check if melds exist |
| `Meld? GetBestMeld(Player, Suit)` | Get best meld |

### **MeldValidator**
**File:** `Validators/MeldValidator.cs`
Implements `IMeldValidator`

### **TrickResolver**
**File:** `Resolvers/TrickResolver.cs`
Implements `ITrickResolver`

### **GameState**
**File:** `Models/GameState.cs`
Implements `IGameState`

### **GameStateNotifier**
**File:** `Notifiers/GameStateNotifier.cs`
Implements `IGameStateNotifier` with C# events for UI subscriptions

### **PlayerTimer**
**File:** `Timers/PlayerTimer.cs`
Implements `IPlayerTimer` - 15 second turn timer

---

## Models

### **Card** (Immutable)
**File:** `Interfaces/IPlayerActions.cs`

```csharp
Card Create(Suit suit, Rank rank)
Card CreateJoker(Suit suit = Suit.Spades, Rank rank = Rank.Ace)

Suit Suit { get; }
Rank Rank { get; }
bool IsJoker { get; }
```

### **Player**
```csharp
string Id { get; set; }
string Name { get; set; }
int Score { get; set; }
List<Card> Hand { get; set; }
bool IsDealer { get; set; }
bool IsBot { get; set; }
List<Meld> DeclaredMelds { get; set; }
List<Card> MeldedCards { get; set; }
```

### **Meld**
```csharp
MeldType Type { get; set; }
List<Card> Cards { get; set; }
int Points { get; set; }
```

### **MeldType** (enum)
`TrumpRun`, `TrumpSeven`, `TrumpMarriage`, `Marriage`, `FourAces`, `FourKings`, `FourQueens`, `FourJacks`, `Bezique`, `DoubleBezique`, `InvalidMeld`

### **Suit** (enum)
`Clubs`, `Diamonds`, `Spades`, `Hearts`

### **Rank** (enum)
`Seven = 7`, `Eight = 8`, `Nine = 9`, `Jack = 10`, `Queen = 11`, `King = 12`, `Ten = 13`, `Ace = 14`

### **GameMode** (enum)
`Standard`, `Advanced`

---

## Constants

### **GameConstants** (Static Class)
**File:** `Constants/GameConstants.cs`

| Category | Constants |
|----------|------------|
| **Dealing** | `CardsPerPlayer = 9`, `DealSetSize = 3` |
| **Meld Points** | `BeziquePoints = 40`, `DoubleBeziquePoints = 500`, `TrumpRunPoints = 250`, `TrumpMarriagePoints = 40`, `MarriagePoints = 20`, `SevenOfTrumpBonus = 10`, `FourAcesPoints = 100`, `FourKingsPoints = 80`, `FourQueensPoints = 60`, `FourJacksPoints = 40` |
| **Bonuses** | `LastTrickBonusPoints = 10`, `TimeoutPenalty = 10`, `AceAndTenBonusPoints = 10` |
| **Deck** | `TotalCardsInDeck = 64`, `LastNineCardsThreshold = 9` |
| **Game** | `WinningScore = 1000` |

### **MeldPoints** (O(1) Lookup)
```csharp
int GetPointsForType(MeldType type)
```

### **RankValues** (O(1) Lookup)
```csharp
int GetValue(Rank rank)        // Returns 7-14
int GetTrickPoints(Rank rank)  // Ace=11, Ten=10, others=0
```

---

## Quick Start Example

```csharp
// Setup
var deckOps = new DeckOperations();
var playerTimer = new PlayerTimer();
var gameState = new GameState(playerTimer);
var notifier = new GameStateNotifier();
var trickResolver = new TrickResolver(gameState);
var meldValidator = new MeldValidator();
var playerActions = new PlayerActions(deckOps, meldValidator, notifier, gameState);
var gameAdapter = new GameAdapter(deckOps, playerActions, notifier, gameState, trickResolver);
var gameFlow = new BeziqueGameFlow(gameAdapter);

// Add players
gameState.AddPlayer(new Player { Id = "1", Name = "Human", IsBot = false });
gameState.AddPlayer(new Player { Id = "2", Name = "AI", IsBot = true });

// Subscribe to events
notifier.OnCardPlayed += (player, card) => Console.WriteLine($"{player.Name} played {card}");
notifier.OnTrickWon += (winner, cards, points) => Console.WriteLine($"{winner.Name} won trick (+{points})");

// Start game
gameFlow.Start();
gameFlow.DispatchGameInitialized();
gameFlow.DispatchCardsDealt();
gameFlow.DispatchTrumpDetermined();
```

---

## Multiplayer Integration Example

```csharp
// Create multiplayer adapter
var multiplayerAdapter = new MultiplayerGameAdapter(
    gameAdapter, deckOps, playerActions, notifier, gameState, trickResolver, meldValidator);

// Subscribe to multiplayer events
multiplayerAdapter.SubscribeToEvents(new MultiplayerEventHandler());

// Initialize with players
multiplayerAdapter.InitializeGameWithPlayers(new List<Player>
{
    new Player { Id = "user1", Name = "Player 1", IsBot = false },
    new Player { Id = "user2", Name = "Player 2", IsBot = false }
});

// Get snapshot for client
var snapshot = multiplayerAdapter.GetSnapshotForPlayer("user1");

// Execute remote command
var result = await multiplayerAdapter.ExecuteRemoteCommandAsync(new PlayerCommand
{
    UserId = "user1",
    Action = BeziqueActions.PlayCard,
    Payload = new { CardIndex = 0 }
});
```
