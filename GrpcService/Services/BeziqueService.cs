using BeziqueCore;
using Grpc.Core;

namespace GrpcService.Services;



public class BeziqueService : Bezique.BeziqueBase
{
    public BeziqueCore.Bezique bezique;
    
    public override Task<GameStarted> StartGame(PlayerCount request, ServerCallContext context)
    {
        var playerCount = request.PlayerCount_;

        // Validate player count
        if (!playerCount.TryValidatePlayerCount(out var errorMessage))
        {
            return Task.FromResult(new GameStarted
            {
                GameStartMessage = errorMessage
            });
        }

        bezique = new BeziqueCore.Bezique();
        bezique.Start();
        bezique.SetAdapter(new BeziqueConcruite(playerCount));

        if (playerCount == 2) bezique.DispatchEvent(BeziqueCore.Bezique.EventId.TWOPLAYERGAME);
        if (playerCount == 4) bezique.DispatchEvent(BeziqueCore.Bezique.EventId.FOURPLAYERGAME);

        var stateIdToString = BeziqueCore.Bezique.StateIdToString(bezique.stateId);

        return Task.FromResult(new GameStarted
        {
            GameStartMessage = $"Game started with {playerCount} players. Current state: {stateIdToString}"
        });
    }
}


public static class BeziqueError
{
    public static bool TryValidatePlayerCount(this int playerCount, out string result)
    {
        result = "";
        if (playerCount != 2 && playerCount != 4)
        {
            result = "Failure! Invalid Player Count";
            return false;
        }
        return true;
    }
    
}