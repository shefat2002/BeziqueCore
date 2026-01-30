using BeziqueGame.CLI.UI;

namespace BeziqueGame.CLI
{
    /// <summary>
    /// Entry point for the Bezique CLI game application.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var controller = new GameController();
            var menu = new MainMenu(controller);

            menu.Show();
        }
    }
}
