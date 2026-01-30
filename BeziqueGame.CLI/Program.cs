using BeziqueGame.CLI;
using BeziqueGame.CLI.UI;
using Spectre.Console;

var controller = new GameController();

AnsiConsole.MarkupLine("[bold yellow]🎴 Welcome to Bezique Card Game![/]");
AnsiConsole.MarkupLine("[dim]Initializing SDK... Please wait.[/]\n");

// Show Figma-designed main menu
var mainMenu = new MainMenu(controller);
mainMenu.Show();
