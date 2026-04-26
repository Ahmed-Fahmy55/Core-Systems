using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Zone8.ImprovedTimers
{
    public class Demo : MonoBehaviour
    {
        [Header("Timer Settings")]
        [SerializeField] float _totalTime = 10f;
        [SerializeField] float _interval = 1f;
        [SerializeField] int _frequency = 1;

        [Header("Timers")]
        [SerializeField] TextMeshProUGUI _countdownTimerText;
        [SerializeField] TextMeshProUGUI _intervalTimerText;
        [SerializeField] TextMeshProUGUI _frequencyTimerText;
        [SerializeField] TextMeshProUGUI _stopWatchTimerText;


        Timer _countdownTtimer;
        Timer _intervalTimer;
        Timer _frequencyTimer;
        Timer _stopWatchTimer;

        private void Start()
        {
            _countdownTtimer = new CountdownTimer(_totalTime);
            _intervalTimer = new IntervalTimer(_totalTime, _interval);
            _frequencyTimer = new FrequencyTimer(_frequency);
            _stopWatchTimer = new StopwatchTimer();
        }

        private void Update()
        {
            if (_intervalTimer.IsRunning) _intervalTimerText.text = $"Interval Timer: {_intervalTimer.CurrentTime:F2}s";
            if (_countdownTtimer.IsRunning) _countdownTimerText.text = $"Countdown Timer: {_countdownTtimer.CurrentTime:F2}s";
            if (_frequencyTimer.IsRunning) _frequencyTimerText.text = $"Frequency Timer: {_frequencyTimer.CurrentTime:F2}s";
            if (_stopWatchTimer.IsRunning) _stopWatchTimerText.text = $"Stopwatch Timer: {_stopWatchTimer.CurrentTime:F2}s";
        }

        [Button]
        void PlayTimers()
        {
            _countdownTtimer.Start();
            _intervalTimer.Start();
            _frequencyTimer.Start();
            _stopWatchTimer.Start();
        }

        [Button]
        void StopTimers()
        {
            _countdownTtimer.Stop();
            _intervalTimer.Stop();
            _frequencyTimer.Stop();
            _stopWatchTimer.Stop();
        }

        [Button]
        void PauseTImers()
        {
            _countdownTtimer.Pause();
            _intervalTimer.Pause();
            _frequencyTimer.Pause();
            _stopWatchTimer.Pause();
        }

        [Button]
        void ResumeTiemrs()
        {
            _countdownTtimer.Resume();
            _intervalTimer.Resume();
            _frequencyTimer.Resume();
            _stopWatchTimer.Resume();
        }

        private void OnDestroy()
        {
            _countdownTtimer.Dispose();
            _intervalTimer.Dispose();
            _frequencyTimer.Dispose();
            _stopWatchTimer.Dispose();
        }
    }

}
