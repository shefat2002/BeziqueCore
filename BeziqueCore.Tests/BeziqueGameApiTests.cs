using BeziqueCore.Interfaces;

namespace BeziqueCore.Tests;

public class BeziqueGameApiTests
{
    #region Game Creation Tests

    public class TestBeziqueAdapter : IBeziqueAdapter
    {
        public void InitializeGame()
        {
            
        }

        public void NotifyGameInitialized()
        {
            throw new NotImplementedException();
        }

        public void DealCards()
        {
            throw new NotImplementedException();
        }

        public void NotifyCardsDealt()
        {
            throw new NotImplementedException();
        }

        public void FlipTrumpCard()
        {
            throw new NotImplementedException();
        }

        public void NotifyTrumpDetermined()
        {
            throw new NotImplementedException();
        }

        public void StartPlayerTimer()
        {
            throw new NotImplementedException();
        }

        public void StopPlayerTimer()
        {
            throw new NotImplementedException();
        }

        public void ResetPlayerTimer()
        {
            throw new NotImplementedException();
        }

        public void DeductTimeoutPoints()
        {
            throw new NotImplementedException();
        }

        public void ProcessOpponentResponses()
        {
            throw new NotImplementedException();
        }

        public void ResolveTrick()
        {
            throw new NotImplementedException();
        }

        public void ProcessMeldOpportunity()
        {
            throw new NotImplementedException();
        }

        public void ScoreMeld()
        {
            throw new NotImplementedException();
        }

        public void DrawCards()
        {
            throw new NotImplementedException();
        }

        public void CheckDeck()
        {
            throw new NotImplementedException();
        }

        public void ProcessL9OpponentResponses()
        {
            throw new NotImplementedException();
        }

        public void ResolveL9Trick()
        {
            throw new NotImplementedException();
        }

        public void CheckL9TrickComplete()
        {
            throw new NotImplementedException();
        }

        public void CalculateL9FinalScores()
        {
            throw new NotImplementedException();
        }

        public void CalculateRoundScores()
        {
            throw new NotImplementedException();
        }

        public void CalculateAcesAndTens()
        {
            throw new NotImplementedException();
        }

        public void NotifyRoundEnded()
        {
            throw new NotImplementedException();
        }

        public void DeclareWinner()
        {
            throw new NotImplementedException();
        }

        public void NotifyGameOver()
        {
            throw new NotImplementedException();
        }

        public bool IsLastNineCardsPhase()
        {
            throw new NotImplementedException();
        }

        public bool IsDeckEmpty()
        {
            throw new NotImplementedException();
        }

        public bool AreAllHandsEmpty()
        {
            throw new NotImplementedException();
        }

        public bool HasPlayerReachedWinningScore()
        {
            throw new NotImplementedException();
        }

        public bool IsTrickComplete()
        {
            throw new NotImplementedException();
        }

        public bool MorePlayersNeedToPlay()
        {
            throw new NotImplementedException();
        }
    }
    
    [Fact]
    public void CreateSinglePlayer_WithValidNames_CreatesGameWithTwoPlayers()
    {
        var beziqueGameState = new BeziqueGameState();
        beziqueGameState.DispatchEvent();
    }
    

    #endregion
}
