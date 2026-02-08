using Grpc.Core;

namespace GrpcService.Services;



public class BeziqueService : Bezique.BeziqueBase
{
    public override Task<GameStarted> StartGame(PlayerCount request, ServerCallContext context)
    {
        var playerCount = request.PlayerCount_;
        return base.StartGame(request, context);
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