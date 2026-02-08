using BeziqueCore.Interfaces;

namespace BeziqueCore;

public partial class Bezique : IStateMachine
{
    protected IBeziqueAdapter? _adapter;

    public void SetAdapter(IBeziqueAdapter adapter) => _adapter = adapter;
}


