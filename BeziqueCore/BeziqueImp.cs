using BeziqueCore.Interfaces;

namespace BeziqueCore;

public partial class Bezique
{

    protected IBeziqueAdapter? _adapter;

    public void SetAdapter(IBeziqueAdapter adapter)
    {
        _adapter = adapter;
    }
}
