using BeziqueCore.Interfaces;

namespace BeziqueGame.CLI
{
    /// <summary>
    /// Null implementation of IPlayerTimer for local multiplayer.
    /// No timer needed since all players share the same device.
    /// </summary>
    public class NullPlayerTimer : IPlayerTimer
    {
        public void Start()
        {
            // No-op for local multiplayer
        }

        public void Stop()
        {
            // No-op for local multiplayer
        }

        public void Reset()
        {
            // No-op for local multiplayer
        }

        public bool IsRunning => false;
        public int RemainingSeconds => int.MaxValue;
        public bool IsExpired => false;
    }
}
