# Bezique Core SDK – Technical Design Document

**Version:** 1.0 (Final)

**Target:** Backend / Logic Developer

**Scope:** Core Logic (C# Class Library), No UI.

---

## 1. System Architecture & Configuration

The SDK must be stateless in terms of Unity `MonoBehaviour` but stateful regarding the `GameContext`. It is driven by a Finite State Machine (FSM).

### 1.1 Game Configuration
The entry point requires a configuration object to determine rule sets.
```csharp
public class GameConfig {
    public int PlayerCount;       // Valid values: 2 or 4
    public GameMode Mode;         // Enum: Standard or Advanced
    public int TargetScore;       // Valid values: 750, 1000, 1500
}
```

### 1.2 Data Constants
*   **Total Cards:** 132
    *   4 Standard Decks.
    *   Ranks kept: Ace, 10, King, Queen, Jack, 9, 8, 7. (Total 32 * 4 = 128)
    *   Jokers: 4 (1 per deck).
*   **Hand Size:** 9 Cards.
*   **Rank Hierarchy (Highest to Lowest):**
    1.  **Ace** (14)
    2.  **Ten** (13) — *CRITICAL: 10 beats King, Queen, Jack.*
    3.  **King** (12)
    4.  **Queen** (11)
    5.  **Jack** (10)
    6.  **Nine** (9)
    7.  **Eight** (8)
    8.  **Seven** (7)
    9.  **Joker** (15/0) — *Dynamic evaluation (See Section 4.1).*

---

## 2. Points & Scoring Rules

This is the **Master Scoring Table**. The SDK must strictly adhere to these values and conditions.

| Event / Meld | Points | Condition | Mode |
| :--- | :--- | :--- | :--- |
| **Dealer Turned 7** | **10** | Trump Card is Rank 7 (At Deal). | All |
| **Swap Trump 7** | **10** | Player swaps Hand[7] with Table[Trump]. | All |
| **Play Trump 7** | **10** | Player plays Trump 7 into a trick. | All |
| **Meld Trump 7** | **10** | Player melds Trump 7. | All |
| **Trump Marriage** | **40** | King + Queen (Trump Suit). | All |
| **Non-Trump Marriage** | **20** | King + Queen (Same Non-Trump Suit). | All |
| **Bezique** | **40** | Queen Spades + Jack Diamonds. | All |
| **Double Bezique** | **500** | 2x Q-Spades + 2x J-Diamonds. **Must be laid at once.** | All |
| **Trump Run** | **250** | A, 10, K, Q, J (Trump Suit). | All |
| **4 Aces** | **100** | Any suit. (Joker substitutes). | All |
| **4 Kings** | **80** | Any suit. (Joker substitutes). | All |
| **4 Queens** | **60** | Any suit. (Joker substitutes). | All |
| **4 Jacks** | **40** | Any suit. (Joker substitutes). | All |
| **Last Trick** | **10** | Winner of the final trick (Phase 2). | All |
| **Last Trick (w/ 7)**| **20** | Replaces 10pts if winner used Trump 7. | All |
| **Ace/10 Count** | **10 ea**| **Counted at Round End from Won Pile.** | **Advanced ONLY** |

---

## 3. Data Structures

### 3.1 Card & Enums
```csharp
public enum Suit { Diamonds, Clubs, Hearts, Spades }

public enum Rank { Seven=7, Eight=8, Nine=9, Jack=10, Queen=11, King=12, Ten=13, Ace=14 }

public readonly struct Card {
    public byte CardId;          // 0-31 (standard cards), 32 (Joker)
    public sbyte DeckIndex;      // -1 = unassigned/in deck, 0-3 = deck instance

    // Computed properties (no storage overhead)
    public bool IsJoker => CardId == 32;
    public Suit Suit => IsJoker ? default : (Suit)(CardId % 4);
    public Rank Rank => IsJoker ? (Rank)15 : (Rank)(7 + CardId / 4);
}
```

**CardId Mapping:**
- Standard cards (0-31): `CardId = (Rank - 7) * 4 + Suit`
  - Seven of Diamonds: 0, Seven of Clubs: 1, Seven of Hearts: 2, Seven of Spades: 3
  - ...
  - Ace of Diamonds: 28, Ace of Clubs: 29, Ace of Hearts: 30, Ace of Spades: 31
- Joker: 32

**Total Cards:** 132 instances (33 types × 4 decks)

### 3.2 Player State
```csharp
public class Player {
    public int PlayerID;
    public List<Card> Hand;       // Cards available to play
    public List<Card> TableCards; // Melded cards (Visible, but playable into tricks)
    public List<Card> WonPile;    // Cards won (Hidden, used for Advanced Scoring)
    public int RoundScore;        // Points accumulated this round
    public int TotalScore;        // Global game score
    public bool HasSwappedSeven;  // Logic flag
}
```

### 3.3 Game Context (Shared State)
```csharp
public class GameContext {
    public Stack<Card> DrawDeck;
    public Card TrumpCard;        // The face-up card
    public Suit TrumpSuit;
    public GamePhase CurrentPhase; // Phase1_Normal or Phase2_Last9
    public int CurrentTurnPlayer;
    public int LastTrickWinner;   // Controls who can Meld/Draw first
}
```

---

## 4. State Machine Implementation Logic

This section maps directly to the State Diagram nodes.

### 4.1 State: Deal
**Transitions:** `DealFirst` -> `DealMid` -> `DealLast` -> `SelectTrump` -> `Play`

1.  **Logic:**
    *   Create 132 cards.
    *   Shuffle (Fisher-Yates).
    *   Deal 9 cards to each `Player.Hand`.
2.  **Trump Selection:**
    *   Pop `DrawDeck`. Set as `TrumpCard`.
    *   Set `TrumpSuit`.
3.  **Dealer Bonus:**
    *   **Check:** If `TrumpCard.Rank == Rank.Seven`.
    *   **Action:** `Dealer.RoundScore += 10`.

### 4.2 State: Play (Phase 1)
**Transitions:** `PlayFirst` -> `PlayMid` -> `PlayLast` -> `Meld`

**Input:** `PlayerID`, `CardIndex`.

1.  **Validation:**
    *   In Phase 1, a player can play **any** card from `Hand` OR `TableCards`.
    *   If a `TableCard` is played, it is removed from the table (un-melded) and enters the trick.
2.  **Trick Resolution (Algorithm):**
    *   **Goal:** Determine Winner.
    *   **Lead Suit:** Determined by the first card played.
    *   **Joker Logic:**
        *   Lead Joker: Wins automatically UNLESS opponent plays a Trump.
        *   Follow Joker: Value is 0. Always loses.
    *   **Comparison Logic:**
        *   `Trump` > `Non-Trump`.
        *   `Higher Rank` > `Lower Rank` (if suits match).
        *   `Lead Suit` > `Off Suit` (non-trump).
    *   **Scoring Update:** If any player played **Trump 7**, add **10 points** to them immediately.
    *   **Result:** Move all trick cards to Winner's `WonPile`. Set `LastTrickWinner`.

### 4.3 State: Meld (Phase 1 Only)
**Transitions:** `TryMelded` -> (`Melded` or `NoMeld`) -> `AddPoint` -> `NewTrick`

**Access:** Only `LastTrickWinner` can perform actions here.

1.  **Action: Swap Seven**
    *   **Condition:** Player has Trump 7 in Hand AND `TrumpCard` is not 7.
    *   **Execution:** Swap Hand Card <-> `Context.TrumpCard`.
    *   **Score:** +10.
2.  **Action: Declare Meld**
    *   **Input:** List of Card IDs, `MeldType`.
    *   **Validation:**
        *   Check Card Ranks/Suits against definitions (See Section 2).
        *   **Reuse Rule:** A specific Card ID cannot be used for the *same* MeldType twice. (e.g., King of Hearts ID#45 cannot be in "4 Kings" twice. But can be in "4 Kings" and then "Marriage").
        *   **Double Bezique Rule:** All 4 cards (2x QS, 2x JD) must be provided in a **single request**.
    *   **Execution:** Move cards to `Player.TableCards`. Add Score.

### 4.4 State: NewTrick & The Draw
**Transitions:** `AddOneCardToAll` -> (`Play` or `L9Play`)

1.  **Draw Logic:**
    *   **Winner** draws top card of `DrawDeck`.
    *   **Loser(s)** draw subsequent cards.
2.  **Phase Transition Trigger (Crucial):**
    *   **Check BEFORE Draw:** If `DrawDeck.Count == PlayerCount` (e.g., 2 cards left in 2P game).
    *   **Special Draw:**
        *   Winner takes the **Face-Down** card.
        *   Loser takes the **Face-Up Trump Card**.
    *   **State Change:**
        *   Set `CurrentPhase = Phase2_Last9`.
        *   **Return all `TableCards` to `Hand`.** (Table is cleared).
        *   Transition to `L9Play`.

### 4.5 State: L9Play (Phase 2)
**Transitions:** `L9PlayFirst` ... -> `L9NewTrick`

**Strict Validation Algorithm:**
1.  **Input:** Player attempts to play Card `C`.
2.  **Check:**
    *   **Rule A (Follow Suit):** If Hand contains `LeadSuit`, `C` MUST be `LeadSuit`.
        *   **Rule A.1 (Win):** If Hand contains `LeadSuit` cards higher than the current winning card, `C` MUST be one of those.
    *   **Rule B (Trump):** If Hand has NO `LeadSuit`, but has `Trump`, `C` MUST be `Trump`.
        *   **Rule B.1 (Over-Trump):** If trick is currently won by a Trump, `C` MUST be a higher Trump (if available).
    *   **Rule C (Any):** Only if A and B are impossible, play any `C`.

### 4.6 State: L9NewTrick (Last Trick Bonus)
1.  **Logic:** Determine winner normally.
2.  **Bonus:**
    *   If `Hand.Count == 0` (The very last trick of the game):
        *   Check Winner's played card.
        *   If `Rank == 7` and `Suit == Trump`: Award **20 Points**.
        *   Else: Award **10 Points**.
3.  **End:** If Hands are empty -> `RoundEnd`.

### 4.7 State: RoundEnd & CalculatePoint
**Transitions:** `CalculatePoint` -> (`Deal` or `GameOver`)

**Logic:**
1.  **Standard Mode:**
    *   Do nothing. `TotalScore` += `RoundScore`.
2.  **Advanced Mode (Ace/10 Count):**
    *   Iterate `Player.WonPile`.
    *   `Count` = Num(Aces) + Num(10s).
    *   `RawPoints` = `Count` * 10.
    *   **Threshold Check:**
        *   **2 Players:** If `Count >= 14`, add `RawPoints`. Else +0.
        *   **4 Players:** If `Count >= 8`, add `RawPoints`. Else +0.
    *   `TotalScore` += `RoundScore` + `AdvancedBonus`.

3.  **Win Check:**
    *   If `TotalScore >= GameConfig.TargetScore`: End Game.
    *   Else: Reset Deck, Hands, Table, WonPiles. Start `Deal`.

---

## 5. Developer Implementation Checklist (Step-by-Step)

Follow this path to ensure "Competitive Programming" style logical isolation.

1.  **Step 1: The Config & Deck:**
    *   Implement `GameConfig` and `Card` classes.
    *   Implement `DeckFactory` that generates the 132 cards correctly.
    *   Unit Test: Ensure 4 Aces of Spades exist with unique IDs.

2.  **Step 2: The Comparator (Engine):**
    *   Write `TrickEvaluator.GetWinner(List<Card> played, Suit trump)`.
    *   Unit Test: Joker vs Trump, Joker vs Non-Trump, 10 vs King.

3.  **Step 3: Phase 1 Logic:**
    *   Implement the Play loop.
    *   Implement `MeldValidator`. Ensure specific `CardID`s are tracked so they aren't reused for the exact same meld type.

4.  **Step 4: The Transition:**
    *   Mock a game state with 2 cards in deck.
    *   Call `Draw()`. Verify Winner gets hidden card, Loser gets Trump card. Verify `Phase` switches to `Last9`.

5.  **Step 5: Phase 2 Validator:**
    *   Write `LegalMoveGenerator(Hand, LeadCard, Trump)`.
    *   Unit Test: Give player a Trump and Lead Suit. Try to play Off-suit (Should Fail). Try to play Lower Lead Suit when Higher exists (Should Fail).

6.  **Step 6: Scoring & Modes:**
    *   Implement `ScoreCalculator`.
    *   Unit Test: `AdvancedMode`. Mock a `WonPile` with 13 Aces/10s in 2P mode. Verify Bonus is 0. Mock with 14. Verify Bonus is 140.

7.  **Step 7: FSM Wrapper:**
    *   Wrap all above in a class `BeziqueGameController`.
    *   Expose public methods: `PlayCard(playerId, cardId)`, `DeclareMeld(...)`.
    *   Expose events: `OnTrickEnd`, `OnRoundEnd`.