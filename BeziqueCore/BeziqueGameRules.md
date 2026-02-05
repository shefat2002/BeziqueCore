Here is the comprehensive list of **Rules, Exceptions, and Edge Cases** for the Bezique Card Game SDK. This list acts as the validation checklist for your `RulesEngine` and `ScoringEngine`.

---

### **1. Deck & Setup Rules**
1.  **Composition:** 4 Standard Decks + 4 Jokers.
2.  **Filtering:** Remove 2, 3, 4, 5, 6. Keep 7, 8, 9, 10, J, Q, K, A, Joker.
3.  **Total Cards:** 132.
4.  **Rank Hierarchy (High to Low):**
    *   Ace (14)
    *   **Ten (13)** — *Exception: Beats King, Queen, Jack.*
    *   King (12)
    *   Queen (11)
    *   Jack (10)
    *   Nine (9)
    *   Eight (8)
    *   Seven (7)
5.  **Dealing:** 9 Cards per player.
6.  **Trump Selection:** First card of remaining deck is flipped.
7.  **Dealer Bonus:** If the flipped Trump card is Rank 7, Dealer gets **+10 Points** immediately.

---

### **2. Trick Resolution Logic (Comparator)**
1.  **Lead Suit:** Defined by the first card played in the trick.
2.  **Standard Win Condition:**
    *   If Trumps are played: Highest Rank Trump wins.
    *   If no Trumps: Highest Rank of the **Lead Suit** wins.
    *   Off-suit (non-Trump) cards never win against Lead Suit.
3.  **Tie-Breaking:**
    *   If two identical cards tie for winning position (e.g., Player 1 plays Ace-Spades, Player 2 plays Ace-Spades): The **first played** card wins.
4.  **Joker Rules (Edge Cases):**
    *   **Lead Joker:** Joker acts as the highest non-Trump card. It wins the trick **UNLESS** an opponent plays a Trump card.
    *   **Follow Joker:** Joker has a value of 0. It cannot win the trick.
    *   *Note:* Jokers have no suit (or `Suit.None`), so they never trigger "Follow Suit" logic for opponents.

---

### **3. Phase 1 Rules (Normal Play)**
1.  **Validation:** Players can play **ANY** card from their Hand or TableCards.
2.  **Draw Order:** Trick Winner draws top card. Loser(s) draw next cards.
3.  **Trump 7 Swap:**
    *   **Who:** Trick Winner only.
    *   **Condition:** Holds Rank 7 of Trump Suit + Game Trump Card is NOT Rank 7.
    *   **Action:** Swap Hand Card with Table Trump Card.
    *   **Score:** +10 Points.
4.  **Playing a 7:**
    *   Any player who plays a Rank 7 of Trump into a trick gets **+10 Points**.

---

### **4. Melding Rules (Complex Logic)**
**Constraint:** Only the **Trick Winner** can meld, immediately before drawing new cards.

#### **4.1 Meld Combinations**
*   **Trump Run (250):** A, 10, K, Q, J (Trump Suit).
*   **Trump Marriage (40):** K, Q (Trump Suit).
*   **Non-Trump Marriage (20):** K, Q (Same Non-Trump Suit).
*   **Bezique (40):** Queen Spades + Jack Diamonds.
*   **Double Bezique (500):** 2x Queen Spades + 2x Jack Diamonds.
*   **Four of a Kind:**
    *   Aces (100), Kings (80), Queens (60), Jacks (40).

#### **4.2 Meld Exceptions & Constraints**
1.  **Double Bezique Strict Rule:** You must lay down all 4 cards **at once**.
    *   *Edge Case:* If you meld a Single Bezique (40), and later get the other pair, melding the second pair counts as another Single Bezique (40). Total 80, not 500.
2.  **Joker Substitution:**
    *   A joker can substitute **ONLY** in Four-of-a-Kind (Aces, Kings, Queens, Jacks).
    *   Jokers **CANNOT** be used in Runs, Marriages, or Bezique.
