using BeziqueCore;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace GrpcService.Services;



public class BeziqueService : Bezique.BeziqueBase
{
    private BeziqueCore.Bezique? _bezique;
    private BeziqueConcrete? _adapter; 
    
    
    
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

        _adapter = new BeziqueConcrete(playerCount);
        _bezique = new BeziqueCore.Bezique();
        _bezique.SetAdapter(_adapter);
        _bezique.Start();

        if (playerCount == 2) _bezique.DispatchEvent(BeziqueCore.Bezique.EventId.TWOPLAYERGAME);
        if (playerCount == 4) _bezique.DispatchEvent(BeziqueCore.Bezique.EventId.FOURPLAYERGAME);

        var stateIdToString = BeziqueCore.Bezique.StateIdToString(_bezique.stateId);

        return Task.FromResult(new GameStarted
        {
            GameStartMessage = $"Game started with {playerCount} players. Current state: {stateIdToString}"
        });
    }

    public override Task<DealCardResponse> DealCard(Empty request, ServerCallContext context)
    {
        if (_bezique == null || _adapter == null)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, "Game not started. Call StartGame first."));
        }

        var currentState = _bezique.stateId;
        bool isDealingState = currentState == BeziqueCore.Bezique.StateId.DEALFOURPLAYER ||
                              currentState == BeziqueCore.Bezique.StateId.DEAL4_FIRSTPLAYER ||
                              currentState == BeziqueCore.Bezique.StateId.DEAL4_SECONDPLAYER ||
                              currentState == BeziqueCore.Bezique.StateId.DEAL4_THIRDPLAYER ||
                              currentState == BeziqueCore.Bezique.StateId.DEAL4_FOURTHPLAYER ||
                              currentState == BeziqueCore.Bezique.StateId.DEALTWOPLAYER ||
                              currentState == BeziqueCore.Bezique.StateId.DEAL2_FIRSTPLAYER ||
                              currentState == BeziqueCore.Bezique.StateId.DEAL2_SECONDPLAYER;
        if (!isDealingState)
        {
            return Task.FromResult(new DealCardResponse
            {
                PlayerIndex = -1,
                CurrentState = BeziqueCore.Bezique.StateIdToString(currentState),
                DealingComplete = currentState == BeziqueCore.Bezique.StateId.PLAYPHASE1 ||
                                  currentState == BeziqueCore.Bezique.StateId.FLIPTRUMP
            });
        }
        
        int playerIndex = _adapter.DealOrder;
        int cardCountBefore = _adapter.Player[playerIndex].Count;

        _bezique.DispatchEvent(BeziqueCore.Bezique.EventId.COMPLETE);

        int cardCountAfter = _adapter.Player[playerIndex].Count;
        var dealtCards = _adapter.Player[playerIndex].Skip(cardCountBefore).Take(3).ToList();

        var cardMessages = dealtCards.Select(cardId => new Card
        {
            Id = cardId,
            Value = CardHelper.GetCardValue((byte)cardId),
            Deck = CardHelper.GetDeckIndex((byte)cardId)
        }).ToList();

        bool dealingComplete = _bezique.stateId == BeziqueCore.Bezique.StateId.PLAYPHASE1 ||
                               _bezique.stateId == BeziqueCore.Bezique.StateId.FLIPTRUMP;

        var response = new DealCardResponse
        {
            PlayerIndex = playerIndex,
            CurrentState = BeziqueCore.Bezique.StateIdToString(_bezique.stateId),
            DealingComplete = dealingComplete
        };
        response.Cards.AddRange(cardMessages);
        return Task.FromResult(response);
    }

    public override Task<GameState> GetGameState(Empty request, ServerCallContext context)
    {
        if (_bezique == null || _adapter == null)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, "Game not started. Call StartGame first."));
        }

        var gameState = new GameState
        {
            CurrentPlayerIndex = _adapter.DealOrder,
            CurrentState = BeziqueCore.Bezique.StateIdToString(_bezique.stateId)
        };

        for (int i = 0; i < _adapter.Player.Length; i++)
        {
            var playerHand = new PlayerHand
            {
                PlayerIndex = i
            };

            foreach (var cardId in _adapter.Player[i])
            {
                playerHand.Cards.Add(new Card
                {
                    Id = cardId,
                    Value = CardHelper.GetCardValue((byte)cardId),
                    Deck = CardHelper.GetDeckIndex((byte)cardId)
                });
            }

            gameState.Players.Add(playerHand);
        }

        return Task.FromResult(gameState);
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