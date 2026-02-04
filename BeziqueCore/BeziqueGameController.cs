namespace BeziqueCore;

public class BeziqueGameController
{
    public Player[] Players { get; private set; }
    public GameContext Context { get; set; }
    public List<Card> PlayedCards { get; private set; }
    public GameState CurrentState { get; set; }
    public byte PlayerCount => (byte)Players.Length;
    public ushort TargetScore { get; private set; }

    public void Initialize(GameConfig config)
    {
        TargetScore = config.TargetScore;
        GameInitializer.InitializeGame(config, out var players, out var context);
        Players = players;
        Context = context;
        PlayedCards = new List<Card>();
        CurrentState = GameState.Play;
    }

    public bool PlayCard(Card card)
    {
        if (CurrentState != GameState.Play && CurrentState != GameState.L9Play)
            return false;

        if (Context.CurrentPhase == GamePhase.Phase1_Normal)
        {
            if (!PlayStateHandler.ValidateAndPlayCardPhase1(
                Players[Context.CurrentTurnPlayer],
                card,
                PlayedCards,
                Context.CurrentTurnPlayer))
                return false;

            if (PlayedCards.Count == PlayerCount)
            {
                CurrentState = GameState.Meld;
                return true;
            }

            AdvanceTurn();
            return true;
        }

        var leadSuit = PlayedCards.Count > 0 ? PlayedCards[0].Suit : Suit.None;
        var currentWinner = PlayedCards.Count > 0 ? PlayedCards[0] : (Card?)null;

        if (Context.CurrentPhase == GamePhase.Phase2_Last9)
        {
            if (!L9PlayStateHandler.ValidateAndPlayCardPhase2(
                Players[Context.CurrentTurnPlayer],
                card,
                PlayedCards,
                Context.CurrentTurnPlayer,
                leadSuit,
                currentWinner,
                Context.TrumpSuit))
                return false;

            if (PlayedCards.Count == PlayerCount)
            {
                ResolveTrick();
                return true;
            }

            AdvanceTurn();
            return true;
        }

        return false;
    }

    public bool DeclareMeld(Card[] cards, MeldType meldType)
    {
        if (CurrentState != GameState.Meld)
            return false;

        if (!MeldStateHandler.DeclareMeld(
            Players[Context.CurrentTurnPlayer],
            cards,
            meldType,
            Context.TrumpSuit))
            return false;

        if (Players[Context.CurrentTurnPlayer].Hand.Count == 0)
        {
            CurrentState = GameState.Play;
        }

        return true;
    }

    public void ResolveTrick()
    {
        var playerIndices = new int[PlayerCount];
        for (int i = 0; i < PlayerCount; i++) playerIndices[i] = i;

        bool isFinalTrick = Context.DrawDeck.Count == 0 && AllHandsEmpty();
        int winnerId = TrickResolverHandler.ResolveTrick(
            PlayedCards,
            Players,
            playerIndices,
            Context.TrumpSuit,
            isFinalTrick);

        Context.LastTrickWinner = winnerId;

        if (isFinalTrick)
        {
            CurrentState = GameState.RoundEnd;
            return;
        }

        if (Context.DrawDeck.Count > 0)
        {
            DrawCards();
            CurrentState = GameState.NewTrick;
        }
        else
        {
            TransitionToPhase2();
        }
    }

    public void StartNewTrick()
    {
        if (CurrentState != GameState.NewTrick && CurrentState != GameState.L9NewTrick)
            return;

        int currentTurn = Context.CurrentTurnPlayer;
        NewTrickHandler.StartNewTrick(PlayedCards, Context.LastTrickWinner, ref currentTurn);
        Context.CurrentTurnPlayer = currentTurn;
        CurrentState = Context.CurrentPhase == GamePhase.Phase2_Last9 ? GameState.L9Play : GameState.Play;
    }

    public int EndRound()
    {
        if (CurrentState != GameState.RoundEnd)
            return -1;

        int roundWinnerId = RoundEndHandler.EndRound(Players, TargetScore);

        if (RoundEndHandler.CheckGameOver(Players, TargetScore))
        {
            int gameWinnerId = GetGameWinner();
            CurrentState = GameState.GameOver;
            return gameWinnerId;
        }

        StartNewRound();
        return roundWinnerId;
    }

    private int GetGameWinner()
    {
        int highestScore = int.MinValue;
        int winnerId = -1;

        for (int i = 0; i < Players.Length; i++)
        {
            if (Players[i].TotalScore > highestScore)
            {
                highestScore = Players[i].TotalScore;
                winnerId = i;
            }
        }

        return winnerId;
    }

    private void StartNewRound()
    {
        GameInitializer.InitializeGame(new GameConfig
        {
            PlayerCount = PlayerCount,
            DeckCount = 2,
            TargetScore = TargetScore
        }, out var players, out var context);

        for (int i = 0; i < Players.Length; i++)
        {
            players[i].TotalScore = Players[i].TotalScore;
        }

        Players = players;
        Context = context;
        PlayedCards.Clear();
        CurrentState = GameState.Play;
    }

    private void DrawCards()
    {
        for (int i = 0; i < Players.Length; i++)
        {
            if (Context.DrawDeck.Count > 0)
            {
                Players[i].Hand.Add(Context.DrawDeck.Pop());
            }
        }
    }

    private void TransitionToPhase2()
    {
        Context.CurrentPhase = GamePhase.Phase2_Last9;
        CurrentState = GameState.NewTrick;
    }

    private void AdvanceTurn()
    {
        Context.CurrentTurnPlayer = TurnManager.AdvanceTurn(Context.CurrentTurnPlayer, PlayerCount);
    }

    private bool AllHandsEmpty()
    {
        for (int i = 0; i < Players.Length; i++)
        {
            if (Players[i].Hand.Count > 0 || Players[i].TableCards.Count > 0)
                return false;
        }
        return true;
    }
}
