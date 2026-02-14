using BeziqueCore;
using BeziqueCore.Interfaces;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace GrpcService.Services;

public class BeziqueService : Bezique.BeziqueBase
{
    private readonly Dictionary<int, BeziqueGame> _games = new();
    private int _nextGameId = 1;

    public override Task<GameStarted> StartGame(PlayerCount request, ServerCallContext context)
    {
        int playerCount = request.PlayerCount_;

        if (playerCount != 2 && playerCount != 4)
        {
            return Task.FromResult(new GameStarted
            {
                GameStartMessage = "Failure! Invalid Player Count"
            });
        }

        var game = new BeziqueGame();
        game.Initialize(playerCount);
        game.StartDealing();

        int gameId = _nextGameId++;
        _games[gameId] = game;

        return Task.FromResult(new GameStarted
        {
            GameStartMessage = $"Game started with {playerCount} players. Game ID: {gameId}"
        });
    }

    public override Task<DealCardResponse> DealCard(Empty request, ServerCallContext context)
    {
        var game = GetDefaultGame();

        bool couldDeal = game.DealNextSet();
        var players = game.Players;
        var currentPlayer = game.CurrentPlayerIndex;

        var response = new DealCardResponse
        {
            PlayerIndex = currentPlayer,
            CurrentState = game.CurrentPhase.ToString(),
            DealingComplete = !couldDeal,
            CurrentRound = (int)game.CurrentPhase + 1,
            TotalRounds = 3
        };

        foreach (var card in players[currentPlayer].Cards)
        {
            response.Cards.Add(new Card
            {
                Id = card.CardId,
                Value = card.CardValue,
                Deck = card.DeckIndex
            });
        }

        return Task.FromResult(response);
    }

    public override Task<GameState> GetGameState(Empty request, ServerCallContext context)
    {
        var game = GetDefaultGame();
        var players = game.Players;

        var gameState = new GameState
        {
            CurrentPlayerIndex = game.CurrentPlayerIndex,
            CurrentState = game.CurrentPhase.ToString()
        };

        foreach (var player in players)
        {
            var playerHand = new PlayerHand
            {
                PlayerIndex = player.PlayerIndex
            };

            foreach (var card in player.Cards)
            {
                playerHand.Cards.Add(new Card
                {
                    Id = card.CardId,
                    Value = card.CardValue,
                    Deck = card.DeckIndex
                });
            }

            gameState.Players.Add(playerHand);
        }

        return Task.FromResult(gameState);
    }

    public override Task<PlayCardResponse> PlayCard(PlayCardRequest request, ServerCallContext context)
    {
        var game = GetDefaultGame();
        int playerIndex = request.PlayerIndex;
        byte cardId = (byte)request.CardId;

        if (!game.IsPlayerTurn(playerIndex))
        {
            return Task.FromResult(new PlayCardResponse
            {
                Success = false,
                Message = "Not your turn",
                NextPlayerIndex = game.CurrentPlayerIndex,
                GameState = game.CurrentPhase.ToString()
            });
        }

        var playableCards = game.GetPlayableCards(playerIndex);
        if (!playableCards.Contains(cardId))
        {
            return Task.FromResult(new PlayCardResponse
            {
                Success = false,
                Message = "Card not in hand",
                NextPlayerIndex = game.CurrentPlayerIndex,
                GameState = game.CurrentPhase.ToString()
            });
        }

        game.PlayCard(playerIndex, cardId);

        return Task.FromResult(new PlayCardResponse
        {
            Success = true,
            Message = "Card played successfully",
            NextPlayerIndex = game.CurrentPlayerIndex,
            GameState = game.CurrentPhase.ToString()
        });
    }

    private BeziqueGame GetDefaultGame()
    {
        if (_games.Count == 0)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, "Game not started. Call StartGame first."));
        }
        return _games.Values.First();
    }
}
