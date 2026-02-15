namespace BeziqueCore.Interfaces;


public interface IBeziqueAdapter
{
    void DealThreeCards();
    void FlipCard();
    void Add10PointsToDealer();
    DeckCheckResult CheckDeckCardCount();
    DrawResult DrawCardFromDeck();
    void AddMeldPoint();
    void SetPendingMeld(MeldType meldType);
    void DetermineWinner();
    HandCheckResult CheckCardsOnHand();
    bool IsTrump7();
    void AllPlayerPlayed();
    void PlayCard(int playerIndex, byte cardId);
}
