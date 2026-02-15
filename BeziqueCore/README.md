# BeziqueCore Unity Integration Guide

## Public API Reference

### Core Classes

| Class | Namespace | Purpose |
|-------|------------|----------|
| `BeziqueGame` | `BeziqueCore` | Main game class - implements `IBeziqueGame` |
| `CardHelper` | `BeziqueCore` | Static utility for card ID operations |
| `GameCard` | `BeziqueCore` | Implements `IGameCard` interface |
| `PlayerHand` | `BeziqueCore` | Implements `IPlayerHand` interface |

### Public Interfaces

| Interface | Namespace | Purpose |
|-----------|------------|---------------|
| `IBeziqueGame` | `BeziqueCore.Interfaces` | Main game interface |
| `IGameState` | `BeziqueCore.Interfaces` | Query game state |
| `IGameEvent` | `BeziqueCore.Interfaces` | Subscribe to game events |
| `IPlayerHand` | `BeziqueCore.Interfaces` | Player data access |
| `IGameCard` | `BeziqueCore.Interfaces` | Card data access |

### Enums

```csharp
// Game phases
public enum GamePhase
{
    Dealing,        // Cards being dealt
    TrumpFlip,      // Trump card being revealed
    Phase1_Playing, // Main gameplay (deck available)
    Phase2_Playing, // Last 9 cards (no deck)
    RoundEnd,       // Round completed
    GameEnd         // Game over
}

// Meld types with point values
public enum MeldType
{
    Bezique,         // Q Spades + J Diamonds = 40 points
    DoubleBezique,   // 2x Q Spades + 2x J Diamonds = 500 points
    FourJacks,       // 40 points
    FourQueens,      // 60 points
    FourKings,        // 80 points
    FourAces,        // 100 points
    CommonMarriage,   // K + Q same suit = 20 points
    TrumpMarriage,    // K + Q trump suit = 40 points
    TrumpRun,        // A, 10, K, Q, J trump suit = 150 points
}
```

---

## IBeziqueGame - Main Game Interface

**Namespace:** `BeziqueCore.Interfaces`

```csharp
public interface IBeziqueGame : IGameState, IGameEvent
{
    // Game lifecycle
    void Initialize(int playerCount);     // Initialize with 2 or 4 players
    void StartDealing();                  // Begin dealing phase
    bool DealNextSet();                   // Deal next 3 cards, returns false if complete
    void CompleteDealing();               // Finalize dealing and flip trump
    void StartNewRound();                 // Start a new round (after RoundEnd)

    // Gameplay actions
    void PlayCard(int playerIndex, byte cardId);   // Play a card
    void DrawCard(int playerIndex);                  // Draw from deck (Phase 1 only)
    void CreateMeld(int playerIndex, MeldType meldType, byte[] cardIds); // Claim meld

    // Queries
    bool IsPlayerTurn(int playerIndex);              // Check if it's player's turn
    IReadOnlyList<byte> GetPlayableCards(int playerIndex); // Get cards in hand
}
```

**Important Notes:**
- `Initialize()` must be called before any other methods
- `StartDealing()` begins the deal process
- `DealNextSet()` must be called repeatedly until it returns `false`
- `CompleteDealing()` must be called after dealing is done to flip trump and start play
- `GetPlayableCards()` returns **all cards in hand**, not just legally playable cards for the current trick

---

## IGameState - Query Game State

```csharp
public interface IGameState
{
    GamePhase CurrentPhase { get; }          // Current game phase
    IReadOnlyList<IPlayerHand> Players { get; } // All players
    IGameCard? TrumpCard { get; }           // The trump card (flipped during deal)
    byte? TrumpSuit { get; }                 // Trump suit (0-3)
    int DeckCount { get; }                    // Cards remaining in deck
    int CurrentPlayerIndex { get; }           // Whose turn it is
    int DealerIndex { get; }                  // Who dealt this round
}
```

---

## IGameEvent - Subscribe to Game Events

