using BeziqueCore.Core.Application.Interfaces;

namespace BeziqueCore.Core.Infrastructure.Timing
{
    public class PlayerTimer : IPlayerTimer
    {
        private readonly System.Timers.Timer _timer;
        private int _remainingSeconds;
        private const int TurnDurationSeconds = 15;

        public bool IsRunning { get; private set; }
        public int RemainingSeconds => _remainingSeconds;

        public PlayerTimer()
        {
            _remainingSeconds = TurnDurationSeconds;
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += OnTimerElapsed;
        }

        public void Start()
        {
            _remainingSeconds = TurnDurationSeconds;
            IsRunning = true;
            _timer.Start();
        }

        public void Stop()
        {
            IsRunning = false;
            _timer.Stop();
        }

        public void Reset()
        {
            Stop();
            _remainingSeconds = TurnDurationSeconds;
        }

        private void OnTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (_remainingSeconds > 0)
            {
                _remainingSeconds--;
            }
            else
            {
                Stop();
            }
        }
    }
}
