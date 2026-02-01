using Grpc.Core;

namespace GrpcService.Services;

public class BeziqueService : Bezique.BeziqueBase
{
    public override Task<GameInitReply> GameInit(CreateRequest request, ServerCallContext context)
    {
        var gameInitReply = new GameInitReply();

        gameInitReply.PlayerMessage = request.PlayerCount is 2 or 4 ? $"Player count = {request.PlayerCount}" :
            "Invalid Player Count!";

        return Task.FromResult(gameInitReply);
    }
    
}

