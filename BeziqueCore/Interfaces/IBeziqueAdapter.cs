namespace BeziqueCore.Interfaces;

/// <summary>
/// Adapter interface for Bezique state machine behaviors.
/// This interface decouples the state machine from the game logic.
/// </summary>
public interface IBeziqueAdapter
{
    // Deal Phase Methods
    void DealFirstCard();
    void DealMidCard();
    void DealLastCard();
    void SelectTrump();

    // Play Phase Methods
    void PlayFirstCard();
    void PlayMidCard();
    void PlayLastCard();

    // Meld Phase Methods
    void TryMeld();
    void MeldSuccess();
    void MeldFailed();
    void AddMeldPoints();

    // Trick Transition Methods
    void StartNewTrick();
    void DealCardsToAll();

    // L9 Play Phase Methods
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
