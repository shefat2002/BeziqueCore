using BeziqueCore.Models;
using BeziqueCore.Interfaces;
using BeziqueCore.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BeziqueGame.CLI
{
    /// <summary>
    /// Simplified game controller for local multiplayer Bezique.
    /// Supports 2-player (vs AI) and 4-player local games.
    /// </summary>
    public class GameController
    {
        private const int WinningScore = 1000;
        private readonly List<Player> _players = new();
        private readonly List<Card> _deck = new();
        private Suit _trumpSuit;
        private int _currentPlayerIndex;
        private readonly Dictionary<Player, Card> _currentTrick = new();
        private GameMode _gameMode;
        private readonly Random _random = new();
        private bool _gameOver = false;

        public void StartTwoPlayerGame(string playerName, GameMode mode)
        {
            _gameMode = mode;
            _players.Clear();

            // Human player
            _players.Add(new Player
            {
                Id = "1",
                Name = playerName,
                Score = 0,
                Hand = new List<Card>(),
                IsDealer = true,
                IsBot = false,
                DeclaredMelds = new List<Meld>(),
                MeldedCards = new List<Card>()
            });

            // AI opponent
            _players.Add(new Player
            {
                Id = "2",
                Name = "AI Opponent",
                Score = 0,
                Hand = new List<Card>(),
                IsDealer = false,
                IsBot = true,
                DeclaredMelds = new List<Meld>(),
                MeldedCards = new List<Card>()
            });

            StartGame();
        }

        public void StartFourPlayerGame(GameMode mode)
        {
            _gameMode = mode;
            _players.Clear();

            // 4 human players
            for (int i = 0; i < 4; i++)
            {
                _players.Add(new Player
                {
                    Id = (i + 1).ToString(),
                    Name = $"Player {i + 1}",
                    Score = 0,
                    Hand = new List<Card>(),
                    IsDealer = i == 0,
                    IsBot = false,
                    DeclaredMelds = new List<Meld>(),
                    MeldedCards = new List<Card>()
                });
            }

            StartGame();
        }

        private void StartGame()
        {
            Console.Clear();
            Console.WriteLine("=== BEZIQUE CARD GAME ===\n");
            Console.WriteLine($"Mode: {_gameMode}");
            Console.WriteLine($"Players: {_players.Count}\n");

            InitializeDeck();
            SelectTrump();
            DealCards();

            _currentPlayerIndex = 0;

            // Game loop - continues until hands are empty OR someone wins
            while (AllHandsHaveCards() && !_gameOver)
            {
                PlayRound();
            }

            EndGame();
        }

        private void InitializeDeck()
        {
            _deck.Clear();

            // Create 4 decks with jokers
            for (int d = 0; d < 4; d++)
            {
                foreach (Suit suit in Enum.GetValues<Suit>())
                {
                    foreach (Rank rank in Enum.GetValues<Rank>())
                    {
                        _deck.Add(Card.Create(suit, rank));
                    }
                }
                _deck.Add(Card.CreateJoker());
            }

            // Shuffle
            for (int i = _deck.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (_deck[i], _deck[j]) = (_deck[j], _deck[i]);
            }

            Console.WriteLine("Deck initialized and shuffled.\n");
        }

        private void SelectTrump()
        {
            if (_deck.Count > 0)
            {
                var trumpCard = _deck[0];
                _deck.RemoveAt(0);
                _trumpSuit = trumpCard.Suit;

                Console.WriteLine($"Trump: {_trumpSuit} ({trumpCard.Rank})\n");
            }
        }

        private void DealCards()
        {
            Console.WriteLine("Dealing cards...\n");

            int cardsPerPlayer = 9;
            for (int i = 0; i < cardsPerPlayer; i++)
            {
                foreach (var player in _players)
                {
                    if (_deck.Count > 0)
                    {
                        var card = _deck[0];
                        _deck.RemoveAt(0);
                        player.Hand.Add(card);
                    }
                }
            }

            // Sort hands
            foreach (var player in _players)
            {
                player.Hand.Sort();
            }

            Console.WriteLine("Cards dealt.\n");
        }

        private bool AllHandsHaveCards()
        {
            return _players.All(p => p.Hand.Count > 0);
        }

        private void PlayRound()
        {
            // Each player plays a card
            for (int i = 0; i < _players.Count; i++)
            {
                var currentPlayer = _players[_currentPlayerIndex];
                ShowGameState();

                if (currentPlayer.IsBot)
                {
                    PlayBotTurn(currentPlayer);
                }
                else
                {
                    PlayHumanTurn(currentPlayer);
                }

                _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey(true);
            }

            // Resolve trick
            ResolveTrick();

            // Draw cards
            if (_deck.Count > 0)
            {
                Console.WriteLine("\nDrawing cards...\n");
                foreach (var player in _players)
                {
                    if (_deck.Count > 0)
                    {
                        var card = _deck[0];
                        _deck.RemoveAt(0);
                        player.Hand.Add(card);
                        player.Hand.Sort();
                    }
                }
            }
        }

        private void ShowGameState()
        {
            Console.Clear();
            Console.WriteLine($"=== BEZIQUE - {_gameMode} Mode ===");
            Console.WriteLine($"Trump: {_trumpSuit}\n");

            // Show scores
            Console.WriteLine("Scores:");
            foreach (var player in _players)
            {
                string indicator = player == _players[_currentPlayerIndex] ? " <<<" : "";
                Console.WriteLine($"  {player.Name}: {player.Score}{indicator}");
            }

            Console.WriteLine();

            // Show current trick
            if (_currentTrick.Count > 0)
            {
                Console.WriteLine("Current Trick:");
                foreach (var kvp in _currentTrick)
                {
                    Console.WriteLine($"  {kvp.Key.Name}: {kvp.Value.Rank} of {kvp.Value.Suit}");
                }
                Console.WriteLine();
            }

            // Show current player's hand
            var currentPlayer = _players[_currentPlayerIndex];
            Console.WriteLine($"{currentPlayer.Name}'s Hand:");
            for (int i = 0; i < currentPlayer.Hand.Count; i++)
            {
                var card = currentPlayer.Hand[i];
                string meldedIndicator = currentPlayer.MeldedCards.Any(mc => mc.Equals(card)) ? " [M]" : "";
                Console.WriteLine($"  {i + 1}. {card.Rank} of {card.Suit}{meldedIndicator}");
            }
        }

        private void PlayBotTurn(Player bot)
        {
            Console.WriteLine($"\n{bot.Name} is thinking...");
            Thread.Sleep(1000);

            // Simple AI: play highest non-trump card, or lowest trump if leading
            Card cardToPlay;
            if (_currentTrick.Count == 0)
            {
                // Leading - play lowest non-trump
                cardToPlay = bot.Hand.Where(c => c.Suit != _trumpSuit)
                    .OrderBy(c => c.Rank)
                    .FirstOrDefault() ?? bot.Hand[0];
            }
            else
            {
                // Following - try to win
                var leadCard = _currentTrick.Values.First();
                var winningCards = bot.Hand.Where(c =>
                    (c.Suit == leadCard.Suit && c.Rank > leadCard.Rank) ||
                    (c.Suit == _trumpSuit && leadCard.Suit != _trumpSuit))
                    .OrderBy(c => c.Rank)
                    .ToList();

                cardToPlay = winningCards.FirstOrDefault() ?? bot.Hand[0];
            }

            Console.WriteLine($"{bot.Name} plays: {cardToPlay.Rank} of {cardToPlay.Suit}");
            bot.Hand.Remove(cardToPlay);
            _currentTrick[bot] = cardToPlay;

            // Check for meld (simplified)
            if (CanMeld(bot) && _deck.Count > 9)
            {
                var meld = GetBestMeld(bot);
                if (meld != null)
                {
                    Console.WriteLine($"{bot.Name} declares: {meld.Type} ({meld.Points} points)");
                    bot.Score += meld.Points;
                    bot.DeclaredMelds.Add(meld);
                    foreach (var card in meld.Cards)
                    {
                        if (!bot.MeldedCards.Contains(card))
                        {
                            bot.MeldedCards.Add(card);
                        }
                    }

                    // Check if bot has reached winning score - game ends immediately
                    if (bot.Score >= WinningScore)
                    {
                        _gameOver = true;
                        Console.WriteLine($"\n*** {bot.Name} has reached {bot.Score} points and wins the game! ***\n");
                    }
                }
            }
        }

        private void PlayHumanTurn(Player player)
        {
            Console.WriteLine($"\n{player.Name}'s Turn\n");

            // Select card to play
            Console.WriteLine("Choose a card to play (1-9):");
            int choice;
            while (true)
            {
                var input = Console.ReadLine();
                if (int.TryParse(input, out choice) && choice >= 1 && choice <= player.Hand.Count)
                {
                    break;
                }
                Console.WriteLine("Invalid choice. Try again:");
            }

            var cardToPlay = player.Hand[choice - 1];
            Console.WriteLine($"\nYou play: {cardToPlay.Rank} of {cardToPlay.Suit}");
            player.Hand.Remove(cardToPlay);
            _currentTrick[player] = cardToPlay;

            // Check for 7 of trump bonus
            if (cardToPlay.Rank == Rank.Seven && cardToPlay.Suit == _trumpSuit)
            {
                Console.WriteLine($"\n{player.Name} played 7 of Trump! (+10 points)");
                player.Score += 10;
            }

            // Offer meld opportunity
            if (CanMeld(player) && _deck.Count > 9)
            {
                Console.WriteLine("\nWould you like to declare a meld? (y/n)");
                if (Console.ReadLine()?.ToLower() == "y")
                {
                    ShowMeldOptions(player);
                }
            }
        }

        private bool CanMeld(Player player)
        {
            return player.Hand.Count >= 2;
        }

        private Meld GetBestMeld(Player player)
        {
            // Simplified meld detection
            var possibleMelds = new List<Meld>();

            // Check for marriages
            foreach (Suit suit in Enum.GetValues<Suit>())
            {
                var king = player.Hand.FirstOrDefault(c => c.Suit == suit && c.Rank == Rank.King);
                var queen = player.Hand.FirstOrDefault(c => c.Suit == suit && c.Rank == Rank.Queen);

                if (king != null && queen != null)
                {
                    int points = (suit == _trumpSuit) ? 40 : 20;
                    possibleMelds.Add(new Meld
                    {
                        Type = (suit == _trumpSuit) ? MeldType.TrumpMarriage : MeldType.Marriage,
                        Cards = new List<Card> { king, queen },
                        Points = points
                    });
                }
            }

            // Check for 7 of trump
            var sevenOfTrump = player.Hand.FirstOrDefault(c => c.Suit == _trumpSuit && c.Rank == Rank.Seven);
            if (sevenOfTrump != null && !player.MeldedCards.Contains(sevenOfTrump))
            {
                possibleMelds.Add(new Meld
                {
                    Type = MeldType.TrumpSeven,
                    Cards = new List<Card> { sevenOfTrump },
                    Points = 10
                });
            }

            return possibleMelds.OrderByDescending(m => m.Points).FirstOrDefault();
        }

        private void ShowMeldOptions(Player player)
        {
            var meld = GetBestMeld(player);
            if (meld != null)
            {
                Console.WriteLine($"\nAvailable: {meld.Type} - {meld.Points} points");
                Console.WriteLine("Declare this meld? (y/n)");

                if (Console.ReadLine()?.ToLower() == "y")
                {
                    player.Score += meld.Points;
                    player.DeclaredMelds.Add(meld);
                    foreach (var card in meld.Cards)
                    {
                        if (!player.MeldedCards.Contains(card))
                        {
                            player.MeldedCards.Add(card);
                        }
                    }
                    Console.WriteLine($"\nDeclared {meld.Type}! +{meld.Points} points\n");

                    // Check if player has reached winning score - game ends immediately
                    if (player.Score >= WinningScore)
                    {
                        _gameOver = true;
                        Console.WriteLine($"*** {player.Name} has reached {player.Score} points and wins the game! ***\n");
                    }
                }
            }
            else
            {
                Console.WriteLine("\nNo melds available.");
            }
        }

        private void ResolveTrick()
        {
            Console.WriteLine("\n--- Resolving Trick ---");

            Card? winningCard = null;
            Player? winner = null;

            foreach (var kvp in _currentTrick)
            {
                var card = kvp.Value;
                Console.WriteLine($"  {kvp.Key.Name}: {card.Rank} of {card.Suit}");

                if (winningCard == null)
                {
                    winningCard = card;
                    winner = kvp.Key;
                }
                else
                {
                    // Check if current card beats winning card
                    if (card.Suit == _trumpSuit && winningCard.Suit != _trumpSuit)
                    {
                        winningCard = card;
                        winner = kvp.Key;
                    }
                    else if (card.Suit == winningCard.Suit && card.Rank > winningCard.Rank)
                    {
                        winningCard = card;
                        winner = kvp.Key;
                    }
                }
            }

            if (winner != null)
            {
                // Award points for Aces and Tens in trick
                int trickPoints = 0;
                foreach (var kvp in _currentTrick)
                {
                    if (kvp.Value.Rank == Rank.Ace) trickPoints += 11;
                    if (kvp.Value.Rank == Rank.Ten) trickPoints += 10;
                }

                winner.Score += trickPoints;
                Console.WriteLine($"\n{winner.Name} wins the trick! (+{trickPoints} points)");

                // Check if winner has reached winning score - game ends immediately
                if (winner.Score >= WinningScore)
                {
                    _gameOver = true;
                    Console.WriteLine($"\n*** {winner.Name} has reached {winner.Score} points and wins the game! ***\n");
                }
            }

            _currentTrick.Clear();
        }

        private void EndGame()
        {
            Console.Clear();
            Console.WriteLine("\n========================================");
            Console.WriteLine("              GAME OVER");
            Console.WriteLine("========================================\n");

            // Advanced mode: Calculate Aces & Tens bonus
            if (_gameMode == GameMode.Advanced)
            {
                Console.WriteLine("--- Advanced Mode Bonuses ---");
                foreach (var player in _players)
                {
                    int bonusPoints = 0;
                    foreach (var meld in player.DeclaredMelds)
                    {
                        foreach (var card in meld.Cards)
                        {
                            if (card.Rank == Rank.Ace || card.Rank == Rank.Ten)
                            {
                                bonusPoints += 10;
                            }
                        }
                    }

                    if (bonusPoints > 0)
                    {
                        player.Score += bonusPoints;
                        Console.WriteLine($"{player.Name} gets Aces & Tens bonus: +{bonusPoints} points");
                    }
                }
                Console.WriteLine();
            }

            // Show final scores
            var sortedPlayers = _players.OrderByDescending(p => p.Score).ToList();
            Console.WriteLine("Final Scores:");
            for (int i = 0; i < sortedPlayers.Count; i++)
            {
                string medal = i switch
                {
                    0 => "🥇",
                    1 => "🥈",
                    2 => "🥉",
                    _ => ""
                };
                Console.WriteLine($"  {medal} {sortedPlayers[i].Name}: {sortedPlayers[i].Score} points");
            }

            Console.WriteLine($"\n{sortedPlayers[0].Name} wins!\n");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }

        public string GetRulesText()
        {
            return @"BEZIQUE GAME RULES

OBJECTIVE:
Score points by winning tricks and declaring melds.

CARD PLAY:
• Players take turns playing one card each
• Must follow suit if possible
• Trump suit beats all other suits
• Highest card of led suit (or trump) wins the trick

MELDS (declare after winning a trick):
• Marriage (K + Q of same suit): 20 points
• Trump Marriage (K + Q of trump): 40 points
• 7 of Trump: 10 points

SCORING:
• Each Ace in a trick: 11 points
• Each Ten in a trick: 10 points
• Last trick bonus: 10 points

GAME MODES:
• Standard: Traditional Bezique rules
• Advanced: Includes 10 bonus points per Ace/Ten in melds";
        }
    }
}
