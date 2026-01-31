using BeziqueCore.Models;

namespace BeziqueCore.Simple;

public class GameActionBuilder
{
    private readonly BeziqueGame _game;
    private int _playerIndex = -1;

    internal GameActionBuilder(BeziqueGame game)
    {
        _game = game;
    }

    public GameActionBuilder ForPlayer(int playerIndex)
    {
        _playerIndex = playerIndex;
        return this;
    }

    public GameActionBuilder ForPlayer(string playerName)
    {
        var index = _game.Players.ToList().FindIndex(p => p.Name == playerName);
        if (index < 0)
            throw new ArgumentException($"Player '{playerName}' not found");

        _playerIndex = index;
        return this;
    }

    public Task<ActionResult> PlayCardAsync(int cardIndex)
    {
        ValidatePlayer();
        return ExecuteAsync(() => _game.PlayCard(_playerIndex, cardIndex), $"PlayCard({cardIndex})");
    }

    public Task<ActionResult> DeclareMeldAsync(params int[] cardIndices)
    {
        ValidatePlayer();
        return ExecuteAsync(() => _game.DeclareMeld(_playerIndex, cardIndices), $"DeclareMeld({string.Join(", ", cardIndices)})");
    }

    public Task<ActionResult> SkipMeldAsync()
    {
        ValidatePlayer();
        return ExecuteAsync(() => _game.SkipMeld(_playerIndex), "SkipMeld");
    }

    public Task<ActionResult> SwitchSevenOfTrumpAsync()
    {
        ValidatePlayer();
        return ExecuteAsync(() => _game.SwitchSevenOfTrump(_playerIndex), "SwitchSevenOfTrump");
    }

    public ActionResult CanPlayCard(int cardIndex)
    {
        ValidatePlayer();
        var canPlay = _game.CanPlayCard(_playerIndex, cardIndex);
        return canPlay
            ? ActionResult.Ok($"Card {cardIndex} can be played")
            : ActionResult.Fail($"Card {cardIndex} cannot be played");
    }

    public ActionResult CanDeclareMeld(params int[] cardIndices)
    {
        ValidatePlayer();
        var canDeclare = _game.CanDeclareMeld(_playerIndex, cardIndices);
        return canDeclare
            ? ActionResult.Ok($"Meld can be declared")
            : ActionResult.Fail("Meld cannot be declared");
    }

    public ActionResult CanSwitchSevenOfTrump()
    {
        ValidatePlayer();
        var canSwitch = _game.CanSwitchSevenOfTrump(_playerIndex);
        return canSwitch
            ? ActionResult.Ok("Can switch seven of trump")
            : ActionResult.Fail("Cannot switch seven of trump");
    }

    public ActionResult<string[]> GetLegalMoves()
    {
        ValidatePlayer();
        var moves = _game.GetLegalMoves(_playerIndex);
        return ActionResult<string[]>.Ok(moves);
    }

    private void ValidatePlayer()
    {
        if (_playerIndex < 0 || _playerIndex >= _game.Players.Count)
            throw new InvalidOperationException("Player not specified. Use ForPlayer() first.");

        if (_playerIndex < 0 || _playerIndex >= _game.Players.Count)
            throw new ArgumentOutOfRangeException(nameof(_playerIndex), "Invalid player index");
    }

    private async Task<ActionResult> ExecuteAsync(Action action, string actionName)
    {
        try
        {
            action();
            return ActionResult.Ok(actionName);
        }
        catch (Exception ex)
        {
            return ActionResult.Fail($"{actionName} failed: {ex.Message}");
        }
    }
}

public static class GameActionExtensions
{
    public static GameActionBuilder Actions(this BeziqueGame game)
    {
        return new GameActionBuilder(game);
    }

    public static async Task<ActionResult> PlayCardAsync(this BeziqueGame game, int playerIndex, int cardIndex)
    {
        return await game.Actions().ForPlayer(playerIndex).PlayCardAsync(cardIndex);
    }

    public static async Task<ActionResult> DeclareMeldAsync(this BeziqueGame game, int playerIndex, params int[] cardIndices)
    {
        return await game.Actions().ForPlayer(playerIndex).DeclareMeldAsync(cardIndices);
    }

    public static async Task<ActionResult> SkipMeldAsync(this BeziqueGame game, int playerIndex)
    {
        return await game.Actions().ForPlayer(playerIndex).SkipMeldAsync();
    }

    public static async Task<ActionResult> SwitchSevenOfTrumpAsync(this BeziqueGame game, int playerIndex)
    {
        return await game.Actions().ForPlayer(playerIndex).SwitchSevenOfTrumpAsync();
    }
}
