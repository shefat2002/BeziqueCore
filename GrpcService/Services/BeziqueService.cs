using Grpc.Core;

namespace GrpcService.Services;

public class BeziqueService : Bezique.BeziqueBase
{
    public override Task<GameInitReply> GameInit(CreateRequest request, ServerCallContext context)
    {
        var gameInitReply = new GameInitReply();
        var playerCount = request.PlayerCount;

        if (!playerCount.TryValidatePlayerCount(out var error)) gameInitReply.PlayerMessage = error;
        

        return Task.FromResult(gameInitReply);
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