**WARNING:** Not all events are currently implemented/firing. See [Event Implementation Status](#event-implementation-status).

```csharp
public interface IGameEvent
{
    event Action<int, IReadOnlyList<IGameCard>> CardsDealt;    // (playerIndex, cards)
    event Action<IGameCard> TrumpCardFlipped;                  // (trumpCard)
    event Action<int> DealerBonusPoints;                         // (dealerIndex) - 10 points
    event Action<int, IGameCard> CardDrawn;                     // (playerIndex, card) - NOTE: Typo in name
    event Action<GamePhase> PhaseChanged;                        // (newPhase)
    event Action<int> TrickComplete;                             // (winnerIndex) - NOT FIRED
    event Action<int, int> MeldPointsAwarded;                    // (playerIndex, points) - NOT FIRED
    event Action<int> RoundEnd;                                  // (winnerIndex) - NOT FIRED
    event Action<int> GameEnd;                                   // (winnerIndex) - NOT FIRED
}
```

### Event Implementation Status

| Event | Status | Notes |
|-------|--------|-------|
| `CardsDealt` | WORKS | Fires during `DealNextSet()` |
| `TrumpCardFlipped` | WORKS | Fires during `CompleteDealing()` |
| `DealerBonusPoints` | WORKS | Fires if trump is 7 |
| `CardDrawn` | NOT WORKING | Event defined but never invoked in code |
| `PhaseChanged` | WORKS | Fires on phase transitions |
| `TrickComplete` | NOT WORKING | Event defined but never invoked |
| `MeldPointsAwarded` | NOT WORKING | Event defined but never invoked |
| `RoundEnd` | NOT WORKING | Event defined but never invoked |
| `GameEnd` | NOT WORKING | Event defined but never invoked |

---

## IPlayerHand - Player Data

```csharp
public interface IPlayerHand
{
    int PlayerIndex { get; }                  // Player index (0-based)
    IReadOnlyList<IGameCard> Cards { get; }   // Cards in hand
    int Score { get; }                        // Current score
    bool IsCurrentPlayer { get; }              // Is this the current player?
}
```

---

## IGameCard - Card Data

```csharp
public interface IGameCard
{
    byte CardValue { get; }   // 0-31 (see CardHelper for mapping)
    byte DeckIndex { get; }   // 0-3 (suit/deck index)
    byte CardId { get; }      // Full card ID
    bool IsJoker { get; }     // True if card is a joker
}
```

---

## CardHelper - Card Utilities

**Namespace:** `BeziqueCore`

```csharp
public static class CardHelper
{
    public const byte JOKER = 32;          // Joker value
    public const byte CARDS_PER_DECK = 32;   // Cards per deck
    public const byte DECK_COUNT = 4;        // Number of decks

    // Create a card ID from value and deck index
    public static byte CreateCardId(byte cardValue, byte deckIndex);

    // Extract components from card ID
    public static byte GetCardValue(byte cardId);  // Returns 0-31
    public static byte GetDeckIndex(byte cardId);   // Returns 0-3

    // Check for joker
    public static bool IsJoker(byte cardId);
}
```

**Card ID Format:**
- Bits 0-5: Card value (0-31, where 0-31 are standard cards, 32 is joker)
- Bits 6-7: Deck index (0-3)

---

## Offline Integration - Direct SDK Usage

### Basic Setup

```csharp
using BeziqueCore;
using BeziqueCore.Interfaces;
using System;

public class BeziqueOfflineManager
{
    private BeziqueGame _game;
    private int _localPlayerIndex = 0;

    // ===== Initialization =====

    public void StartNewGame(int playerCount = 2)
    {
        _game = new BeziqueGame();
        _game.Initialize(playerCount);
        _game.StartDealing();

        // Subscribe to events
        _game.CardsDealt += OnCardsDealt;
        _game.TrumpCardFlipped += OnTrumpCardFlipped;
        _game.PhaseChanged += OnPhaseChanged;
        _game.DealerBonusPoints += OnDealerBonusPoints;

        // Deal all cards (3 sets of 3 cards per player)
        DealAllCards();
    }

    private void DealAllCards()
    {
        while (_game.DealNextSet())
        {
            // CardsDealt event fires for each player
        }

        // Complete dealing to flip trump
        _game.CompleteDealing();
    }

    // ===== Event Handlers =====

    private void OnCardsDealt(int playerIndex, IReadOnlyList<IGameCard> cards)
    {
        Console.WriteLine($"Player {playerIndex} dealt {cards.Count} cards");
        // Update UI with new cards
    }

    private void OnTrumpCardFlipped(IGameCard trumpCard)
    {
        Console.WriteLine($"Trump: Deck {trumpCard.DeckIndex}, Value {trumpCard.CardValue}");
        // Update UI with trump suit
    }

    private void OnPhaseChanged(GamePhase newPhase)
    {
        Console.WriteLine($"Phase changed to: {newPhase}");
        // Update game state UI
    }

    private void OnDealerBonusPoints(int dealerIndex)
    {
        Console.WriteLine($"Dealer {dealerIndex} gets 10 points for trump 7!");
    }

    // ===== Gameplay =====

    public bool PlayCard(byte cardId)
    {
        if (!_game.IsPlayerTurn(_localPlayerIndex))
        {
            Console.WriteLine("Not your turn!");
            return false;
        }

        var cards = _game.GetPlayableCards(_localPlayerIndex);
        if (!cards.Contains(cardId))
        {
            Console.WriteLine("Card not in hand!");
            return false;
        }

        _game.PlayCard(_localPlayerIndex, cardId);
        return true;
    }

    public void DrawCard()
    {
        if (_game.CurrentPhase == GamePhase.Phase1_Playing)
        {
            _game.DrawCard(_localPlayerIndex);
        }
    }

    // ===== Query Methods =====

    public List<byte> GetLocalPlayerHand()
    {
        var result = new List<byte>();
        if (_game == null) return result;

        var players = _game.Players;
        if (_localPlayerIndex >= players.Count) return result;

        foreach (var card in players[_localPlayerIndex].Cards)
            result.Add(card.CardId);

        return result;
    }

    public int[] GetScores()
    {
        if (_game == null) return new int[0];

        var players = _game.Players;
        var scores = new int[players.Count];
        for (int i = 0; i < players.Count; i++)
            scores[i] = players[i].Score;

        return scores;
    }

    public GamePhase GetCurrentPhase() => _game?.CurrentPhase ?? GamePhase.Dealing;
    public int GetCurrentPlayer() => _game?.CurrentPlayerIndex ?? -1;
}
```

---

## Online Integration (gRPC)

### Prerequisites

1. **Install gRPC packages for Unity:**
   - `Grpc.Net.Client`
   - `Google.Protobuf`
   - `Grpc.Tools` (for code generation)

2. **Generate C# code from proto:**
   ```bash
   protoc -I. --csharp_out . --grpc_out . bezique.proto
   ```

### gRPC Service API

The server implements the following RPCs:

| RPC | Request | Response | Description |
|-----|---------|----------|-------------|
| `StartGame` | `PlayerCount` | `GameStarted` | Initialize and start dealing |
| `DealCard` | `DealCardRequest` | `DealCardResponse` | Deal next set of cards |
| `GetGameState` | `GameStateRequest` | `GameState` | Query current state |
| `PlayCard` | `PlayCardRequest` | `PlayCardResponse` | Play a card |
| `StartGame` | `PlayerCount` | `GameStarted` | Initialize and start dealing |
| `DealCard` | `DealCardRequest` | `DealCardResponse` | Deal next set of cards |
| `GetGameState` | `GameStateRequest` | `GameState` | Query current state |
| `PlayCard` | `PlayCardRequest` | `PlayCardResponse` | Play a card |

### Unity gRPC Client Example

```csharp
using Grpc.Core;
using GrpcService;
using System;
using System.Threading.Tasks;

public class BeziqueOnlineManager
{
    private Channel _channel;
    private Bezique.BeziqueClient _client;
    private int _currentGameId = -1;

    public bool IsConnected => _channel?.State == ChannelState.Ready;

    public async Task ConnectAsync(string serverAddress = "127.0.0.1:5275")
    {
        _channel = new Channel(serverAddress, ChannelCredentials.Insecure);
        _client = new Bezique.BeziqueClient(_client);

        await Task.CompletedTask; // Connection establishes on first call
        Console.WriteLine($"Connected to {serverAddress}");
    }

    public async Task<int> StartGameAsync(int playerCount)
    {
        var request = new PlayerCount { PlayerCount_ = playerCount };
        var response = await _client.StartGameAsync(request);

        _currentGameId = response.GameId;
        Console.WriteLine(response.GameStartMessage);

        return _currentGameId;
    }

    public async Task<DealCardResponse> DealCardsAsync()
    {
        var request = new DealCardRequest { GameId = _currentGameId };
        return await _client.DealCardAsync(request);
    }

    public async Task<GameState> GetGameStateAsync()
    {
        var request = new GameStateRequest { GameId = _currentGameId };
        return await _client.GetGameStateAsync(request);
    }

    public async Task<PlayCardResponse> PlayCardAsync(int playerIndex, int cardId)
    {
        var request = new PlayCardRequest
        {
            GameId = _currentGameId,
            PlayerIndex = playerIndex,
            CardId = cardId
        };

        return await _client.PlayCardAsync(request);
    }

    public async Task DisconnectAsync()
    {
        await _channel.ShutdownAsync();
        _channel = null;
    }
}
```

---

## Complete Unity Example

```csharp
using BeziqueCore;
using BeziqueCore.Interfaces;
using UnityEngine;
using UnityEngine.UI;

public class BeziqueGameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int playerCount = 2;
    [SerializeField] private bool useOnlineMode = false;

    [Header("UI References")]
    [SerializeField] private Text phaseText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Transform handContainer;
    [SerializeField] private GameObject cardPrefab;

    private BeziqueGame _game;
    private int _localPlayerIndex = 0;

    private void Start()
    {
        if (!useOnlineMode)
        {
            StartOfflineGame();
        }
    }

    private void StartOfflineGame()
    {
        _game = new BeziqueGame();
        _game.Initialize(playerCount);

        // Subscribe to events
        _game.CardsDealt += OnCardsDealt;
        _game.TrumpCardFlipped += OnTrumpCardFlipped;
        _game.PhaseChanged += OnPhaseChanged;
        _game.DealerBonusPoints += OnDealerBonus;

        // Start dealing
        _game.StartDealing();
        StartCoroutine(DealAllCards());
    }

    private System.Collections.IEnumerator DealAllCards()
    {
        while (_game.DealNextSet())
        {
            yield return new WaitForSeconds(0.2f); // Visual delay
        }

        _game.CompleteDealing();
        RefreshUI();
    }

    // ===== Event Handlers =====

    private void OnCardsDealt(int playerIndex, IReadOnlyList<IGameCard> cards)
    {
        if (playerIndex == _localPlayerIndex)
        {
            Debug.Log($"You received {cards.Count} cards");
        }
    }

    private void OnTrumpCardFlipped(IGameCard trumpCard)
    {
        Debug.Log($"Trump is deck {trumpCard.DeckIndex}");
        RefreshUI();
    }

    private void OnPhaseChanged(GamePhase phase)
    {
        phaseText.text = $"Phase: {phase}";
        RefreshUI();
    }

    private void OnDealerBonus(int dealerIndex)
    {
        Debug.Log($"Dealer {dealerIndex} gets 10 bonus points!");
        RefreshUI();
    }

    // ===== UI Methods =====

    public void OnCardClicked(byte cardId)
    {
        if (_game.IsPlayerTurn(_localPlayerIndex))
        {
            _game.PlayCard(_localPlayerIndex, cardId);
            RefreshUI();
        }
    }

    public void OnDrawButtonClicked()
    {
        if (_game.IsPlayerTurn(_localPlayerIndex) &&
            _game.CurrentPhase == GamePhase.Phase1_Playing)
        {
            _game.DrawCard(_localPlayerIndex);
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        // Update scores
        string scores = "";
        for (int i = 0; i < _game.Players.Count; i++)
        {
            scores += $"P{i}: {_game.Players[i].Score}  ";
        }
        scoreText.text = scores;

        // Update hand
        foreach (Transform child in handContainer)
        {
            Destroy(child.gameObject);
        }

        var hand = _game.Players[_localPlayerIndex].Cards;
        for (int i = 0; i < hand.Count; i++)
        {
            var cardObj = Instantiate(cardPrefab, handContainer);
            var cardUI = cardObj.GetComponent<BeziqueCardUI>();
            cardUI.SetCard(hand[i].CardId);
            cardUI.OnClick.AddListener(() => OnCardClicked(hand[i].CardId));
        }
    }
}
```

---

## Known Limitations

### Currently NOT Implemented

| Feature | Status |
|---------|--------|
| **Event `CardDrawn`** | Event defined but never fires in `DrawCard()` |
| **Event `TrickComplete`** | Event defined but never fires |
| **Event `MeldPointsAwarded`** | Event defined but never fires |
| **Event `RoundEnd`** | Event defined but never fires |
| **Event `GameEnd`** | Event defined but never fires |
| **Meld Validation** | `CreateMeld()` accepts any meld without card validation |
| **Playable Card Validation** | `GetPlayableCards()` returns entire hand, not trick-legal cards |
| **Trick Winner Detection** | No API to get who won the last trick |
| **Turn Management** | Must manually track turn order via state polling |
| **Network Sync** | No built-in networking; use gRPC for online play |
| **Event `CardDrawn`** | Event defined but never fires in `DrawCard()` |
| **Event `TrickComplete`** | Event defined but never fires |
| **Event `MeldPointsAwarded`** | Event defined but never fires |
| **Event `RoundEnd`** | Event defined but never fires |
| **Event `GameEnd`** | Event defined but never fires |
| **Meld Validation** | `CreateMeld()` accepts any meld without card validation |
| **Playable Card Validation** | `GetPlayableCards()` returns entire hand, not trick-legal cards |
| **Trick Winner Detection** | No API to get who won the last trick |
| **Turn Management** | Must manually track turn order via state polling |
| **Network Sync** | No built-in networking; use gRPC for online play |

### Workarounds

1. **For `CardDrawn` event**: Poll `DeckCount` before/after draw to detect if card was drawn
2. **For `TrickComplete`**: Monitor `CurrentPlayerIndex` changes to detect trick end
3. **For meld validation**: Implement client-side validation before calling `CreateMeld()`
4. **For playable cards**: Implement your own logic based on lead card and trump suit

---

## Game Flow Diagram

```
Initialize(2 or 4)
    |
    v
StartDealing()
    |
    +---> DealNextSet() [repeat 3 times per player]
    |         |
    |         v
    |     [CardsDealt event fires]
    |
    v
CompleteDealing()
    |
    +---> [TrumpCardFlipped event fires]
    |
    v
Phase1_Playing
    |
    +---> PlayCard(player, cardId)
    |       |
    |       v
    |   [Player turn changes]
    |
    +---> DrawCard(player) [until deck empty]
    |
    v
Phase2_Playing [when deck empty]
    |
    +---> PlayCard(player, cardId) [no drawing]
    |
    v
RoundEnd / GameEnd [events not fired]
```

---

## Card ID Reference

### Deck Layout
- **4 decks** (index 0-3), each with **33 cards** (32 standard + 1 joker)
- **Total: 132 cards**

### Card Values (0-31 in each deck)
| Value | Meaning |
|-------|---------|
| 0-31 | Standard card in deck |
| 32 | Joker |
| 0-31 | Standard card in deck |
| 32 | Joker |

Use `CardHelper` to convert:
```csharp
byte cardId = CardHelper.CreateCardId(cardValue: 5, deckIndex: 0);
byte value = CardHelper.GetCardValue(cardId);  // 5
byte deck = CardHelper.GetDeckIndex(cardId);   // 0
bool isJoker = CardHelper.IsJoker(cardId);    // false
```

---


## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | Current | Initial release with basic gameplay, dealing, and state management |
