using BeziqueCore.Core.Domain.Entities;

namespace BeziqueCore.Core.Application.Interfaces;

public interface IPlayerActions
{
    void PlayCard(Player player, Card card);
    void DeclareMeld(Player player, Meld meld);
    void DrawCard(Player player);
    void SwitchSevenOfTrump(Player player);
    void SkipMeld(Player player);
}