3.  **Card Reusability (The "ID" Check):**
    *   A specific card (by UniqueID) cannot be used for the **same MeldType** twice.
    *   *Allowed:* King-Hearts used in "4 Kings" can later be used in "Marriage".
    *   *Allowed:* King-Hearts used in "Marriage" can later be used in "Trump Run".
    *   *Forbidden:* King-Hearts used in "4 Kings" cannot be used in another "4 Kings" set.
4.  **Sequence Logic:**
    *   If you meld `Trump Marriage` (K, Q), you can later add (A, 10, J) to upgrade it to a `Trump Run` (+250).
    *   If you meld `Trump Run` (All 5), you get 250. You cannot claim the Marriage points separately afterwards using those same cards.

---

### **5. The Phase Transition (Crucial Edge Case)**
**Trigger:** `DrawDeck.Count` equals `PlayerCount` (e.g., 2 cards left in 2-player game).

1.  **The Draw:**
    *   **Winner** gets the hidden top card.
    *   **Loser** gets the visible Trump Card.
2.  **State Change:**
    *   **Melds Disabled:** No more melding allowed.
    *   **Table Clearing:** All `TableCards` (active melds) are logically returned to the `Hand`.
    *   **Phase Flag:** Switch to `Phase2_Last9`.

---

### **6. Phase 2 Rules (Last 9 Cards)**
**Constraint:** Melding is Disabled. Strict playing rules apply.

#### **6.1 Strict Validation Logic (Priority Order)**
1.  **Rule A (Follow Suit):** If Player has cards of the **Lead Suit**, they **MUST** play one.
    *   **Sub-Rule (Must Win):** If they have cards of Lead Suit that are **Higher Rank** than the current winning card, they **MUST** play a winner.
2.  **Rule B (Trump):** If Player has **NO** Lead Suit, but has **Trump**, they **MUST** play a Trump.
    *   **Sub-Rule (Over-Trump):** If the trick is already being won by a Trump, and the player must play a Trump, they **MUST** play a higher Trump if available.
3.  **Rule C (Any):** If neither A nor B applies, play any card.

#### **6.2 Phase 2 Bonus**
*   **Last Trick:** The winner of the 9th (final) trick gets **+10 Points**.
*   **Exception:** If the winning card of the last trick is **Rank 7 of Trump**, award **+20 Points** instead.

---

### **7. End Game Scoring (Mode Specific)**
**Trigger:** All 9 tricks of Phase 2 are complete.

#### **7.1 Standard Mode**
*   No additional scoring. `Total Score = Accumulated In-Game Points`.

#### **7.2 Advanced Mode (Ace/Ten Count)**
*   **Logic:** Iterate through the `WonPile` of the player.
*   **Value:** Each Ace = 10pts, Each Ten = 10pts.
*   **Thresholds (The "All or Nothing" Rule):**
    *   **2 Players:** Need **14 or more** Aces/Tens to score. (<14 = 0 pts).
    *   **4 Players:** Need **8 or more** Aces/Tens to score. (<8 = 0 pts).

---

### **8. Technical Edge Cases (Developer Checklist)**
1.  **Card Identity:**
    *   There are 4 copies of every card (e.g., 4 x King of Hearts).
    *   Logic must distinguish them by `DeckIndex` or `UniqueID` to prevent re-using the "same physical card" logic improperly.
2.  **Empty Hand Check:**
    *   Game ends strictly when hands are empty in Phase 2.
3.  **Swap 7 with Joker?**
    *   No. Swap 7 only applies to the Trump Card. (Joker rules do not apply to the Swap mechanics).
4.  **Melding with Table Cards:**
    *   In Phase 1, cards currently in a Meld on the table can be played into a trick.
    *   *Edge Case:* If a player plays a Melded Card into a trick, it is removed from `TableCards`. If that breaks a Meld (e.g., removing K from a Marriage), the points remain scored, but the K is gone.
5.  **Score Limits:**
    *   Game ends **instantly** when `TargetScore` (e.g., 1500) is reached. Even mid-trick or mid-meld.