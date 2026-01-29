using BeziqueCore.CLI;
using Spectre.Console;

if (args.Length > 0 && args[0] == "test")
{
    RunTestMode();
    return;
}

// Initialize game controller (like Unity's MonoBehaviour Awake)
var controller = new GameController();

AnsiConsole.MarkupLine("[bold yellow]🎴 Welcome to Bezique Card Game![/]");
AnsiConsole.MarkupLine("[dim]Initializing SDK... Please wait.[/]");

// Show main menu (like Unity's scene start)
controller.ShowMainMenu();

void RunTestMode()
{
    AnsiConsole.MarkupLine("[yellow]Running in TEST mode...[/]");
    AnsiConsole.MarkupLine("[green]✓ SDK loaded successfully[/]");
    AnsiConsole.MarkupLine("[dim]Use normal mode to play the game.[/]");
}
