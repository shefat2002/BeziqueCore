You are an elite .NET Systems Architect. You are writing the **Implementation Layer** for a card game SDK where the Core Logic is driven by an auto-generated Finite State Machine (FSM).

**Context & Architecture:**
1.  **The Driver (Client):** `Bezique.cs` (Auto-Generated FSM). It dictates *when* things happen. It calls `IBeziqueAdapter`. **Do not generate this.**
2.  **The Contract:** `IBeziqueAdapter.cs`. The interface the FSM talks to.
3.  **The Bridge (Adapter):** `BeziqueConcrete.cs`. Implements `IBeziqueAdapter`. It translates FSM commands into Logic calls.
4.  **The Worker (Adaptee):** `BeziqueImp.cs`. Holds the DOD Data (`struct`) and executes the heavy lifting.

**Global Constraints:**
1.  **Pure C# .NET:** 
2.  **No Documentation:** No XML summaries. Side comments only (`// comment`).
3.  **Zero-Allocation:** Use `struct`, `ref`, and `Span<T>` where possible.
4.  **Structure:** Follow the 4-Layer DOD pattern inside the Implementation files.

---

## 🏛️ IMPLEMENTATION ARCHITECTURE

### 1. Layer A: Data (Inside `BeziqueImp.cs`)
*   **Role:** The raw memory layout of the game.
*   **Rules:** `struct` only. `[StructLayout]`. No logic.

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct BeziqueData
{
    public int PlayerScore;
    public int OpponentScore;
    public int CardsInDeck;
    
    // Fixed buffer for zero-alloc hand management
    public unsafe fixed byte PlayerHand[8]; 

    public override string ToString() => $"[Data] Deck:{CardsInDeck} P_Score:{PlayerScore}";
}
```

### 2. Layer B: Pure Logic (Helper Static Class)
*   **Role:** Stateless rules (Math, Scoring, Shuffling).
*   **Location:** Can be nested in `BeziqueImp` or a separate `BeziqueLogic` class.

```csharp
public static class BeziqueRules
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CalculateMeld(int cardA, int cardB, out int points)
    {
        points = (cardA == cardB) ? 20 : 0; // Simplified rule
    }
}
```

### 3. Layer C: The Adaptee (`BeziqueImp.cs`)
*   **Role:** Holds the `struct` state and exposes "How-To" methods.
*   **Rules:** Uses `ref` extensions. Contains the actual game algorithms.

```csharp
public class BeziqueImp
{
    private BeziqueData _data; // The State

    public void PerformShuffle()
    {
        // Fisher-Yates logic here modifying _data
        _data.CardsInDeck = 64; 
    }

    public bool TryScoreMeld(out int points)
    {
        BeziqueRules.CalculateMeld(1, 1, out points); // Call Logic
        _data.PlayerScore += points; // Mutate State
        return points > 0;
    }
}
```

### 4. Layer D: The Adapter (`BeziqueConcrete.cs`)
*   **Role:** Connects the FSM (`Bezique.cs`) to the Implementation (`BeziqueImp.cs`).
*   **Rules:** Implements `IBeziqueAdapter`. **Zero logic here**, only delegation.

```csharp
public class BeziqueConcrete : IBeziqueAdapter
{
    private readonly BeziqueImp _imp;

    public BeziqueConcrete() => _imp = new BeziqueImp();

    // FSM calls this -> Adapter delegates to Imp
    void IBeziqueAdapter.ProcessCardLogic()
    {
        _imp.PerformShuffle(); // Delegate
    }

    void IBeziqueAdapter.UpdateScore()
    {
        _imp.TryScoreMeld(out _); // Delegate
    }
}
```

---

## 📄 GENERATION TEMPLATE
**Generate the following files based on the "Bezique" Card Game logic:**

### File 1: `IBeziqueAdapter.cs` (The Contract)
```csharp
public interface IBeziqueAdapter
{
    void OnStartGame();
    void OnDealCards();
    bool CheckValidMove(int cardId);
}
```

### File 2: `BeziqueImp.cs` (The Adaptee - Layer A, B, C)
```csharp
// LAYER A: DATA
[StructLayout(LayoutKind.Sequential)]
public struct GameStateData
{
    public int DeckCount;
    public int PlayerScore;
    public bool IsRoundActive;

    public override string ToString() => $"[Bezique] Deck:{DeckCount} Score:{PlayerScore}";
}

// LAYER B: LOGIC
public static class GameRules
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DecrementDeck(int current, out int result)
    {
        result = Math.Max(0, current - 1); // Atomic math
    }
}

// LAYER C: MECHANICS (The Worker)
public class BeziqueImp
{
    private GameStateData _state;

    public BeziqueImp()
    {
        _state = new GameStateData { DeckCount = 64, IsRoundActive = true };
    }

    public void ExecuteDeal()
    {
        // Implementation of shuffling/dealing
        GameRules.DecrementDeck(_state.DeckCount, out _state.DeckCount);
    }

    public bool ValidateMove(int cardId)
    {
        return _state.IsRoundActive; // Check state
    }

    public string GetDebugString() => _state.ToString();
}
```

### File 3: `BeziqueConcrete.cs` (The Adapter - Layer D)
```csharp
public class BeziqueConcrete : IBeziqueAdapter
{
    private readonly BeziqueImp _imp;

    public BeziqueConcrete()
    {
        _imp = new BeziqueImp(); // Init Worker
    }

    // FSM Command -> Adapter -> Worker
    void IBeziqueAdapter.OnStartGame()
    {
        Console.WriteLine($"[Adapter] Game Started: {_imp.GetDebugString()}"); // Log
    }

    void IBeziqueAdapter.OnDealCards()
    {
        _imp.ExecuteDeal(); // Delegate
    }

    bool IBeziqueAdapter.CheckValidMove(int cardId)
    {
        return _imp.ValidateMove(cardId); // Delegate return value
    }
}
```