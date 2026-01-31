# Bezique AI Bot System

## Overview

The BeziqueCore SDK includes a unified AI bot system for single-player offline gameplay. The bot implements maximum difficulty strategies, combining optimal card play with intelligent meld decisions.

## Architecture

### Core Interface

```csharp
public interface IBeziqueBot
{
    Card SelectCardToPlay(Player bot, Dictionary<Player, Card> currentTrick, Suit trumpSuit, bool isLastNineCardsPhase);
    Meld? DecideMeld(Player bot, Suit trumpSuit);
    string BotName { get; }
}
```

### Unified Bot

```csharp
public class BeziqueBot : IBeziqueBot
{
    public string BotName => "AI";

    public Card SelectCardToPlay(...);
    public Meld? DecideMeld(...);
}
```

## Bot Strategy

### Leading Strategy

When leading a trick, the AI uses intelligent card selection:

**Trump Lead:**
- Leads trump when holding 3+ trump cards
- Uses low trump to force opponent to waste high trumps

**Suit Lead:**
- Analyzes all suits to find strongest holding
- Leads highest card from strongest suit
- Avoids leading from weak suits

**Card Power Calculation:**
```csharp
protected int GetCardPower(Card card, Suit trumpSuit)
{
    if (card.IsJoker) return 100;
    if (card.Suit == trumpSuit) return 50 + GetRankValue(card.Rank);
    return GetRankValue(card.Rank);
}
```

### Following Strategy

When following in a trick:

**Winning Situation:**
- Calculates if it can win the trick
- Uses lowest card that wins (saves high cards)
- Efficient card management

**Can't Win Situation:**
- Dumps 7 of trump (to get +10 bonus)
- Dumps lowest non-point cards (avoids Aces/Tens)
- Strategic card disposal

**Example:**
```csharp
if (winningCards.Any())
{
    // Win with lowest card that wins
    return winningCards.OrderBy(c => GetCardPower(c, trumpSuit)).First();
}

// Can't win - dump strategically
var sevenOfTrump = validCards.FirstOrDefault(c => c.Rank == Rank.Seven && c.Suit == trumpSuit);
if (sevenOfTrump != null) return sevenOfTrump;
```

### Meld Strategy

**Always Declares:**
- Finds all possible melds using `MeldHelper.FindAllPossibleMelds()`
- Returns highest point meld (sorted by points descending)
- No hesitation - optimal meld declaration

**Meld Tracking:**
- Respects melded cards tracking
- Can reuse melded cards if at least one new card is included
- Follows Bezique rules correctly

## Integration in GameController

```csharp
// Create unified AI bot
var bot = new Player
{
    Name = "AI",
    // ... other properties
};

// Map bot to AI
_bots[bot] = new BeziqueBot();

// Bot plays card during its turn
var cardToPlay = bot.SelectCardToPlay(
    botPlayer,
    _gameState.CurrentTrick,
    _gameState.TrumpSuit,
    isLastNineCardsPhase
);

_playerActions.PlayCard(botPlayer, cardToPlay);

// Bot decides on meld
var meld = bot.DecideMeld(botPlayer, _gameState.TrumpSuit);
if (meld != null)
{
    _playerActions.DeclareMeld(botPlayer, meld);
}
```

## Key Features

### 1. Rule Compliance

The AI respects all game rules:
- **Last 9 Cards Strict Rules:** Must follow suit with higher card if possible
- **Valid Card Detection:** Only plays valid cards from hand
- **Meld Rules:** Properly handles melded card tracking

### 2. Valid Card Detection

```csharp
private List<Card> GetValidCards(Player bot, Dictionary<Player, Card> currentTrick,
                                   Suit trumpSuit, bool isLastNineCardsPhase)
{
    if (!isLastNineCardsPhase)
        return bot.Hand.ToList();

    // Last 9 cards strict rules apply
    // 1. Must follow suit with higher card if possible
    // 2. Otherwise play lower card of same suit
    // 3. Otherwise any card is valid
}
```

### 3. Strategic Thinking

**Trump Management:**
- Counts trumps before leading
- Forces opponent to waste trumps
- Preserves high trumps for critical moments

**Suit Control:**
- Identifies strongest suits
- Leads from strength when possible
- Avoids leading from weak suits

**Point Optimization:**
- Saves high point cards (Aces, Tens)
- Dumps low value cards when can't win
- Maximizes trick points

### 4. Meld Decision Making

The AI uses the SDK's `MeldHelper` to:
- Find all possible melds from current hand
- Select highest-point meld
- Always declare when beneficial

```csharp
public Meld? DecideMeld(Player bot, Suit trumpSuit)
{
    var possibleMelds = MeldHelper.FindAllPossibleMelds(bot, trumpSuit);
    if (possibleMelds.Any())
    {
        return possibleMelds.First(); // Already sorted by points descending
    }
    return null;
}
```

## Usage Example

```csharp
// In single-player mode
var humanPlayer = new Player { Name = "Alice" };
var botPlayer = new Player { Name = "AI" };

var bot = new BeziqueBot();
_bots[botPlayer] = bot;

// Bot automatically plays during its turn
if (_bots.ContainsKey(currentPlayer))
{
    HandleBotTurn(currentPlayer);
}
```

## Creating Custom Bots

You can extend the bot system by implementing `IBeziqueBot`:

```csharp
public class CustomBot : IBeziqueBot
{
    public string BotName => "My Custom AI";

    public Card SelectCardToPlay(
        Player bot,
        Dictionary<Player, Card> currentTrick,
        Suit trumpSuit,
        bool isLastNineCardsPhase)
    {
        // Your custom logic here
        return validCards.First();
    }

    public Meld? DecideMeld(Player bot, Suit trumpSuit)
    {
        // Your meld strategy here
        return null;
    }
}

// Use in GameController
_bots[botPlayer] = new CustomBot();
```

## Technical Notes

- **Fair Play:** Bot has same information as human players (no cheating)
- **SDK Integration:** Uses same SDK interfaces as human players
- **Realistic Pacing:** Bot delay simulated with `Thread.Sleep()`
- **No Base Class:** Direct implementation of `IBeziqueBot` interface

## AI Strengths

| Aspect | Strategy |
|--------|----------|
| **Card Play** | Optimal - considers trump, suits, and game state |
| **Meld Frequency** | Always declares when beneficial |
| **Trick Winning** | Efficient - uses lowest winning card |
| **Card Dumping** | Strategic - prioritizes bonus cards and low values |
| **Last 9 Cards** | Fully compliant with strict rules |
| **Trump Management** | Advanced - forces opponent waste |

## Testing

Test the AI by running the CLI:

```bash
dotnet run --project BeziqueGame.CLI
```

Select "Single Player (vs AI)" to play against the unified AI bot.

## Future Enhancements

Potential improvements for the AI:

- **Card Memory:** Track played cards to make better informed decisions
- **Probability Calculations:** Calculate odds of opponent holding specific cards
- **Adaptive Strategy:** Adjust play style based on score difference
- **Pattern Recognition:** Detect opponent playing patterns
- **Endgame Optimization:** Specialized strategies for close games
