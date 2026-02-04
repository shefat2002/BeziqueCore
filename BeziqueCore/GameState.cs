namespace BeziqueCore;

public enum GameState : byte
{
    Deal,
    Play,
    Meld,
    NewTrick,
    L9Play,
    L9NewTrick,
    RoundEnd,
    GameOver
}
