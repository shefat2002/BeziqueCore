using BeziqueCore.Interfaces;
using BeziqueCore.Multiplayer.Models;

namespace BeziqueCore.Multiplayer;

public interface IMultiplayerGameAdapter : IGameAdapter
{
    GameSnapshotDto GetSnapshot();
    GameSnapshotDto GetSnapshotForPlayer(string userId);
    Task<GameActionResult> ExecuteRemoteCommandAsync(PlayerCommand command);
    string[] GetLegalMoves(string userId);
    bool CanPlayerAct(string userId);
    string? GetCurrentPlayerUserId();
    Player? GetPlayerById(string userId);
    void InitializeGameWithPlayers(List<Player> players);
    bool CanPlayCard(string userId, int cardIndex);
    bool CanDeclareMeld(string userId, int[] cardIndices);
    bool CanSwitchSevenOfTrump(string userId);
    string GetCurrentStateName();
    void SubscribeToEvents(IMultiplayerEventHandler eventHandler);
}
