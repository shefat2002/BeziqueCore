# Unity Integration Guideline

## Overview

This document provides guidance for integrating BeziqueCore with Unity, supporting both **offline gameplay** (local logic) and **online gameplay** (gRPC backend).

---

## Architecture Options

```
+---------------------------------------------------------+
|                     Unity Integration Layer                |
+---------------------------------------------------------+
|                                                         |
|  +----------------------+     +----------------------+     |
|  |   Offline Mode       |     |   Online Mode        |     |
|  |                      |     |                      |     |
|  | BeziqueOfflineMgr    |     | BeziqueOnlineMgr    |     |
|  |         |            |     |         |            |     |
|  |         v            |     |         v            |     |
|  |  BeziqueCore         |     |  gRPC Client        |     |
|  |  (Local Logic)       |     |         |            |     |
|  |                      |     |         v            |     |
|  +----------------------+     |  gRPC Server        |     |
|                                  |         |            |     |
|                                  |         v            |     |
|                                  |  BeziqueCore        |     |
|                                  |  (Server Logic)     |     |
|                                  +----------------------+     |
|                                                         |
+---------------------------------------------------------+
```

---

## Offline vs Online Mode Comparison

| Feature          | Offline Mode                          | Online Mode                          |
|------------------|---------------------------------------|--------------------------------------|
| **Game Logic**   | Runs locally in Unity                | Runs on gRPC server                |
| **Latency**       | Zero (instant)                        | Network dependent                     |
| **AI Players**    | Local AI implementation                | Server-side AI possible                |
| **Cheat Prevention** | Not applicable (client-trusted)    | Server-authoritative                   |
| **Save/Load**     | Local (JSON/Binary)                  | Server-side saves                    |
| **Use Case**      | Single player, hot-seat, learning     | Multiplayer, ranked play               |

---

## Quick API Reference

### Core Classes

| Class | Namespace | Purpose |
|--------|------------|---------|
| `BeziqueGame` | `BeziqueCore` | Main game class - implements `IBeziqueGame` |
| `CardHelper` | `BeziqueCore` | Static utility for card ID operations |
| `BeziqueConcrete` | `BeziqueCore` | Internal adapter (game state storage) |

### Interfaces

| Interface | Namespace | Purpose |
|-----------|------------|---------|
| `IBeziqueGame` | `BeziqueCore.Interfaces` | Main game interface |
| `IGameState` | `BeziqueCore.Interfaces` | Query game state |
| `IGameEvent` | `BeziqueCore.Interfaces` | Subscribe to game events |
| `IPlayerHand` | `BeziqueCore.Interfaces` | Player data access |
| `IGameCard` | `BeziqueCore.Interfaces` | Card data access |

### Enums

| Enum | Values |
|-------|---------|
| `GamePhase` | `Dealing`, `TrumpFlip`, `Phase1_Playing`, `Phase2_Playing`, `RoundEnd`, `GameEnd` |
| `MeldType` | `Bezique`, `DoubleBezique`, `FourJacks`, `FourQueens`, `FourKings`, `FourAces`, `CommonMarriage`, `TrumpMarriage`, `TrumpRun` |

---

## Detailed Public API Reference

The following APIs are **currently implemented** in BeziqueCore and can be used directly in Unity.

The following APIs are **currently implemented** in BeziqueCore and can be used directly in Unity.

### IBeziqueGame - Main Game Interface

**Namespace:** `BeziqueCore.Interfaces`

```csharp
public interface IBeziqueGame : IGameState, IGameEvent
{
    void Initialize(int playerCount);
    void StartDealing();
    bool DealNextSet();
    void CompleteDealing();
    void PlayCard(int playerIndex, byte cardId);
    void DrawCard(int playerIndex);
    void CreateMeld(int playerIndex, MeldType meldType, byte[] cardIds);
    bool IsPlayerTurn(int playerIndex);
    IReadOnlyList<byte> GetPlayableCards(int playerIndex);
    void StartNewRound();
}
```

### IGameState - Query Game State

