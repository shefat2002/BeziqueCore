namespace BeziqueCore.Interfaces;

public interface IRandom
{
    int Next(int maxValue);
    int Next(int minValue, int maxValue);
}

public sealed class SystemRandom : IRandom
{
    private readonly Random _random = new();

    public int Next(int maxValue) => _random.Next(maxValue);
    public int Next(int minValue, int maxValue) => _random.Next(minValue, maxValue);
}
