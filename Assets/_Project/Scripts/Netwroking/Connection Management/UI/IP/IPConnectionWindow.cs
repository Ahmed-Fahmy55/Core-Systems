using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using Zone8.Events;

namespace Zone8.Multiplayer.ConnectionManagement.UI
{
    public class IPConnectionWindow : MonoBehaviour
    {
        [SerializeField] CanvasGroup _canvasGroup;

        [SerializeField] TextMeshProUGUI _titleText;


        IPUIMediator _iPUIMediator;

        EventBinding<ConnectionMessageEvent> _connectionStatusBinding;


        void Awake()
        {
            _iPUIMediator = FindAnyObjectByType<IPUIMediator>();
            _connectionStatusBinding = new(OnConnectStatusMessage);
            EventBus<ConnectionMessageEvent>.Register(_connectionStatusBinding);

            Hide();
        }

        void OnDestroy()
        {
            EventBus<ConnectionMessageEvent>.Deregister(_connectionStatusBinding);

        }

        void OnConnectStatusMessage(ConnectionMessageEvent connectStatus)
        {
            CancelConnectionWindow();
            _iPUIMediator.DisableSignInSpinner();
        }

        void Show()
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
        }

        void Hide()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
        }

        public void ShowConnectingWindow()
        {
            void OnTimeElapsed()
            {
                Hide();
                _iPUIMediator.DisableSignInSpinner();
            }

            var utp = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            var maxConnectAttempts = utp.MaxConnectAttempts;
            var connectTimeoutMS = utp.ConnectTimeoutMS;
            StartCoroutine(DisplayUTPConnectionDuration(maxConnectAttempts, connectTimeoutMS, OnTimeElapsed));

            Show();
        }

        public void CancelConnectionWindow()
        {
            Hide();
            StopAllCoroutines();
        }

        IEnumerator DisplayUTPConnectionDuration(int maxReconnectAttempts, int connectTimeoutMS, Action endAction)
        {
            var connectionDuration = maxReconnectAttempts * connectTimeoutMS / 1000f;

            var seconds = Mathf.CeilToInt(connectionDuration);

            while (seconds > 0)
            {
                _titleText.text = $"Connecting...\n{seconds}";
                yield return new WaitForSeconds(1f);
                seconds--;
            }
            _titleText.text = "Connecting...";

            endAction();
        }

        // invoked by UI cancel button
        public void OnCancelJoinButtonPressed()
        {
            CancelConnectionWindow();
            _iPUIMediator.JoiningWindowCancelled();
        }
    }
}
