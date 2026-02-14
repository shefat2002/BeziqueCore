using BeziqueCore;
using BeziqueCore.Interfaces;
using Google.Protobuf.WellKnownTypes;
using GrpcService.Services;
using Grpc.Core;
using GrpcService;
using Xunit;

namespace BeziqueCore.Tests;

public class BeziqueServiceTests
{
    private readonly BeziqueService _service;
    private readonly ServerCallContext _context = default!;

    public BeziqueServiceTests()
    {
        _service = new BeziqueService();
    }

    [Fact]
    public async Task StartGame_With2Players_ReturnsSuccess()
    {
        var request = new PlayerCount { PlayerCount_ = 2 };
        var result = await _service.StartGame(request, _context);

        Assert.Contains("Game started with 2 players", result.GameStartMessage);
    }

    [Fact]
    public async Task StartGame_With4Players_ReturnsSuccess()
    {
        var request = new PlayerCount { PlayerCount_ = 4 };
        var result = await _service.StartGame(request, _context);

        Assert.Contains("Game started with 4 players", result.GameStartMessage);
    }

    [Fact]
    public async Task StartGame_WithInvalidPlayerCount_ReturnsFailure()
    {
        var request = new PlayerCount { PlayerCount_ = 3 };
        var result = await _service.StartGame(request, _context);

        Assert.Equal("Failure! Invalid Player Count", result.GameStartMessage);
    }

    [Fact]
    public async Task DealCard_WithoutStartingGame_ThrowsException()
    {
        var request = new Empty();

        var exception = await Assert.ThrowsAsync<RpcException>(() =>
            _service.DealCard(request, _context));

        Assert.Contains("Game not started", exception.Status.Detail);
    }

    [Fact]
    public async Task GetGameState_AfterStartGame_ReturnsValidState()
    {
        await _service.StartGame(new PlayerCount { PlayerCount_ = 2 }, _context);
        var result = await _service.GetGameState(new Empty(), _context);

        Assert.NotNull(result);
        Assert.NotNull(result.CurrentState);
        Assert.Equal(2, result.Players.Count);
    }

    [Fact]
    public async Task GetGameState_WithoutStartingGame_ThrowsException()
    {
        var exception = await Assert.ThrowsAsync<RpcException>(() =>
            _service.GetGameState(new Empty(), _context));

        Assert.Contains("Game not started", exception.Status.Detail);
    }

    [Fact]
    public async Task PlayCard_WithoutStartingGame_ThrowsException()
    {
        var request = new PlayCardRequest { PlayerIndex = 0, CardId = 1 };

        var exception = await Assert.ThrowsAsync<RpcException>(() =>
            _service.PlayCard(request, _context));

        Assert.Contains("Game not started", exception.Status.Detail);
    }

    [Fact]
    public async Task PlayCard_WithValidCard_ReturnsSuccess()
    {
        await _service.StartGame(new PlayerCount { PlayerCount_ = 2 }, _context);
        await _service.DealCard(new Empty(), _context);

        var request = new PlayCardRequest { PlayerIndex = 0, CardId = 1 };
        var result = await _service.PlayCard(request, _context);

        Assert.True(result.Success);
        Assert.Equal("Card played successfully", result.Message);
    }

    [Fact]
    public async Task PlayCard_WrongPlayerTurn_ReturnsFailure()
    {
        await _service.StartGame(new PlayerCount { PlayerCount_ = 2 }, _context);
        await _service.DealCard(new Empty(), _context);

        var request = new PlayCardRequest { PlayerIndex = 1, CardId = 1 };
        var result = await _service.PlayCard(request, _context);

        Assert.False(result.Success);
        Assert.Equal("Not your turn", result.Message);
    }

    [Fact]
    public async Task PlayCard_CardNotInHand_ReturnsFailure()
    {
        await _service.StartGame(new PlayerCount { PlayerCount_ = 2 }, _context);
        await _service.DealCard(new Empty(), _context);

        var request = new PlayCardRequest { PlayerIndex = 0, CardId = 999 };
        var result = await _service.PlayCard(request, _context);

        Assert.False(result.Success);
        Assert.Equal("Card not in hand", result.Message);
    }
}
