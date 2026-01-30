namespace BeziqueCore.Interfaces
{
    public interface IGameAdapter
    {
        void InitializeGame();
        void NotifyGameInitialized();
        void DealCards();
        void NotifyCardsDealt();
        void FlipTrumpCard();
        void NotifyTrumpDetermined();

        // Player turn actions
        void StartPlayerTimer();
        void StopPlayerTimer();
        void ResetPlayerTimer();
        void DeductTimeoutPoints();

        // Gameplay actions
        void ProcessOpponentResponses();
        void ResolveTrick();
        void ProcessMeldOpportunity();
        void ScoreMeld();
        void DrawCards();
        void CheckDeck();

        // Last 9 cards actions
        void ProcessL9OpponentResponses();
        void ResolveL9Trick();
        void CheckL9TrickComplete();
        void CalculateL9FinalScores();

        // Round and game end
        void CalculateRoundScores();
        void CalculateAcesAndTens();
        void NotifyRoundEnded();
        void DeclareWinner();
        void NotifyGameOver();

        bool IsLastNineCardsPhase();
        bool IsDeckEmpty();
        bool AreAllHandsEmpty();
        bool HasPlayerReachedWinningScore();

        // Turn-based gameplay helpers
        bool IsTrickComplete();
        bool MorePlayersNeedToPlay();
    }
}
