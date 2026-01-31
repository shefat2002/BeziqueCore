using BeziqueCore.Core.Domain.Entities;
using BeziqueCore.Core.Infrastructure.Deck;
using BeziqueCore.Core.Infrastructure.Notifications;
using BeziqueCore.Core.Infrastructure.Players;
using BeziqueCore.Core.Infrastructure.Timing;
using BeziqueCore.Core.Infrastructure.Validation;
using BeziqueCore.Core.Multiplayer.Adapters;

namespace BeziqueCore.Core.API.Factory;

public static class BeziqueGameManager
{
    public static BeziqueGame CreateSinglePlayer(string humanName, string aiName)
    {
        return CreateGame(
            new PlayerConfig { Name = humanName, IsBot = false },
            new PlayerConfig { Name = aiName, IsBot = true }
        );
    }

    public static BeziqueGame CreateMultiplayer(params string[] playerNames)
    {
        if (playerNames.Length < 2)
            throw new ArgumentException("At least 2 players required");

        var configs = playerNames.Select(name => new PlayerConfig { Name = name, IsBot = false });
        return CreateGame(configs.ToArray());
    }

    public static BeziqueGame CreateGame(params PlayerConfig[] playerConfigs)
    {
        if (playerConfigs.Length < 2)
            throw new ArgumentException("At least 2 players required");

        var playerTimer = new PlayerTimer();
        var gameState = new GameState(playerTimer);
        var notifier = new GameStateNotifier();
        var trickResolver = new TrickResolver(gameState);
        var meldValidator = new MeldValidator();
        var deckOps = new DeckOperations();
        var playerActions = new PlayerActions(deckOps, meldValidator, notifier, gameState);
        var gameAdapter = new GameAdapter(deckOps, playerActions, notifier, gameState, trickResolver);

        var multiplayerAdapter = new MultiplayerGameAdapter(
            gameAdapter, deckOps, playerActions, notifier, gameState, trickResolver, meldValidator);

        var game = new BeziqueGame(multiplayerAdapter, notifier, gameState);

        int seatIndex = 0;
        foreach (var config in playerConfigs)
        {
            var player = new Player
            {
                Id = Guid.NewGuid().ToString(),
                Name = config.Name,
                IsBot = config.IsBot,
                IsDealer = seatIndex == 0,
                Hand = new List<Card>()
            };
            game.AddPlayer(player);
            seatIndex++;
        }

        game.Initialize();
        return game;
    }

    public static BeziqueGame CreateCustom(Action<BeziqueGameBuilder> configure)
    {
        var builder = new BeziqueGameBuilder();
        configure(builder);
        return builder.Build();
    }
}

public class PlayerConfig
{
    public required string Name { get; init; }
    public bool IsBot { get; init; }
    public bool IsDealer { get; init; }
}

public class BeziqueGameBuilder
{
    private readonly List<PlayerConfig> _playerConfigs = new();
    private GameMode? _gameMode;
    private int? _winningScore;
    private int? _timerSeconds;

    public BeziqueGameBuilder WithPlayer(string name, bool isBot = false, bool isDealer = false)
    {
        _playerConfigs.Add(new PlayerConfig { Name = name, IsBot = isBot, IsDealer = isDealer });
        return this;
    }

    public BeziqueGameBuilder WithPlayers(params Action<PlayerConfigBuilder>[] playerBuilders)
    {
        foreach (var builder in playerBuilders)
        {
            var configBuilder = new PlayerConfigBuilder();
            builder(configBuilder);
            _playerConfigs.Add(configBuilder.Build());
        }
        return this;
    }

    public BeziqueGameBuilder WithGameMode(GameMode mode)
    {
        _gameMode = mode;
        return this;
    }

    public BeziqueGameBuilder WithWinningScore(int score)
    {
        _winningScore = score;
        return this;
    }

    public BeziqueGameBuilder WithTimer(int seconds)
    {
        _timerSeconds = seconds;
        return this;
    }

    public BeziqueGame Build()
    {
        if (_playerConfigs.Count < 2)
            throw new InvalidOperationException("At least 2 players required");

        return BeziqueGameManager.CreateGame(_playerConfigs.ToArray());
    }
}

public class PlayerConfigBuilder
{
    private string _name = "Player";
    private bool _isBot;
    private bool _isDealer;

    public PlayerConfigBuilder Named(string name)
    {
        _name = name;
        return this;
    }

    public PlayerConfigBuilder AsHuman()
    {
        _isBot = false;
        return this;
    }

    public PlayerConfigBuilder AsAI()
    {
        _isBot = true;
        return this;
    }

    public PlayerConfigBuilder AsDealer()
    {
        _isDealer = true;
        return this;
    }

    public PlayerConfig Build()
    {
        return new PlayerConfig
        {
            Name = _name,
            IsBot = _isBot,
            IsDealer = _isDealer
        };
    }
}
