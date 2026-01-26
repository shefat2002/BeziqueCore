namespace BeziqueCore.CLI;

public class ConcreateBezicGame : IBeziqueGame
{
    public void NotifyAll()
    {
        Console.WriteLine(nameof(ConcreateBezicGame));
    }
}