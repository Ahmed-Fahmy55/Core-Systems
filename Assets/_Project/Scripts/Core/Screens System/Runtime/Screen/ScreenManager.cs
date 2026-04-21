using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Zone8.Screens
{
    public class ScreenManager : SerializedMonoBehaviour
    {
        public event Action<Screen, string> ScreenChanged;

        [SerializeField] private Dictionary<EScreen, Screen> _screenInstances;
        [SerializeField] private EScreen _startScreen;
        [SerializeField] private float _startScreenDelay = 2;

        private Screen _currentScreen;

        // Buffer queue for screen requests
        private readonly Queue<ScreenRequest> _screenRequestQueue = new Queue<ScreenRequest>();
        private bool _isProcessingQueue = false;

        private IEnumerator Start()
        {
            if (_startScreen != null)
            {
                yield return new WaitForSeconds(_startScreenDelay);
                ShowScreen(_startScreen);
            }
        }

        public void ShowScreen(EScreen screen,
             bool hideCurrent = true,
             bool autoHide = false,
             float secondsToHide = 1,
             Action OnActiveScreenHide = null,
             Action OnNewScreenShow = null,
             Action OnAutoHidden = null)
        {
            _screenRequestQueue.Enqueue(new ScreenRequest
            {
                Screen = screen,
                HideCurrent = hideCurrent,
                AutoHide = autoHide,
                SecondsToHide = secondsToHide,
                OnPreviousHidden = OnActiveScreenHide,
                OnAutoHidden = OnAutoHidden,
                OnNewShown = OnNewScreenShow
            });

            if (!_isProcessingQueue)
                _ = ProcessQueue();
        }

        public void ShowScreen(string screen,
            bool hideCurrent = true,
            bool autoHide = false,
            float secondsToHide = 1,
            Action OnActiveScreenHide = null,
            Action OnNewScreenShow = null,
            Action OnAutoHidden = null)
        {
            EScreen targetScreen = GetScreenByName(screen);
            if (targetScreen == null)
            {
                Logger.LogError($"Screen with name {screen} not found.");
                return;
            }

            _screenRequestQueue.Enqueue(new ScreenRequest
            {
                Screen = targetScreen,
                HideCurrent = hideCurrent,
                AutoHide = autoHide,
                SecondsToHide = secondsToHide,
                OnPreviousHidden = OnActiveScreenHide,
                OnAutoHidden = OnAutoHidden,
                OnNewShown = OnNewScreenShow
            });

            if (!_isProcessingQueue)
                _ = ProcessQueue();
        }

        public void ShowScreenSO(EScreen screen)
        {
            if (screen == null)
            {
                Logger.LogError("Screen ScriptableObject is null.");
                return;
            }

            ShowScreen(screen);
        }

        public async Awaitable HideScreenAsync(EScreen screen)
        {
            if (screen == null || !_screenInstances.TryGetValue(screen, out var screenInstance))
            {
                Logger.LogError($"Screen instance not found for {screen?.ScreenName ?? "null"}.");
                return;
            }

            if (_currentScreen == screenInstance)
            {
                _currentScreen = null;
                await screenInstance.Hide();
            }
        }

        public void HideScreen(EScreen screen)
        {
            if (screen == null)
            {
                Logger.LogError("Screen ScriptableObject is null.");
                return;
            }

            _ = HideScreenAsync(screen);
        }

        private EScreen GetScreenByName(string screen)
        {
            foreach (var screenInstance in _screenInstances)
            {
                if (screenInstance.Key.ScreenName == screen) return screenInstance.Key;
            }
            return null;
        }

        private async Awaitable ProcessQueue()
        {
            _isProcessingQueue = true;
            try
            {
                while (_screenRequestQueue.Count > 0)
                {
                    var request = _screenRequestQueue.Dequeue();
                    await ShowInternal(request);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error while processing screen {ex}");
            }
            finally
            {
                _isProcessingQueue = false;
            }
        }

        private async Awaitable ShowInternal(ScreenRequest request)
        {
            if (request.Screen == null || !_screenInstances.TryGetValue(request.Screen, out Screen target))
            {
                Logger.LogError($"Screen instance not found for {request.Screen?.ScreenName ?? "null"}. Please ensure it is registered in the ScreenManager.");
                return;
            }
            if (target == null)
            {
                Logger.LogError("Target screen is null");
                return;
            }
            if (target == _currentScreen) return;


            if (_currentScreen != null && request.HideCurrent)
            {
                await _currentScreen.Hide();
                request.OnPreviousHidden?.Invoke();
            }

            await target.Show();
            request.OnNewShown?.Invoke();
            ScreenChanged?.Invoke(target, request.Screen.ScreenName);
            _currentScreen = target;

            // Auto hide current screen
            if (request.AutoHide)
            {
                await Awaitable.WaitForSecondsAsync(request.SecondsToHide);
                await target.Hide();
                request.OnAutoHidden?.Invoke();
                _currentScreen = null;
            }
        }

        private struct ScreenRequest
        {
            public EScreen Screen;
            public bool HideCurrent;
            public bool AutoHide;
            public float SecondsToHide;
            public Action OnPreviousHidden;
            public Action OnAutoHidden;
            public Action OnNewShown;
        }
    }
}
