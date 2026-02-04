namespace BeziqueCore.Interfaces;

/// <summary>
/// Adapter interface for Bezique state machine behaviors.
/// This interface decouples the state machine from the game logic.
/// The state machine calls these methods to execute game behaviors.
///
/// Implement this interface in BeziqueGameController to handle the actual game logic.
/// </summary>
public interface IBeziqueAdapter
{
    // Deal Phase Methods
    /// <summary>Deal first set of 3 cards to each player</summary>
    void DealFirstSet();
    /// <summary>Deal second set of 3 cards to each player</summary>
    void DealMidSet();
    /// <summary>Deal third set of 3 cards to each player</summary>
    void DealLastSet();
    /// <summary>Flip the trump card and determine trump suit</summary>
    void SelectTrump();

    // Play Phase Methods
    /// <summary>First player plays a card</summary>
    void PlayFirstCard();
    /// <summary>Middle player(s) play a card (4 player game only)</summary>
    void PlayMidCard();
    /// <summary>Last player plays a card</summary>
    void PlayLastCard();

    // Meld Phase Methods
    /// <summary>Enter meld phase - check if player wants to meld</summary>
    void TryMeld();
    /// <summary>Called when meld was successfully declared</summary>
    void MeldSuccess();
    /// <summary>Called when meld was not declared or failed</summary>
    void MeldFailed();

    // Trick Transition Methods
    /// <summary>Start a new trick - clear played cards, winner leads</summary>
    void StartNewTrick();
    /// <summary>Each player draws 1 card from the deck</summary>
    void DrawCardsForAll();

    // L9 Play Phase Methods (Last 9 cards, no drawing)
    /// <summary>First player plays a card in L9 phase</summary>
    void L9PlayFirstCard();
    /// <summary>Middle player(s) play a card in L9 phase (4 player game only)</summary>
    void L9PlayMidCard();
    /// <summary>Last player plays a card in L9 phase</summary>
    void L9PlayLastCard();
    /// <summary>Start a new trick in L9 phase</summary>
    void StartL9NewTrick();

    // Round End Methods
    /// <summary>End the round and calculate round scores</summary>
    void EndRound();
    /// <summary>Calculate total scores and check for winning score</summary>
    void CalculatePoints();

    // Game End Method
    /// <summary>Game is over - declare winner</summary>
    void GameOver();
}
