using Spectre.Console;
using BeziqueCore.Models;

namespace BeziqueGame.CLI.UI
{
    /// <summary>
    /// Main menu UI matching Figma design with game mode cards
    /// </summary>
    public class MainMenu
    {
        private readonly GameController _controller;

        public MainMenu(GameController controller)
        {
            _controller = controller;
        }

        public void Show()
        {
            while (true)
            {
                AnsiConsole.Clear();
                DisplayHeader();
                DisplayGameModeCards();

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("")
                        .AddChoices(new[] { "1 vs 1", "4 Player", "Show Rules", "Exit" })
                );

                switch (choice)
                {
                    case "1 vs 1":
                        ShowModeSelection(singlePlayer: true);
                        break;
                    case "4 Player":
                        ShowModeSelection(singlePlayer: false);
                        break;
                    case "Show Rules":
                        ShowRules();
                        break;
                    case "Exit":
                        Environment.Exit(0);
                        break;
                }
            }
        }

        private void DisplayHeader()
        {
            // Deep navy blue background with teal-green accent
            AnsiConsole.Clear();

            var header = new FigletText("BEZIQUE")
            {
                Justification = Justify.Center
            };
            AnsiConsole.Write(header);

            AnsiConsole.MarkupLine("\n[bold teal]  CARD GAME  [/]\n");
        }

        private void DisplayGameModeCards()
        {
            // Display game mode cards as per Figma design
            var grid = new Grid()
                .Centered()
                .AddColumn()
                .AddColumn()
                .AddColumn();

            // 1 vs 1 Card (Blue) and 4 Player Card (Green)
            grid.AddRow(
                CreateCard("1 VS 1", "blue"),
                new Text("    "),
                CreateCard("4 PLAYER", "green")
            );

            AnsiConsole.Write(grid);
            AnsiConsole.WriteLine();

            // Version info at bottom right
            AnsiConsole.MarkupLine("[dim grey23]VERSION 1.0.0[/]");
        }

        private Markup CreateCard(string title, string colorName)
        {
            var cardDisplay = $"[bold {colorName}]   ♠   [/]\n\n[bold white]{title}[/]";
            return new Markup(cardDisplay);
        }

        private void ShowModeSelection(bool singlePlayer)
        {
            var mode = ShowModeModal();

            if (singlePlayer)
            {
                var playerName = GetPlayerName();
                _controller.StartSinglePlayerGame(playerName, mode);
            }
            else
            {
                _controller.StartMultiplayerGame(4, mode);
            }
        }

        private GameMode ShowModeModal()
        {
            // Dark blue-grey background for modal
            AnsiConsole.Clear();

            DisplayHeader();

            // Card decoration
            var cardArt = "[bold purple]    ╭─────╮  ╭─────╮[/]\n" +
                           "[bold purple]    │♠    │  │    ♠│[/]\n" +
                           "[bold purple]    │     │  │     │[/]\n" +
                           "[bold purple]    │  ♠  │  │  ♠  │[/]\n" +
                           "[bold purple]    │     │  │     │[/]\n" +
                           "[bold purple]    │♠    │  │    ♠│[/]\n" +
                           "[bold purple]    ╰─────╯  ╰─────╯[/]";

            AnsiConsole.MarkupLine(cardArt);
            AnsiConsole.MarkupLine("\n[bold white]WHICH MODE DO YOU WANT TO PLAY?[/]\n");

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("")
                    .HighlightStyle(new Style(Color.White))
                    .AddChoices(new[] { "STANDARD", "ADVANCED" })
            );

            return choice == "ADVANCED" ? GameMode.Advanced : GameMode.Standard;
        }

        private string GetPlayerName()
        {
            AnsiConsole.MarkupLine("\n[bold yellow]Enter your name:[/]");
            var name = AnsiConsole.Ask<string>("> ");
            return string.IsNullOrWhiteSpace(name) ? "Player 1" : name;
        }

        private void ShowRules()
        {
            AnsiConsole.Clear();

            DisplayHeader();

            var rules = new Panel(_controller.GetRulesText())
            {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Teal),
                Padding = new Padding(2, 2, 2, 2),
                Expand = true
            };

            AnsiConsole.Write(rules);
            AnsiConsole.MarkupLine("\n[dim]Press any key to return...[/]");
            Console.ReadKey(true);
        }
    }
}
