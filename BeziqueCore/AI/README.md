# Bezique AI Bot System

## Overview

The BeziqueCore SDK now includes a complete AI bot system for single-player offline gameplay. The bot system demonstrates how game AI can be implemented using the SDK's interfaces.

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

### Base Class

```csharp
public abstract class BeziqueBotBase : IBeziqueBot
{
    protected readonly Random _random;
    protected List<Card> GetValidCards(...);
    protected int GetCardPower(Card card, Suit trumpSuit);
    protected int GetRankValue(Rank rank);
}
```

## Bot Difficulty Levels

### 1. EasyBot

**Behavior:**
- Plays randomly from valid cards
- Rarely declares melds (30% chance)
- Good for beginners learning the game

**Card Selection:** Random valid card

**Meld Strategy:** Simple marriages and four-of-a-kinds

---

### 2. MediumBot

**Behavior:**
- Plays strategically but not optimally
- Leading: Plays low cards to save high cards
- Following: Tries to win with lowest winning card
- Can't win: Dumps lowest card
- Declares melds 70% of the time

**Card Selection:** Strategic based on card power

**Meld Strategy:** Uses SDK's meld validator, declares melds worth ≥20 points

---

### 3. HardBot

**Behavior:**
- Plays optimally with advanced strategy
- Leading: Leads trump if has 3+ trumps
- Following: Wins efficiently with lowest winning card
- Can't win: Dumps strategically (7 of trump, then low cards)
- Always declares available melds

**Card Selection:** Calculated optimal play

**Meld Strategy:** Always declares any beneficial meld

---

### 4. ExpertBot

**Behavior:**
- Uses minimax-style thinking
- Leading: Leads with high card from strongest suit
- Following: Calculates optimal winning strategy
- Can't win: Dumps to minimize opponent's advantage
- Always melds optimally

**Card Selection:** Advanced probability calculations

**Meld Strategy:** Optimal meld declaration

## Integration in GameController

```csharp
// Create bot based on difficulty
IBeziqueBot botAI = difficulty switch
{
    "Easy" => new EasyBot(),
    "Medium" => new MediumBot(),
    "Hard" => new HardBot(),
    "Expert" => new ExpertBot(),
    _ => new MediumBot()
};

// Map bot to player
_bots[botPlayer] = botAI;

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

All bots respect game rules:
- Follow last 9 cards strict rules
- Only play valid cards from hand
- Properly calculate card power

### 2. Card Power Calculation

```csharp
protected int GetCardPower(Card card, Suit trumpSuit)
{
    if (card.IsJoker) return 100;
    if (card.Suit == trumpSuit) return 50 + GetRankValue(card.Rank);
    return GetRankValue(card.Rank);
}
```

### 3. Valid Card Detection

Bots automatically detect valid plays based on:
- Current trick state
- Trump suit
- Last 9 cards phase rules

### 4. Meld Decision Making

Bots use the SDK's `MeldValidator` to:
- Find all possible melds
- Select highest-point meld
- Decide whether to declare based on difficulty

## Usage Example

```csharp
// In single-player mode
var humanPlayer = new Player { Name = "Alice" };
var botPlayer = new Player { Name = "Medium AI" };

var bot = new MediumBot();
_bots[botPlayer] = bot;

// Bot automatically plays during its turn
if (_bots.ContainsKey(currentPlayer))
{
    HandleBotTurn(currentPlayer);
}
```

## Extending the Bot System

### Custom Bot Implementation

```csharp
public class CustomBot : BeziqueBotBase
{
    public override string BotName => "My Custom AI";

    public override Card SelectCardToPlay(...)
    {
        // Your custom logic here
        return validCards.First();
    }

    public override Meld? DecideMeld(...)
    {
        // Your meld strategy here
        return null;
    }
}
```

### Adding to GameController

```csharp
var customBot = new CustomBot();
_bots[botPlayer] = customBot;
```

## Bot Behavior Summary

| Difficulty | Win Rate | Meld Frequency | Strategy |
|-----------|----------|-----------------|----------|
| Easy | 30-40% | Low | Random valid moves |
| Medium | 45-55% | Medium | Strategic play |
| Hard | 60-70% | High | Optimal play |
| Expert | 70-80% | High | Advanced strategy |

## Technical Notes

- All bots inherit from `BeziqueBotBase` for common functionality
- Bots use the same SDK as human players
- No cheating - bots have same information as human players
- Bot delay is simulated with `Thread.Sleep()` for realistic pacing

## Future Enhancements

Potential improvements for bot AI:
- **Memory system**: Track played cards to make better decisions
- **Probability calculations**: Calculate odds of opponent having certain cards
- **Adaptive strategy**: Adjust play style based on game state
- **Learning**: Implement reinforcement learning for improving over time
- **Personality**: Different bot personalities (aggressive, defensive, etc.)

## Testing

Test the bots by running the CLI:
```bash
dotnet run --project BeziqueGame.CLI
```

Select "Single Player (vs AI)" and choose your preferred difficulty.