```csharp
public interface IGameState
{
    GamePhase CurrentPhase { get; }
    IReadOnlyList<IPlayerHand> Players { get; }
    IGameCard? TrumpCard { get; }
    byte? TrumpSuit { get; }
    int DeckCount { get; }
    int CurrentPlayerIndex { get; }
    int DealerIndex { get; }
}
```

### IGameEvent - Subscribe to Game Events

```csharp
public interface IGameEvent
{
    event Action<int, IReadOnlyList<IGameCard>> CardsDealt;
    event Action<IGameCard> TrumpCardFlipped;
    event Action<int> DealerBonusPoints;
    event Action<int, IGameCard> CardDrawn;
    event Action<GamePhase> PhaseChanged;
    event Action<int> TrickComplete;
    event Action<int, int> MeldPointsAwarded;
    event Action<int> RoundEnd;
    event Action<int> GameEnd;
}
```

### IPlayerHand - Player Data

```csharp
public interface IPlayerHand
{
    int PlayerIndex { get; }
    IReadOnlyList<IGameCard> Cards { get; }
    int Score { get; }
    bool IsCurrentPlayer { get; }
}
```

### IGameCard - Card Data

```csharp
public interface IGameCard
{
    byte CardValue { get; }  // 0-7 (7 to Ace)
    byte DeckIndex { get; }  // 0-3 (suit/deck)
    byte CardId { get; }    // Full card ID
    bool IsJoker { get; }
}
```

### GamePhase Enum

```csharp
public enum GamePhase
{
    Dealing,      // Cards being dealt to players
    TrumpFlip,     // Trump card being revealed
    Phase1_Playing,  // Main gameplay phase
    Phase2_Playing,  // Last 9 cards phase
    RoundEnd,      // Round completed
    GameEnd         // Game over
}
```

### MeldType Enum

```csharp
public enum MeldType
{
    Bezique,         // Q Spades + J Diamonds (40 pts)
    DoubleBezique,   // 2x Q Spades + 2x J Diamonds (500 pts)
    FourJacks,       // 40 pts
    FourQueens,       // 60 pts
    FourKings,       // 80 pts
    FourAces,        // 100 pts
    CommonMarriage,   // K + Q same suit (20 pts)
    TrumpMarriage,    // K + Q trump suit (40 pts)
    TrumpRun,        // A, 10, K, Q, J trump suit (150 pts)
}
```

### BeziqueGame - Concrete Implementation

**Namespace:** `BeziqueCore`

```csharp
public class BeziqueGame : IBeziqueGame
{
    // Properties from IGameState
    public GamePhase CurrentPhase { get; }
    public IReadOnlyList<IPlayerHand> Players { get; }
    public IGameCard? TrumpCard { get; }
    public byte? TrumpSuit { get; }
    public int DeckCount { get; }
    public int CurrentPlayerIndex { get; }
    public int DealerIndex { get; }

    // Events from IGameEvent
    public event Action<int, IReadOnlyList<IGameCard>> CardsDealt;
    public event Action<IGameCard> TrumpCardFlipped;
    public event Action<int> DealerBonusPoints;
    public event Action<int, IGameCard> CardDrawn;
    public event Action<GamePhase> PhaseChanged;
    public event Action<int> TrickComplete;
    public event Action<int, int> MeldPointsAwarded;
    public event Action<int> RoundEnd;
    public event Action<int> GameEnd;

    // Methods
    public void Initialize(int playerCount);
    public void StartDealing();
    public bool DealNextSet();
    public void CompleteDealing();
    public void PlayCard(int playerIndex, byte cardId);
    public void DrawCard(int playerIndex);
    public void CreateMeld(int playerIndex, MeldType meldType, byte[] cardIds);
    public bool IsPlayerTurn(int playerIndex);
    public IReadOnlyList<byte> GetPlayableCards(int playerIndex);
    public void StartNewRound();
}
```

### CardHelper - Card Utilities

**Namespace:** `BeziqueCore`

