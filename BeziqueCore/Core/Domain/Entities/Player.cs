namespace BeziqueCore.Core.Domain.Entities;

public class Player
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Score { get; set; }
    public List<Card> Hand { get; set; }
    public bool IsDealer { get; set; }
    public bool IsBot { get; set; }
    public List<Meld> DeclaredMelds { get; set; }
    public List<Card> MeldedCards { get; set; }
}

public class Meld
{
    public MeldType Type { get; set; }
    public List<Card> Cards { get; set; }
    public int Points { get; set; }
}

public enum MeldType
{
    TrumpRun,
    TrumpSeven,
    TrumpMarriage,
    Marriage,
    FourAces,
    FourKings,
    FourQueens,
    FourJacks,
    Bezique,
    DoubleBezique,
    InvalidMeld
}
