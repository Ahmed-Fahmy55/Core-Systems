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

        /// <summary>
        /// Buffers the screen show request and processes them in order.
        /// </summary>
        public void ShowScreen(EScreen screen, Action OnActiveScreenHide = null, Action OnNewScreenShow = null)
        {
            _screenRequestQueue.Enqueue(new ScreenRequest
            {
                Screen = screen,
                OnActiveScreenHide = OnActiveScreenHide,
                OnNewScreenShow = OnNewScreenShow
            });

            if (!_isProcessingQueue)
                _ = ProcessQueue();
        }

        public void ShowScreen(string screen, Action OnActiveScreenHide = null, Action OnNewScreenShow = null)
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
                OnActiveScreenHide = OnActiveScreenHide,
                OnNewScreenShow = OnNewScreenShow
            });

            if (!_isProcessingQueue)
                _ = ProcessQueue();
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
            while (_screenRequestQueue.Count > 0)
            {
                var request = _screenRequestQueue.Dequeue();
                await ShowScreenInternal(request.Screen, request.OnActiveScreenHide, request.OnNewScreenShow);
            }
            _isProcessingQueue = false;
        }

        private async Awaitable ShowScreenInternal(EScreen screen, Action OnActiveScreenHide, Action OnNewScreenShow)
        {
            if (screen == null || !_screenInstances.TryGetValue(screen, out Screen screenInstance))
            {
                Logger.LogError($"Screen instance not found for {screen?.ScreenName ?? "null"}. Please ensure it is registered in the ScreenManager.");
                return;
            }

            if (screenInstance == null)
            {
                Logger.LogError("Target screen is null");
                return;
            }

            if (screenInstance == _currentScreen)
            {
                Logger.Log($"Screen {screen.ScreenName} is already active.");
                return;
            }

            // Hide the current screen if required
            if (_currentScreen != null && screenInstance.HideCurrent)
            {
                await _currentScreen.Hide();
                OnActiveScreenHide?.Invoke();
            }

            await screenInstance.Show();
            OnNewScreenShow?.Invoke();
            ScreenChanged?.Invoke(screenInstance, screen.ScreenName);
            _currentScreen = screenInstance;

            // Auto hide current screen
            if (screenInstance.AutoHide)
            {
                await screenInstance.Hide();
                _currentScreen = null;
            }
        }

        /// <summary>
        /// Hides the specified screen if it is active.
        /// </summary>
        public async Awaitable HideScreen(EScreen screen)
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

        public void ShowScreenSO(EScreen screen)
        {
            if (screen == null)
            {
                Logger.LogError("Screen ScriptableObject is null.");
                return;
            }

            ShowScreen(screen);
        }

        public void HideScreenSO(EScreen screen)
        {
            if (screen == null)
            {
                Logger.LogError("Screen ScriptableObject is null.");
                return;
            }

            _ = HideScreen(screen);
        }

        private class ScreenRequest
        {
            public EScreen Screen;
            public Action OnActiveScreenHide;
            public Action OnNewScreenShow;
        }
    }
}
