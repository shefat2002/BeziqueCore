namespace BeziqueCore;

public enum DeckCheckResult
{
    Empty,
    AvailableCards
}

public enum DrawResult
{
    MoreToDraw,
    DrawComplete
}

public enum HandCheckResult
{
    Empty,
    MoreCards
}

public enum RoundEndResult
{
    WinningScore,
    WinningScoreNotReached
}

public enum TrickResult
{
    PlayerTurn,
    OpponentTurn
}

public enum MeldResult
{
    Opportunity,
    NoOpportunity
}

public enum PlayResult
{
    AllPlayerPlayed,
    WaitingForOpponent
}
