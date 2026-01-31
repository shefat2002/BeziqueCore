namespace BeziqueCore.Core.Application.Interfaces
{
    public interface IPlayerTimer
    {
        void Start();
        void Stop();
        void Reset();
        bool IsRunning { get; }
        int RemainingSeconds { get; }
    }
}
