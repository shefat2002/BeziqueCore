namespace BeziqueCore;

public interface IStateMachine
{
    public void Start();
    public void DispatchEvent(Bezique.EventId eventId);
}