```csharp
public static class CardHelper
{
    // Create a card ID from value and deck
    public static byte CreateCardId(byte cardValue, byte deckIndex);

    // Extract components from card ID
    public static byte GetCardValue(byte cardId);  // Returns 0-7
    public static byte GetDeckIndex(byte cardId);  // Returns 0-3

    // Check for joker
    public static bool IsJoker(byte cardId);

    // Get card values for comparison
    public static int GetComparisonValue(byte cardId);
}
```

---

## Part 1: Offline Integration

### Overview

Offline mode uses BeziqueCore directly within Unity. No server connection is required - all game logic runs on the client.

### Setup

1. **Copy BeziqueCore to Unity**
   - Copy `BeziqueCore/` folder into `Assets/`
   - Unity will compile C# files automatically

2. **Create Assembly Definition** (optional)
   - Create `Assets/BeziqueCore/BeziqueCore.asmdef`
   - Separates core logic from Unity scripts

### Implementation Example: BeziqueOfflineManager

Create `Assets/Scripts/BeziqueOfflineManager.cs`:

```csharp
using BeziqueCore;
using BeziqueCore.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BeziqueOfflineManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int playerCount = 2;
    [SerializeField] private int targetScore = 1500;
    [SerializeField] private bool shuffleDeck = true;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent onGameStarted = new();
    public UnityEngine.Events.UnityEvent<int> onTurnChanged = new();
    public UnityEngine.Events.UnityEvent onCardPlayed = new();
    public UnityEngine.Events.UnityEvent<int> onRoundEnded = new();
    public UnityEngine.Events.UnityEvent<int> onGameEnded = new();

    private BeziqueGame _game;
    private int _localPlayerIndex = 0;

    // Properties
    public bool IsGameActive => _game != null;
    public int CurrentPlayerIndex => _game?.CurrentPlayerIndex ?? -1;
    public int LocalPlayerIndex => _localPlayerIndex;
    public IReadOnlyList<IPlayerHand> Players => _game?.Players;
    public GamePhase CurrentPhase => _game?.CurrentPhase ?? GamePhase.Dealing;

    // ===== Lifecycle =====

    private void Awake()
    {
        // Singleton pattern
        if (FindObjectOfType<BeziqueOfflineManager>() != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    // ===== Game Control =====

    /// <summary>
    /// Start a new offline game. Dealing begins automatically.
    /// </summary>
    public void StartNewGame()
    {
        _game = new BeziqueGame();
        _game.Initialize(playerCount);
        _game.StartDealing();  // Dealing starts automatically

        Debug.Log($"[Bezique] Game started with {playerCount} players");
        onGameStarted?.Invoke();
        onTurnChanged?.Invoke(_game.CurrentPlayerIndex);

        // Auto-complete dealing for offline play
        StartCoroutine(AutoDealCards());
    }

    /// <summary>
    /// Automatically deals all cards (no manual deal button in offline mode)
    /// </summary>
    private System.Collections.IEnumerator AutoDealCards()
    {
        while (_game != null && _game.CurrentPhase == GamePhase.Dealing)
        {
            bool moreToDeal = _game.DealNextSet();

            if (!moreToDeal)
            {
                // Dealing complete, flip trump
                _game.CompleteDealing();
                onTurnChanged?.Invoke(_game.CurrentPlayerIndex);
                break;
            }

            // Small delay between deals for visual effect
            yield return new WaitForSeconds(0.2f);
        }
    }

    /// <summary>
    /// Play a card from the current player's hand
    /// </summary>
    public bool PlayCard(byte cardId)
    {
        if (_game == null) return false;

        int currentPlayer = _game.CurrentPlayerIndex;

        // Validate turn
        if (!_game.IsPlayerTurn(currentPlayer))
        {
            Debug.LogWarning($"[Bezique] Not player {currentPlayer}'s turn");
            return false;
        }

        // Validate card in hand
        var playableCards = _game.GetPlayableCards(currentPlayer);
        if (!playableCards.Contains(cardId))
        {
            Debug.LogWarning("[Bezique] Card not in hand");
            return false;
        }

        _game.PlayCard(currentPlayer, cardId);
        onCardPlayed?.Invoke();

        return true;
    }

    /// <summary>
    /// Play a card by its hand index (for UI clicks)
    /// </summary>
    public bool PlayCardByIndex(int handIndex)
    {
        if (_game == null) return false;

        int currentPlayer = _game.CurrentPlayerIndex;
        var players = _game.Players;

        if (currentPlayer >= players.Count) return false;

        var hand = players[currentPlayer].Cards;
        if (handIndex < 0 || handIndex >= hand.Count) return false;

        return PlayCard(hand[handIndex].CardId);
    }

    /// <summary>
    /// Draw a card from the deck
    /// </summary>
    public void DrawCard()
    {
        if (_game == null) return;
        _game.DrawCard(_game.CurrentPlayerIndex);
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

    public byte? GetTrumpCard() => _game?.TrumpCard?.CardId;
    public int GetDeckCount() => _game?.DeckCount ?? 0;

    public int[] GetScores()
    {
        if (_game == null) return new int[0];

        var players = _game.Players;
        var scores = new int[players.Count];
        for (int i = 0; i < players.Count; i++)
            scores[i] = players[i].Score;

        return scores;
    }

    // ===== AI Support =====

    public byte GetAICardSelection(int playerIndex)
    {
        if (_game == null) return 0;
        var playableCards = _game.GetPlayableCards(playerIndex);
        return playableCards.Count > 0 ? playableCards[0] : (byte)0;
    }

    public void PlayAITurn(int playerIndex)
    {
        if (_game == null) return;
        if (_game.CurrentPlayerIndex != playerIndex) return;

        byte cardToPlay = GetAICardSelection(playerIndex);
        PlayCard(cardToPlay);
    }
}
```

