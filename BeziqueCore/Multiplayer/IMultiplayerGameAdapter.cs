using BeziqueCore.Interfaces;
using BeziqueCore.Multiplayer.Models;

namespace BeziqueCore.Multiplayer;

/// <summary>
/// Multiplayer-specific game adapter interface for online Bezique games.
/// Extends IGameAdapter with network-aware functionality.
/// </summary>
public interface IMultiplayerGameAdapter : IGameAdapter
{
    /// <summary>
    /// Gets the current game state as a serializable snapshot for network transmission.
    /// </summary>
    GameSnapshotDto GetSnapshot();

    /// <summary>
    /// Gets the current game state as a serializable snapshot for a specific player.
    /// Includes the player's hand in the snapshot.
    /// </summary>
    /// <param name="userId">The unique player identifier.</param>
    GameSnapshotDto GetSnapshotForPlayer(string userId);

    /// <summary>
    /// Executes a remote player command from the network.
    /// </summary>
    /// <param name="command">The player command to execute.</param>
    /// <returns>The result of the command execution.</returns>
    Task<GameActionResult> ExecuteRemoteCommandAsync(PlayerCommand command);

    /// <summary>
    /// Gets the legal moves/actions available for a specific player.
    /// Used for client-side validation and UI enabling/disabling.
    /// </summary>
    /// <param name="userId">The unique player identifier.</param>
    /// <returns>Array of legal action names for this player.</returns>
    string[] GetLegalMoves(string userId);

    /// <summary>
    /// Checks if a specific player can act (is it their turn).
    /// </summary>
    /// <param name="userId">The unique player identifier.</param>
    /// <returns>True if the player can act; otherwise, false.</returns>
    bool CanPlayerAct(string userId);

    /// <summary>
    /// Gets the current player's user ID.
    /// </summary>
    string? GetCurrentPlayerUserId();

    /// <summary>
    /// Gets a player by their user ID.
    /// </summary>
    /// <param name="userId">The unique player identifier.</param>
    /// <returns>The player if found; otherwise, null.</returns>
    Player? GetPlayerById(string userId);

    /// <summary>
    /// Initializes the game with specific players for multiplayer.
    /// </summary>
    /// <param name="players">The players to add to the game.</param>
    void InitializeGameWithPlayers(List<Player> players);

    /// <summary>
    /// Checks if a specific card can be played by the current player.
    /// </summary>
    /// <param name="userId">The unique player identifier.</param>
    /// <param name="cardIndex">The index of the card in the player's hand.</param>
    /// <returns>True if the card can be played; otherwise, false.</returns>
    bool CanPlayCard(string userId, int cardIndex);

    /// <summary>
    /// Checks if a meld can be declared by the current player.
    /// </summary>
    /// <param name="userId">The unique player identifier.</param>
    /// <param name="cardIndices">The indices of the cards in the player's hand.</param>
    /// <returns>True if the meld can be declared; otherwise, false.</returns>
    bool CanDeclareMeld(string userId, int[] cardIndices);

    /// <summary>
    /// Checks if the player has the seven of trump to switch.
    /// </summary>
    /// <param name="userId">The unique player identifier.</param>
    /// <returns>True if the player can switch; otherwise, false.</returns>
    bool CanSwitchSevenOfTrump(string userId);

    /// <summary>
    /// Gets the current state name from the state machine.
    /// </summary>
    string GetCurrentStateName();

    /// <summary>
    /// Subscribes to multiplayer-specific game events.
    /// </summary>
    /// <param name="eventHandler">The event handler for multiplayer events.</param>
    void SubscribeToEvents(IMultiplayerEventHandler eventHandler);
}
