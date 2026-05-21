using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

namespace Zone8.Multiplayer.ConnectionManagement.UI
{
    /// <summary>
    /// An individual Session UI in the list of available Sessions.
    /// </summary>
    public class SessionListItemUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _sessionNameText;
        [SerializeField] TextMeshProUGUI _sessionCountText;
        [SerializeField] Button _button;

        SessionUIMediator _sessionUIMediator;

        ISessionInfo _data;

        private void Awake()
        {
            _button.onClick.AddListener(OnClick);
        }
        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnClick);
        }

        public void SetData(ISessionInfo data, SessionUIMediator sessionUIMediator)
        {
            _sessionUIMediator = sessionUIMediator;
            _data = data;
            _sessionNameText.SetText(data.Name);
            _sessionCountText.SetText($"{data.MaxPlayers - data.AvailableSlots}/{data.MaxPlayers}");
        }

        private void OnClick()
        {
            _sessionUIMediator.JoinSessionRequest(_data);
        }
    }
}
