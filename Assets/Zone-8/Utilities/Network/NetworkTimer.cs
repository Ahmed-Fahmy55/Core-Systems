using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Zone8.Utilities
{
    public class NetworkTimer : NetworkBehaviour
    {
        [SerializeField] private TextMeshProUGUI _timerText;

        public UnityEvent TimerStarted;
        public UnityEvent TimerFinished;
        public UnityEvent<float> TimerTickedNormalized;


        private bool _isTimerRunning = false;
        private NetworkVariable<float> _timerDuration = new(0);
        private NetworkVariable<float> _timerElapsedTime = new(0);


        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _timerElapsedTime.OnValueChanged += OnTimerElapsedTimeChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _timerElapsedTime.OnValueChanged -= OnTimerElapsedTimeChanged;
        }



        private void Update()
        {
            if (!IsServer) return;
            if (!_isTimerRunning) return;

            _timerElapsedTime.Value += Time.deltaTime;
        }


        public void StartTimer(float duration)
        {
            if (!IsServer) return;
            _timerDuration.Value = duration;
            ResetTimer();
            ResumeTimer();
            TimerStarted?.Invoke();

        }

        public void PauseTimer()
        {
            if (!IsServer) return;
            _isTimerRunning = false;
        }

        public void ResumeTimer()
        {
            if (!IsServer) return;
            _isTimerRunning = true;
        }

        public void ResetTimer()
        {
            if (!IsServer) return;
            _timerElapsedTime.Value = 0;
            UpdateUI(_timerElapsedTime.Value);
        }

        public virtual void HideUI()
        {
            if (_timerText) _timerText.transform.parent.gameObject.SetActive(false);
        }

        protected virtual void UpdateUI(float time)
        {
            if (_timerText != null)
            {
                float remainingTime = _timerDuration.Value - time;
                string minutes = Mathf.Floor(remainingTime / 60).ToString("00");
                string seconds = (remainingTime % 60).ToString("00");
                _timerText.text = $"{minutes}:{seconds}";
            }
        }

        private void OnTimerElapsedTimeChanged(float previousValue, float newValue)
        {
            TimerTickedNormalized?.Invoke(newValue / _timerDuration.Value);
            UpdateUI(newValue);
            if (_timerElapsedTime.Value >= _timerDuration.Value)
            {
                UpdateUI(_timerDuration.Value);
                _isTimerRunning = false;
                TimerFinished?.Invoke();
            }
        }
    }


}
