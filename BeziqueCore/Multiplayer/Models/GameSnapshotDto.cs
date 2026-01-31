using BeziqueCore.Interfaces;

namespace BeziqueCore.Multiplayer.Models;

/// <summary>
/// A serializable snapshot of the current game state for network transmission.
/// Contains all necessary information for clients to render the game state.
/// </summary>
public record GameSnapshotDto
{
    /// <summary>
    /// The current state name from the state machine.
    /// </summary>
    public required string StateName { get; init; }

    /// <summary>
    /// Information about all players in the game.
    /// </summary>
    public required PlayerStateDto[] Players { get; init; }

    /// <summary>
    /// The current trump suit.
    /// </summary>
    public required string TrumpSuit { get; init; }

    /// <summary>
    /// The current trump card (face-up card).
    /// </summary>
    public required CardDto? TrumpCard { get; init; }

    /// <summary>
    /// The current trick being played.
    /// </summary>
    public required TrickStateDto CurrentTrick { get; init; }

    /// <summary>
    /// The user ID of the player whose turn it is.
    /// </summary>
    public required string CurrentPlayerUserId { get; init; }

    /// <summary>
    /// The user ID of the player who is the dealer.
    /// </summary>
    public required string DealerUserId { get; init; }

    /// <summary>
    /// The number of cards remaining in the deck.
    /// </summary>
    public required int DeckCardCount { get; init; }

    /// <summary>
    /// Whether the game is in the last 9 cards phase.
    /// </summary>
    public required bool IsLastNineCardsPhase { get; init; }

    /// <summary>
    /// The lead suit of the current trick (if any).
    /// </summary>
    public string? LeadSuit { get; init; }

    /// <summary>
    /// The round scores for all players.
    /// </summary>
    public Dictionary<string, int> RoundScores { get; init; } = new();

    /// <summary>
    /// The game mode (Standard or Advanced).
    /// </summary>
    public required string GameMode { get; init; }

    /// <summary>
    /// The user ID of the winner (if game is over).
    /// </summary>
    public string? WinnerUserId { get; init; }
}

/// <summary>
/// Represents a player's state in the game snapshot.
/// </summary>
public record PlayerStateDto
{
    /// <summary>
    /// The player's unique identifier.
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// The player's display name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The player's current score.
    /// </summary>
    public required int Score { get; init; }

    /// <summary>
    /// Whether this player is the dealer.
    /// </summary>
    public required bool IsDealer { get; init; }

    /// <summary>
    /// Whether this player is a bot (AI).
    /// </summary>
    public required bool IsBot { get; init; }

    /// <summary>
    /// The number of cards in the player's hand.
    /// </summary>
    public required int HandCardCount { get; init; }

    /// <summary>
    /// The player's hand (only included for the requesting player in multiplayer).
    /// </summary>
    public CardDto[]? Hand { get; init; }

    /// <summary>
    /// All melds declared by this player.
    /// </summary>
    public required MeldDto[] DeclaredMelds { get; init; }

    /// <summary>
    /// The number of melded cards (cards used in declared melds).
    /// </summary>
    public required int MeldedCardCount { get; init; }
}

/// <summary>
/// Represents a card in the game snapshot.
/// </summary>
public record CardDto
{
    /// <summary>
    /// The card's suit.
    /// </summary>
    public required string Suit { get; init; }

    /// <summary>
    /// The card's rank.
    /// </summary>
    public required string Rank { get; init; }

    /// <summary>
    /// Whether this card is a joker.
    /// </summary>
    public required bool IsJoker { get; init; }
}

/// <summary>
/// Represents a declared meld in the game snapshot.
/// </summary>
public record MeldDto
{
    /// <summary>
    /// The type of meld.
    /// </summary>
    public required string MeldType { get; init; }

    /// <summary>
    /// The cards in this meld.
    /// </summary>
    public required CardDto[] Cards { get; init; }

    /// <summary>
    /// The points awarded for this meld.
    /// </summary>
    public required int Points { get; init; }
}

/// <summary>
/// Represents the current trick state in the game snapshot.
/// </summary>
public record TrickStateDto
{
    /// <summary>
    /// The cards played in the current trick, keyed by player user ID.
    /// </summary>
    public required Dictionary<string, CardDto> PlayedCards { get; init; }

    /// <summary>
    /// The number of cards played so far in this trick.
    /// </summary>
    public required int CardsPlayedCount { get; init; }

    /// <summary>
    /// Whether the trick is complete (all players have played).
    /// </summary>
    public required bool IsComplete { get; init; }
}
