namespace BeziqueCore.Core.Application.DTOs;

public record GameSnapshotDto
{
    public required string StateName { get; init; }
    public required PlayerStateDto[] Players { get; init; }
    public required string TrumpSuit { get; init; }
    public required CardDto? TrumpCard { get; init; }
    public required TrickStateDto CurrentTrick { get; init; }
    public required string CurrentPlayerUserId { get; init; }
    public required string DealerUserId { get; init; }
    public required int DeckCardCount { get; init; }
    public required bool IsLastNineCardsPhase { get; init; }
    public string? LeadSuit { get; init; }
    public Dictionary<string, int> RoundScores { get; init; } = new();
    public required string GameMode { get; init; }
    public string? WinnerUserId { get; init; }
}

public record PlayerStateDto
{
    public required string UserId { get; init; }
    public required string Name { get; init; }
    public required int Score { get; init; }
    public required bool IsDealer { get; init; }
    public required bool IsBot { get; init; }
    public required int HandCardCount { get; init; }
    public CardDto[]? Hand { get; init; }
    public required MeldDto[] DeclaredMelds { get; init; }
    public required int MeldedCardCount { get; init; }
}

public record CardDto
{
    public required string Suit { get; init; }
    public required string Rank { get; init; }
    public required bool IsJoker { get; init; }
}

public record MeldDto
{
    public required string MeldType { get; init; }
    public required CardDto[] Cards { get; init; }
    public required int Points { get; init; }
}

public record TrickStateDto
{
    public required Dictionary<string, CardDto> PlayedCards { get; init; }
    public required int CardsPlayedCount { get; init; }
    public required bool IsComplete { get; init; }
}