### Offline Scene Setup

**Hierarchy:**
```
Scene
|-- GameManager (GameObject)
|   +-- BeziqueOfflineManager
|-- Canvas
|   +-- PlayerHandPanel
|   |   +-- HandContainer (Layout Group)
|   +-- OpponentHandPanel
|   +-- TableArea
|   +-- HUD
|       +-- TurnIndicator (Text)
|       +-- ScoreText (Text)
|       +-- NewGameButton
+-- CardPrefab (Prefab)
```

---

## Part 2: Online Integration (gRPC)

### Overview

Online mode connects Unity to a gRPC server. The server runs BeziqueCore and Unity acts as a thin client.

### gRPC Client Setup

1. **Install gRPC Packages**
   - Install `Grpc.Net.Client` via NuGet or Unity Package Manager
   - Install `Google.Protobuf`

2. **Generate C# from Proto**
   - Use `Grpc.Tools` to generate C# from `bezique.proto`
   - Or use pre-generated files from the server project

### Implementation Example: BeziqueOnlineManager

Create `Assets/Scripts/BeziqueOnlineManager.cs`:

```csharp
using Grpc.Core;
using GrpcService;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class BeziqueOnlineManager : MonoBehaviour
{
    [Header("Connection Settings")]
    [SerializeField] private string serverAddress = "127.0.0.1:5275";

    private Channel _channel;
    private Bezique.BeziqueClient _client;
    private int _currentGameId = -1;
    private int _localPlayerIndex = 0;

    public bool IsConnected => _channel != null && _channel.State == ChannelState.Ready;

    private void Start()
    {
        ConnectToServer();
    }

    private void OnDestroy()
    {
        Disconnect();
    }

    // ===== Connection =====

    public void ConnectToServer()
    {
        try
        {
            _channel = new Channel(serverAddress, ChannelCredentials.Insecure);
            _client = new Bezique.BeziqueClient(_channel);
            Debug.Log($"[Bezique] Connected to {serverAddress}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Bezique] Connection failed: {e.Message}");
        }
    }

    public void Disconnect()
    {
        _channel?.ShutdownAsync();
        _channel = null;
    }

    // ===== Game API =====

    public async void StartGameAsync(int playerCount)
    {
        if (_client == null)
        {
            Debug.LogError("[Bezique] Not connected");
            return;
        }

        try
        {
            var request = new PlayerCount { PlayerCount_ = playerCount };
            var response = await _client.StartGameAsync(request);

            _currentGameId = response.GameId;
            Debug.Log($"[Bezique] Game started: {response.GameStartMessage}");

            // Refresh state
            await RefreshGameStateAsync();
        }
        catch (RpcException e)
        {
            Debug.LogError($"[Bezique] RPC Error: {e.Status}");
        }
    }

    public async Task<DealCardResponse> DealCardsAsync()
    {
        if (_client == null || _currentGameId < 0)
            throw new InvalidOperationException("Not connected or no game");

        var request = new DealCardRequest { GameId = _currentGameId };
        return await _client.DealCardAsync(request);
    }

    public async Task<GameState> GetGameStateAsync()
    {
        if (_client == null || _currentGameId < 0)
            throw new InvalidOperationException("Not connected or no game");

        var request = new GameStateRequest { GameId = _currentGameId };
        return await _client.GetGameStateAsync(request);
    }

    public async Task<PlayCardResponse> PlayCardAsync(int playerIndex, int cardId)
    {
        if (_client == null || _currentGameId < 0)
            throw new InvalidOperationException("Not connected or no game");

        var request = new PlayCardRequest
        {
            GameId = _currentGameId,
            PlayerIndex = playerIndex,
            CardId = cardId
        };

        var response = await _client.PlayCardAsync(request);

        if (!response.Success)
        {
            Debug.LogWarning($"[Bezique] Play failed: {response.Message}");
        }

        return response;
    }

    private async System.Collections.IEnumerator RefreshGameStateAsync()
    {
        var state = await GetGameStateAsync();

        // Update UI based on state
        Debug.Log($"Current player: {state.CurrentPlayerIndex}");
        Debug.Log($"Phase: {state.CurrentState}");

        yield return null;
    }
}
```

