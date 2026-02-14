namespace BeziqueCore.Interfaces;

public interface IGameCard
{
    byte CardValue { get; }
    byte DeckIndex { get; }
    byte CardId { get; }
    bool IsJoker { get; }
}
