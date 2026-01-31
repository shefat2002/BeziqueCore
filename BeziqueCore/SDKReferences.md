# BeziqueCore SDK - Complete API Reference

## Quick Start (Simple API) - Recommended for Unity

### **BeziqueGameManager** (Factory)
**File:** `Simple/BeziqueGameManager.cs`

The simplest way to create a game - reduces setup from 24 lines to 3 lines.

| Method | Description |
|--------|-------------|
| `BeziqueGame CreateSinglePlayer(string humanName, string aiName)` | Human vs AI game |
| `BeziqueGame CreateMultiplayer(params string[] playerNames)` | Multiplayer local game |
| `BeziqueGame CreateGame(params PlayerConfig[] configs)` | Custom player configuration |
| `BeziqueGame CreateCustom(Action<BeziqueGameBuilder> configure)` | Builder pattern for advanced setup |

```csharp
// Single player vs AI - 3 lines
var game = BeziqueGameManager.CreateSinglePlayer("Alice", "Bot Bob");
game.CardPlayed += (s, e) => Debug.Log($"{e.PlayerName} played {e.Card}");
game.Start();

// Local multiplayer
var game = BeziqueGameManager.CreateMultiplayer("Alice", "Bob", "Charlie");

// Custom configuration with builder
var game = BeziqueGameManager.CreateCustom(b => b
    .WithPlayer("Alice", isBot: false, isDealer: true)
    .WithPlayer("Bob", isBot: true)
    .WithGameMode(GameMode.Advanced)
    .WithWinningScore(1500));
```

### **BeziqueGame** (Simplified Game Wrapper)
**File:** `Simple/BeziqueGame.cs`

Simplified game interface with direct actions and unified events.

| Properties | Description |
|------------|-------------|
| `EventSubscriptionManager Events` | Advanced event subscription manager |
| `bool IsGameOver` | Is game finished |
| `Player CurrentPlayer` | Whose turn it is |
| `string CurrentStateName` | Current state name |
| `IReadOnlyList<Player> Players` | All players |

| Methods | Description |
|---------|-------------|
| `void Start()` | Start the game |
| `void PlayCard(int playerIndex, int cardIndex)` | Play a card |
| `void DeclareMeld(int playerIndex, int[] cardIndices)` | Declare meld |
| `void SkipMeld(int playerIndex)` | Skip meld phase |
| `void SwitchSevenOfTrump(int playerIndex)` | Switch 7 of trump |
| `void BeginEventBatch()` | Begin batching events |
| `void EndEventBatch()` | Flush and publish batched events |
| `void FlushEventBatch()` | Flush batch and start new batch |
| `GameSnapshotDto GetSnapshot()` | Get full game state |
| `GameSnapshotDto GetSnapshotForPlayer(string playerId)` | Get player-specific view |
| `bool CanPlayCard(int playerIndex, int cardIndex)` | Check if card can be played |
| `bool CanDeclareMeld(int playerIndex, int[] cardIndices)` | Check if meld is valid |
| `bool CanSwitchSevenOfTrump(int playerIndex)` | Check if can switch trump |
| `string[] GetLegalMoves(int playerIndex)` | Get available actions |

### **EventSubscriptionManager** (Advanced Events)
**File:** `Simple/EventSubscriptionManager.cs`

Advanced event subscription with filtering, batching, and type-safe extensions.

| Methods | Description |
|---------|-------------|
| `Subscribe<T>(handler, filter?)` | Subscribe with optional predicate filter |
| `Unsubscribe<T>(handler)` | Unsubscribe specific handler |
| `UnsubscribeAll<T>()` | Unsubscribe all handlers for type |
| `Publish<T>(sender, args)` | Publish event to subscribers |
| `BeginBatch()` | Start batching events |
| `EndBatch(sender)` | Publish all batched events |
| `FlushBatch(sender)` | Flush and start new batch |
| `GetSubscriberCount<T>()` | Get subscriber count for type |
| `ClearAll()` | Clear all subscriptions |

**Extension Methods:**

```csharp
// Simplified subscription with action only
game.Events.SubscribeToCardPlayed(e => Debug.Log($"{e.PlayerName} played {e.Card}"));
game.Events.SubscribeToMeldDeclared(e => Debug.Log($"{e.MeldType} for {e.Points} points"));
game.Events.SubscribeToTrickResolved(e => Debug.Log($"{e.WinnerName} won (+{e.Points})"));
game.Events.SubscribeToPlayerTurnChanged(e => Debug.Log($"Turn: {e.PlayerName}"));
game.Events.SubscribeToRoundEnded(e => UpdateScoreboard(e.RoundScores));
game.Events.SubscribeToGameEnded(e => ShowGameOver(e.WinnerName, e.FinalScores));
game.Events.SubscribeToError(e => Debug.LogError(e.Message));

// Filtered subscription - only process specific player
game.Events.SubscribeToCardPlayed(
    e => Debug.Log($"Alice played {e.Card}"),
    e => e.PlayerName == "Alice"
);

// Filter by multiple conditions
game.Events.SubscribeToMeldDeclared(
    e => Debug.Log($"Big meld! {e.Points} points"),
    e => e.Points >= 100
);

// Event batching - batch multiple events and process together
game.BeginEventBatch();
game.PlayCard(0, 3);
game.PlayCard(1, 5);
game.DeclareMeld(0, new[] { 0, 1, 2 });
game.EndEventBatch();  // All events fired together
```

