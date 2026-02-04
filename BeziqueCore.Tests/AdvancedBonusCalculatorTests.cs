using BeziqueCore;

public class AdvancedBonusCalculatorTests
{
    [Fact]
    public void CalculateBonus_TwoPlayers_BelowThreshold_ReturnsZero()
    {
        Assert.Equal(0, AdvancedBonusCalculator.CalculateBonus(aceTenCount: 13, playerCount: 2));
    }

    [Fact]
    public void CalculateBonus_TwoPlayers_AtThreshold_ReturnsBonus()
    {
        Assert.Equal(140, AdvancedBonusCalculator.CalculateBonus(aceTenCount: 14, playerCount: 2));
    }

    [Fact]
    public void CalculateBonus_TwoPlayers_AboveThreshold_ReturnsBonus()
    {
        Assert.Equal(150, AdvancedBonusCalculator.CalculateBonus(aceTenCount: 15, playerCount: 2));
    }

    [Fact]
    public void CalculateBonus_TwoPlayers_MaxBonus_ReturnsCorrectAmount()
    {
        Assert.Equal(250, AdvancedBonusCalculator.CalculateBonus(aceTenCount: 25, playerCount: 2));
    }

    [Fact]
    public void CalculateBonus_FourPlayers_BelowThreshold_ReturnsZero()
    {
        Assert.Equal(0, AdvancedBonusCalculator.CalculateBonus(aceTenCount: 7, playerCount: 4));
    }

    [Fact]
    public void CalculateBonus_FourPlayers_AtThreshold_ReturnsBonus()
    {
        Assert.Equal(80, AdvancedBonusCalculator.CalculateBonus(aceTenCount: 8, playerCount: 4));
    }

    [Fact]
    public void CalculateBonus_FourPlayers_AboveThreshold_ReturnsBonus()
    {
        Assert.Equal(90, AdvancedBonusCalculator.CalculateBonus(aceTenCount: 9, playerCount: 4));
    }

    [Fact]
    public void CalculateBonus_FourPlayers_MaxBonus_ReturnsCorrectAmount()
    {
        Assert.Equal(160, AdvancedBonusCalculator.CalculateBonus(aceTenCount: 16, playerCount: 4));
    }

    [Fact]
    public void CalculateBonus_ZeroCount_ReturnsZero()
    {
        Assert.Equal(0, AdvancedBonusCalculator.CalculateBonus(aceTenCount: 0, playerCount: 2));
        Assert.Equal(0, AdvancedBonusCalculator.CalculateBonus(aceTenCount: 0, playerCount: 4));
    }
}
