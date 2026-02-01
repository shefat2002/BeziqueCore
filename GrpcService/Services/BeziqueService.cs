using Grpc.Core;

namespace GrpcService.Services;



public class BeziqueService : Bezique.BeziqueBase
{
    private static readonly Random Rnd = new();
    private List<string> _cards = new ();
    
    
    public override Task<GameInitReply> GameInit(PlayerJoinRequest request, ServerCallContext context)
    {
        
        var gameInitReply = new GameInitReply();
        var playerCount = request.PlayerCount;

        if (!playerCount.TryValidatePlayerCount(out var error)) gameInitReply.PlayerMessage = error;
        
        return Task.FromResult(gameInitReply);
    }
    
    public override Task<CardDealtReply> CardDealt(CardRequest request, ServerCallContext context)
    {
        var cardDealtReply = new CardDealtReply
        {
            CardId1 = 0,
            CardId2 = 1,
            CardId3 = 2
        };
        return Task.FromResult(cardDealtReply);
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