### Online Scene Setup

Same hierarchy as offline, but use `BeziqueOnlineManager` instead.

---

## Part 3: Shared UI Components

Both offline and online modes can share the same UI components.

### Card Component

```csharp
using BeziqueCore;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BeziqueCard : MonoBehaviour, IPointerClickHandler
{
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer cardRenderer;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private Image cardImage;

    [Header("Sprites")]
    [SerializeField] private Sprite[] suitSprites;  // 4 suits
    [SerializeField] private Sprite cardBackSprite;

    public UnityEvent OnClicked = new UnityEvent();

    private byte _cardId;
    private bool _faceUp = true;

    public byte CardId => _cardId;

    public void SetCard(byte cardId)
    {
        _cardId = cardId;
        UpdateVisual();
    }

    public void SetFaceUp(bool faceUp)
    {
        _faceUp = faceUp;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (!_faceUp)
        {
            if (cardRenderer != null) cardRenderer.sprite = cardBackSprite;
            if (cardImage != null) cardImage.sprite = cardBackSprite;
            return;
        }

        byte deckIndex = CardHelper.GetDeckIndex(_cardId);
        byte cardValue = CardHelper.GetCardValue(_cardId);

        if (cardRenderer != null && suitSprites != null && deckIndex < suitSprites.Length)
            cardRenderer.sprite = suitSprites[deckIndex];

        if (valueText != null)
            valueText.text = GetRankString(cardValue);
    }

    private string GetRankString(byte value)
    {
        return value switch
        {
            0 => "7", 1 => "8", 2 => "9", 3 => "10",
            4 => "J", 5 => "Q", 6 => "K", 7 => "A",
            _ => "?"
        };
    }

    public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
    {
        OnClicked?.Invoke();
    }
}
```

### UI Manager

