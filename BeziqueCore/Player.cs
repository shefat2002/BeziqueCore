namespace BeziqueCore;

public class Player
{
    public int PlayerID { get; init; }
    public List<Card> Hand { get; init; }
    public List<Card> TableCards { get; init; }
    public List<Card> WonPile { get; init; }
    public int RoundScore { get; set; }
    public int TotalScore { get; set; }
    public bool HasSwappedSeven { get; set; }
    public Dictionary<MeldType, List<Card>> MeldHistory { get; init; }

    public Player(int playerId)
    {
        PlayerID = playerId;
        Hand = [];
        TableCards = [];
        WonPile = [];
        RoundScore = 0;
        TotalScore = 0;
        HasSwappedSeven = false;
        MeldHistory = [];
    }
}