### **Simple Events** (Unified Event Args)
**File:** `Simple/Events/GameEvents.cs`

All events use standard .NET `EventHandler<T>` pattern.

| Event | Args | Description |
|-------|------|-------------|
| `CardPlayed` | `CardPlayedEventArgs` | Card was played |
| `MeldDeclared` | `MeldDeclaredEventArgs` | Meld was declared |
| `MeldSkipped` | `MeldSkippedEventArgs` | Meld was skipped |
| `TrickResolved` | `TrickResolvedEventArgs` | Trick was resolved |
| `PlayerTurnChanged` | `PlayerTurnChangedEventArgs` | Turn changed |
| `RoundEnded` | `RoundEndedEventArgs` | Round ended |
| `GameEnded` | `GameEndedEventArgs` | Game ended |
| `Error` | `GameErrorEventArgs` | Error occurred |

```csharp
// Subscribe to events (standard .NET events)
game.CardPlayed += (sender, e) => {
    Debug.Log($"{e.PlayerName} played {e.Card}");
};

game.MeldDeclared += (sender, e) => {
    Debug.Log($"{e.PlayerName} declared {e.MeldType} for {e.Points} points");
};

game.TrickResolved += (sender, e) => {
    Debug.Log($"{e.WinnerName} won the trick (+{e.Points} points)");
};

game.GameEnded += (sender, e) => {
    Debug.Log($"{e.WinnerName} wins the game!");
};

// OR use advanced EventSubscriptionManager with filtering
game.Events.SubscribeToCardPlayed(
    e => Debug.Log($"{e.PlayerName} played {e.Card}"),
    e => e.PlayerName == "Alice"  // Only Alice's plays
);
```

### **GameActionBuilder** (Fluent Actions API)
**File:** `Simple/GameActions.cs`

Fluent builder pattern for game actions with result objects.

| Methods | Description |
|---------|-------------|
| `ForPlayer(int index)` / `ForPlayer(string name)` | Set target player |
| `PlayCardAsync(int cardIndex)` | Play a card asynchronously |
| `DeclareMeldAsync(params int[] cardIndices)` | Declare meld asynchronously |
| `SkipMeldAsync()` | Skip meld phase asynchronously |
| `SwitchSevenOfTrumpAsync()` | Switch trump asynchronously |
| `CanPlayCard(int cardIndex)` | Check if card can be played |
| `CanDeclareMeld(params int[] cardIndices)` | Check if meld is valid |
| `CanSwitchSevenOfTrump()` | Check if can switch trump |
| `GetLegalMoves()` | Get available actions |

```csharp
// Fluent builder pattern
var result = await game.Actions()
    .ForPlayer(0)
    .PlayCardAsync(3);

if (result.Success)
    Debug.Log("Card played successfully");
else
    Debug.LogError($"Failed: {result.Message}");

// Query with builder
var canPlay = game.Actions()
    .ForPlayer("Alice")
    .CanPlayCard(5);

canPlay.Match(
    onSuccess: () => Debug.Log("Can play this card!"),
    onFailure: (error) => Debug.LogError(error)
);

// Extension methods for quick actions
await game.PlayCardAsync(0, 3);
await game.DeclareMeldAsync(0, 0, 1, 2);
await game.SkipMeldAsync(0);
```

### **ActionResult** (Result Objects)
**File:** `Simple/ActionResult.cs`

Type-safe result objects with pattern matching support.

| Properties | Description |
|------------|-------------|
| `bool Success` | Whether action succeeded |
| `string? Message` | Error or success message |
| `object? Data` | Optional result data |

| Methods | Description |
|---------|-------------|
| `Match(Action onSuccess, Action<string> onFailure)` | Pattern matching for void results |
| `Match<TResult>(Func<TResult> onSuccess, Func<string, TResult> onFailure)` | Pattern matching with return value |

```csharp
ActionResult<string[]> GetMoves()
{
    return game.Actions()
        .ForPlayer(0)
        .GetLegalMoves();
}

// Pattern matching
var moves = GetMoves();
moves.Match(
    onSuccess: (availableMoves) => {
        foreach (var move in availableMoves)
            Debug.Log(move);
    },
    onFailure: (error) => Debug.LogError(error)
);
```

---

## Advanced Entry Point

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

## Quick Start Examples

### Simple API (Recommended)

```csharp
// 3 lines to start
var game = BeziqueGameManager.CreateSinglePlayer("Alice", "Bot Bob");
game.CardPlayed += (s, e) => Debug.Log($"{e.PlayerName} played {e.Card}");
game.Start();

// Player actions
game.PlayCard(0, 3);              // Play 4th card
game.DeclareMeld(0, [0, 1, 2]);   // Declare meld with first 3 cards
game.SkipMeld(0);                 // Skip meld phase
game.SwitchSevenOfTrump(0);       // Switch trump card

// Query state
var snapshot = game.GetSnapshotForPlayer(playerId);
var legalMoves = game.GetLegalMoves(0);
bool canPlay = game.CanPlayCard(0, 3);
```

### Advanced API (State Machine)

```csharp
// Setup (24 lines)
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
