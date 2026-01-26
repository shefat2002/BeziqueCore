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
        void StartPlayerTimer();
        void StopPlayerTimer();
        void ResetPlayerTimer();
        void DeductTimeoutPoints();
        void CalculateRoundScores();
        void NotifyRoundEnded();
        void DeclareWinner();
        void NotifyGameOver();
        bool IsLastNineCardsPhase();
    }
}