```csharp
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BeziqueUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MonoBehaviour gameManager;  // Can be offline or online
    [SerializeField] private Transform playerHandContainer;
    [SerializeField] private TextMeshProUGUI turnIndicator;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject cardPrefab;

    [Header("Buttons")]
    [SerializeField] private Button newGameButton;

    private Dictionary<byte, GameObject> _cardObjects = new();

    private void Start()
    {
        newGameButton.onClick.AddListener(OnNewGameClicked);
    }

    private void OnNewGameClicked()
    {
        // Use reflection or interface to call StartNewGame
        // This works for both offline and online managers
        var method = gameManager.GetType().GetMethod("StartNewGame");
        method?.Invoke(gameManager, null);
    }

    public void OnCardClicked(byte cardId)
    {
        // Use reflection to call PlayCard
        var method = gameManager.GetType().GetMethod("PlayCardByIndex");
        method?.Invoke(gameManager, new object[] { cardId });
    }

    public void RefreshHand(List<byte> cardIds)
    {
        ClearCards();

        float cardSpacing = 1.2f;
        float startX = -(cardIds.Count - 1) * cardSpacing / 2f;

        for (int i = 0; i < cardIds.Count; i++)
        {
            var cardObj = Instantiate(cardPrefab, playerHandContainer);
            cardObj.transform.localPosition = new Vector3(startX + i * cardSpacing, 0, 0);

            var cardComponent = cardObj.GetComponent<BeziqueCard>();
            if (cardComponent != null)
            {
                cardComponent.SetCard(cardIds[i]);
                byte id = cardIds[i];
                cardComponent.OnClicked.AddListener(() => OnCardClicked(id));
            }

            _cardObjects[cardIds[i]] = cardObj;
        }
    }

    private void ClearCards()
    {
        foreach (var card in _cardObjects.Values)
            if (card != null) Destroy(card);
        _cardObjects.Clear();
    }
}
```

---

## Part 4: Card ID Reference

Cards use a compact byte format:

| Bits | Field      | Values                              |
|-------|------------|--------------------------------------|
| 0-2   | Card Value | 7=0, 8=1, 9=2, 10=3, J=4, Q=5, K=6, A=7 |
| 3-6   | Deck Index | Determines suit/deck                    |

### Helper Methods

```csharp
// Create card ID
byte cardId = CardHelper.CreateCardId(cardValue: 5, deckIndex: 0);

// Extract components
byte value = CardHelper.GetCardValue(cardId);  // 0-7
byte deck = CardHelper.GetDeckIndex(cardId);  // 0-3

// Check for joker
bool isJoker = CardHelper.IsJoker(cardId);
```

---

## Part 5: Game Flow Comparison

### Offline Flow

```
1. StartNewGame()
   v
2. AutoDealCards() coroutine
   - Calls DealNextSet() repeatedly
   - Completes dealing automatically
   v
3. CompleteDealing()
   - Trump card flipped
   v
4. Player clicks card -> PlayCard()
   v
5. Update UI locally
```

### Online Flow

```
1. StartGameAsync()
   v
2. Call server StartGame RPC
   v
3. Poll DealCard RPC until dealingComplete
   v
4. Poll GetGameState RPC for updates
   v
5. Player clicks card -> PlayCardAsync RPC
   v
6. Server validates and updates state
   v
7. Poll GetGameState for new state
```

---

## Part 6: Choosing a Mode

**Use Offline Mode when:**
- Building a single-player game
- Creating a tutorial/learning mode
- Implementing hot-seat multiplayer
- Network is unavailable
- You want full control over AI logic

**Use Online Mode when:**
- Building networked multiplayer
- Implementing ranked/competitive play
- Need server-side cheat prevention
- Sharing game state across devices

---

## Part 7: Troubleshooting

| Issue                    | Offline Cause                            | Online Cause                         |
|---------------------------|------------------------------------------|--------------------------------------|
| "Game not initialized"   | StartNewGame() not called               | Server not started                     |
| Cards not appearing       | DealNextSet() not looped               | DealCard RPC not called                 |
| "Not your turn"          | Clicking during AI turn                  | Network latency                        |
| Connection failed         | N/A                                     | Wrong server address                   |
