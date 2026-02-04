namespace BeziqueCore;

public class GameContext
{
    public Stack<Card> DrawDeck { get; set; }
    public Card TrumpCard { get; set; }
    public Suit TrumpSuit { get; set; }
    public GamePhase CurrentPhase { get; set; }
    public int CurrentTurnPlayer { get; set; }
    public int LastTrickWinner { get; set; }

    public GameContext()
    {
        DrawDeck = new();
        CurrentPhase = GamePhase.Phase1_Normal;
        CurrentTurnPlayer = 0;
        LastTrickWinner = 0;
    }
}
