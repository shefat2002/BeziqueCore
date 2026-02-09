namespace BeziqueCore.Interfaces;


public interface IBeziqueAdapter
{
    void DealThreeCards();
    void FlipCard();
    void Add10PointsToDealer();
    void CheckDeckCardCount();
    void DrawCardFromDeck();
    void AddMeldPoint();
    void AllPlayerPlayed();
    void DetermineWinner();
    void CheckCardsOnHand();
}
