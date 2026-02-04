using BeziqueCore;

public class MeldDefinitionsTests
{
    [Fact]
    public void GetPoints_ReturnsCorrectValue_ForAllMeldTypes()
    {
        Assert.Equal(10, MeldDefinitions.GetPoints(MeldType.TrumpSeven));
        Assert.Equal(40, MeldDefinitions.GetPoints(MeldType.TrumpMarriage));
        Assert.Equal(20, MeldDefinitions.GetPoints(MeldType.NonTrumpMarriage));
        Assert.Equal(40, MeldDefinitions.GetPoints(MeldType.Bezique));
        Assert.Equal(500, MeldDefinitions.GetPoints(MeldType.DoubleBezique));
        Assert.Equal(250, MeldDefinitions.GetPoints(MeldType.TrumpRun));
        Assert.Equal(100, MeldDefinitions.GetPoints(MeldType.FourAces));
        Assert.Equal(80, MeldDefinitions.GetPoints(MeldType.FourKings));
        Assert.Equal(60, MeldDefinitions.GetPoints(MeldType.FourQueens));
        Assert.Equal(40, MeldDefinitions.GetPoints(MeldType.FourJacks));
    }

    [Fact]
    public void GetRequiredCardCount_ReturnsCorrectValue_ForAllMeldTypes()
    {
        Assert.Equal(1, MeldDefinitions.GetRequiredCardCount(MeldType.TrumpSeven));
        Assert.Equal(2, MeldDefinitions.GetRequiredCardCount(MeldType.TrumpMarriage));
        Assert.Equal(2, MeldDefinitions.GetRequiredCardCount(MeldType.NonTrumpMarriage));
        Assert.Equal(2, MeldDefinitions.GetRequiredCardCount(MeldType.Bezique));
        Assert.Equal(4, MeldDefinitions.GetRequiredCardCount(MeldType.DoubleBezique));
        Assert.Equal(5, MeldDefinitions.GetRequiredCardCount(MeldType.TrumpRun));
        Assert.Equal(4, MeldDefinitions.GetRequiredCardCount(MeldType.FourAces));
        Assert.Equal(4, MeldDefinitions.GetRequiredCardCount(MeldType.FourKings));
        Assert.Equal(4, MeldDefinitions.GetRequiredCardCount(MeldType.FourQueens));
        Assert.Equal(4, MeldDefinitions.GetRequiredCardCount(MeldType.FourJacks));
    }

    [Fact]
    public void GetPoints_ReturnsZero_ForInvalidMeldType()
    {
        var invalidMeld = (MeldType)99;
        Assert.Equal(0, MeldDefinitions.GetPoints(invalidMeld));
    }

    [Fact]
    public void GetRequiredCardCount_ReturnsZero_ForInvalidMeldType()
    {
        var invalidMeld = (MeldType)99;
        Assert.Equal(0, MeldDefinitions.GetRequiredCardCount(invalidMeld));
    }
}
