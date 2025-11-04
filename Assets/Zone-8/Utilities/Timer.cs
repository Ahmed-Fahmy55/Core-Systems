using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Zone8.Utilities
{
    public class Timer : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private GameObject _timeUIContainer;

        public UnityEvent TimerStarted;
        public UnityEvent TimerFinished;
        public UnityEvent<float> TimerTickedNormalized;


        private bool _isTimerRunning = false;
        private float _timerDuration;
        private float _timerElapsedTime;




        private void Update()
        {
            if (!_isTimerRunning) return;
            _timerElapsedTime += Time.deltaTime;
            Tick(_timerElapsedTime);
        }


        public void StartTimer(float duration)
        {
            _timerDuration = duration;
            ResetTimer();
            ResumeTimer();
            TimerStarted?.Invoke();
        }

        public void PauseTimer()
        {
            _isTimerRunning = false;
        }

        public void ResumeTimer()
        {
            _isTimerRunning = true;
        }

        public void ResetTimer()
        {
            _timerElapsedTime = 0;
            UpdateUI(_timerElapsedTime);
        }

        public virtual void HideUI()
        {
            if (_timeUIContainer) _timeUIContainer.SetActive(false);
        }

        protected virtual void UpdateUI(float time)
        {
            if (_timerText != null)
            {
                float remainingTime = _timerDuration - time;
                string minutes = Mathf.Floor(remainingTime / 60).ToString("00");
                string seconds = (remainingTime % 60).ToString("00");
                _timerText.text = $"{minutes}:{seconds}";
            }
        }

        private void Tick(float newValue)
        {
            TimerTickedNormalized?.Invoke(newValue / _timerDuration);
            UpdateUI(newValue);
            if (newValue >= _timerDuration)
            {
                UpdateUI(_timerDuration);
                _isTimerRunning = false;
                TimerFinished?.Invoke();
            }
        }

    }
}

