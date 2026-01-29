using BeziqueGame.CLI;
using Spectre.Console;

var controller = new GameController();

AnsiConsole.MarkupLine("[bold yellow]🎴 Welcome to Bezique Card Game![/]");
AnsiConsole.MarkupLine("[dim]Initializing SDK... Please wait.[/]");

controller.ShowMainMenu();
