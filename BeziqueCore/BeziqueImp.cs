using BeziqueCore.Interfaces;

namespace BeziqueCore;

public partial class Bezique : IStateMachine
{
    protected IBeziqueAdapter? _adapter;

    public void SetAdapter(IBeziqueAdapter adapter) => _adapter = adapter;
}

internal class BeziqueAdapter : IBeziqueAdapter
{
    public void CheckPlayerCount()
    {
        throw new NotImplementedException();
    }

    public void DealThreeCards()
    {
        throw new NotImplementedException();
    }

    public void FlipCard()
    {
        throw new NotImplementedException();
    }

    public void Add10PointsToDealer()
    {
        throw new NotImplementedException();
    }
}
