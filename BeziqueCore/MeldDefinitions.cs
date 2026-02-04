namespace BeziqueCore;

public static class MeldDefinitions
{
    public static int GetPoints(MeldType meldType)
    {
        return meldType switch
        {
            MeldType.TrumpSeven => 10,
            MeldType.TrumpMarriage => 40,
            MeldType.NonTrumpMarriage => 20,
            MeldType.Bezique => 40,
            MeldType.DoubleBezique => 500,
            MeldType.TrumpRun => 250,
            MeldType.FourAces => 100,
            MeldType.FourKings => 80,
            MeldType.FourQueens => 60,
            MeldType.FourJacks => 40,
            _ => 0
        };
    }

    public static int GetRequiredCardCount(MeldType meldType)
    {
        return meldType switch
        {
            MeldType.TrumpSeven => 1,
            MeldType.TrumpMarriage => 2,
            MeldType.NonTrumpMarriage => 2,
            MeldType.Bezique => 2,
            MeldType.DoubleBezique => 4,
            MeldType.TrumpRun => 5,
            MeldType.FourAces => 4,
            MeldType.FourKings => 4,
            MeldType.FourQueens => 4,
            MeldType.FourJacks => 4,
            _ => 0
        };
    }
}
