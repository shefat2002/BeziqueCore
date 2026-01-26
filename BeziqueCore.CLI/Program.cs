using BeziqueCore.CLI;
using Spectre.Console;
using BeziqueCore.Models;
using BeziqueCore.Interfaces;

if (args.Length > 0 && args[0] == "test")
{
    RunTestMode();
    return;
}

var gameConsole = new GameConsole();
var stateMachine = gameConsole.CreateStateMachine();
var menu = new GameMenu(gameConsole, stateMachine);

AnsiConsole.MarkupLine("[bold yellow]🎴 Welcome to Bezique Card Game![/]");
AnsiConsole.MarkupLine("[dim]Initializing... Please wait.[/]");

menu.ShowMainMenu();

void RunTestMode()
{
    AnsiConsole.MarkupLine("[yellow]Running in TEST mode...[/]");

    var gameConsole = new GameConsole();

    var player1 = new Player { Id = "1", Name = "Alice", Score = 0, Hand = new List<Card>(), IsDealer = false };
    var player2 = new Player { Id = "2", Name = "Bob", Score = 0, Hand = new List<Card>(), IsDealer = true };

    gameConsole.AddPlayer(player1);
    gameConsole.AddPlayer(player2);
    gameConsole.SetCurrentPlayer(0);

    var stateMachine = gameConsole.CreateStateMachine();

    AnsiConsole.MarkupLine("[green]✓ Game initialized[/]");
    AnsiConsole.MarkupLine("[dim]Starting state machine...[/]");

    stateMachine.Start();

    AnsiConsole.MarkupLine($"[cyan]Current state: {stateMachine.stateId}[/]");

    stateMachine.DispatchGameInitialized();
    AnsiConsole.MarkupLine($"[cyan]After INIT: {stateMachine.stateId}[/]");

    stateMachine.DispatchCardsDealt();
    AnsiConsole.MarkupLine($"[cyan]After DEALING: {stateMachine.stateId}[/]");

    stateMachine.DispatchTrumpDetermined();
    AnsiConsole.MarkupLine($"[cyan]After TRUMP: {stateMachine.stateId}[/]");

    gameConsole.DisplayHands();
}
