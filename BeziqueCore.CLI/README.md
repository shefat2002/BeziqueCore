# Bezique Card Game CLI

A user-friendly command-line interface for the Bezique card game.

## Features

- **Interactive Menu System**: Easy-to-navigate menus with Spectre.Console styling
- **Visual Game State**: Real-time display of cards, scores, and game state
- **2-4 Player Support**: Configure the number of players
- **Complete Game Flow**: Full implementation from deal to game over
- **Card Display**: Beautiful Unicode card symbols (♥♦♣♠)
- **Test Mode**: Run automated tests with `dotnet run -- test`

## Running the Game

### Normal Mode
```bash
cd BeziqueCore.CLI
dotnet run
```

### Test Mode
```bash
dotnet run -- test
```

## Game Menu Options

### Main Menu
- **Start New Game** - Begin a new game with 2 or 4 players
- **Show Rules** - Display complete game rules
- **Exit** - Exit the application

### In-Game Actions
- **View Hands** - See all players' cards
- **Game Status** - Detailed game state information
- **Play Card** - Select a card from your hand to play
- **Declare Meld** - Declare a meld (Trump Run, Marriage, Bezique, etc.)
- **Skip Meld** - Pass on melding opportunity
- **Continue Playing** - Proceed to next trick
- **Last 9 Cards Reached** - Transition to endgame phase
- **Continue Game** - Start a new round
- **Game Over** - End the game and declare winner

## How to Play

1. **Setup**: Choose 2 or 4 players and enter names
2. **Card Play**: When it's your turn, select a card number to play
3. **Melds**: After winning a trick, you can declare a meld or skip
4. **Last 9 Cards**: When deck runs low, strict rules apply
5. **Winning**: First player to reach the winning score wins!

## Card Display

Cards are shown with suit symbols and ranks:
- ♥ Hearts, ♦ Diamonds, ♣ Clubs, ♠ Spades
- Ranks: A, K, Q, J, 10, 9, 8, 7
- 🃏 Joker (special card with unique rules)

## State Machine

The CLI uses the StateSmith-generated state machine with these states:
- GAME_INIT → DEALING → TRUMP_SELECTION → GAMEPLAY → LAST_9_CARDS → ROUND_END → GAME_OVER
