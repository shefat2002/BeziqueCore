using BeziqueCore;
using BeziqueCore.Interfaces;
using Grpc.Core;

namespace GrpcService.Services;

public class BeziqueService : Bezique.BeziqueBase
{
    private static readonly Dictionary<int, BeziqueGame> _games = new();
    private static int _nextGameId = 1;

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
            GameStartMessage = $"Game started with {playerCount} players. Game ID: {gameId}",
            GameId = gameId
        });
    }

    public override Task<DealCardResponse> DealCard(DealCardRequest request, ServerCallContext context)
    {
        var game = GetGame(request.GameId);

        bool couldDeal = game.DealNextSet();
        var players = game.Players;
        var currentPlayer = game.CurrentPlayerIndex;

        var response = new DealCardResponse
        {
            PlayerIndex = currentPlayer,
            CurrentState = game.CurrentPhase.ToString(),
            DealingComplete = !couldDeal,
            CurrentRound = (int)game.CurrentPhase + 1,
            TotalRounds = 3,
            GameId = request.GameId
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

    public override Task<GameState> GetGameState(GameStateRequest request, ServerCallContext context)
    {
        var game = GetGame(request.GameId);
        var players = game.Players;

        var gameState = new GameState
        {
            CurrentPlayerIndex = game.CurrentPlayerIndex,
            CurrentState = game.CurrentPhase.ToString(),
            GameId = request.GameId
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
        var game = GetGame(request.GameId);
        int playerIndex = request.PlayerIndex;
        byte cardId = (byte)request.CardId;

        if (!game.IsPlayerTurn(playerIndex))
        {
            return Task.FromResult(new PlayCardResponse
            {
                Success = false,
                Message = "Not your turn",
                NextPlayerIndex = game.CurrentPlayerIndex,
                GameState = game.CurrentPhase.ToString(),
                GameId = request.GameId
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
                GameState = game.CurrentPhase.ToString(),
                GameId = request.GameId
            });
        }

        game.PlayCard(playerIndex, cardId);

        return Task.FromResult(new PlayCardResponse
        {
            Success = true,
            Message = "Card played successfully",
            NextPlayerIndex = game.CurrentPlayerIndex,
            GameState = game.CurrentPhase.ToString(),
            GameId = request.GameId
        });
    }

    private BeziqueGame GetGame(int gameId)
    {
        if (!_games.TryGetValue(gameId, out var game))
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Game ID {gameId} not found. Call StartGame first."));
        }
        return game;
    }
}
