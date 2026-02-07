namespace BeziqueCore.Interfaces;


public interface IBeziqueAdapter
{
    void DealFirstSet();
    void DealMidSet();
    void DealLastSet();
    void SelectTrump();

    // Play Phase Methods
    void PlayFirstCard();
    void PlayMidCard();
    void PlayLastCard();

    // Meld Phase Methods
    void TryMeld();
    void MeldSuccess();
    void MeldFailed();

    // Trick Transition Methods
    void StartNewTrick();
    void DrawCardsForAll();

    // L9 Play Phase Methods (Last 9 cards, no drawing)
    void L9PlayFirstCard();
    void L9PlayMidCard();
    void L9PlayLastCard();
    void StartL9NewTrick();

    // Round End Methods
    void EndRound();
    void CalculatePoints();

    // Game End Method
    void GameOver();
}
