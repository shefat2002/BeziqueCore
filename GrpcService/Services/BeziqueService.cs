using Grpc.Core;

namespace GrpcService.Services;

public class BeziqueService : Bezique.BeziqueBase
{
    public override Task<GameInitReply> GameInit(CreateRequest request, ServerCallContext context)
    {
        var gameInitReply = new GameInitReply();
        var is2Player = request.PlayerCount == 2;
        var is4Player = request.PlayerCount == 4;
        if (is2Player == false && is4Player == false)
        {
            gameInitReply.Message = "Mor shala";
        }
        return Task.FromResult(gameInitReply);
    }
}