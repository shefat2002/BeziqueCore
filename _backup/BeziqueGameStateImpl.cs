using BeziqueCore.Interfaces;

namespace BeziqueCore;
 
public partial class BeziqueGameState
{
    private readonly IBeziqueAdapter _adapter;
    
    public BeziqueGameState(IBeziqueAdapter adapter)
    {
        _adapter = adapter;
    }
}