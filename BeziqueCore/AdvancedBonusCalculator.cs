namespace BeziqueCore;

public static class AdvancedBonusCalculator
{
    public static int CalculateBonus(int aceTenCount, byte playerCount)
    {
        int threshold = playerCount == 2 ? 14 : 8;
        if (aceTenCount < threshold) return 0;
        return aceTenCount * 10;
    }
}
