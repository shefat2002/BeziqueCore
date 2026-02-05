using BeziqueCore;
using BeziqueCLI.UI;

namespace BeziqueCLI;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var renderer = new GameRenderer();
        renderer.ShowWelcome();

        var game = new BeziqueGame(renderer);

        try
        {
            game.Start();
        }
        catch (Exception ex)
        {
            renderer.ShowError($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}
