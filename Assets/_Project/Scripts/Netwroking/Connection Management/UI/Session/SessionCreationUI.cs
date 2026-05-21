using Michsky.UI.Heat;
using TMPro;
using UnityEngine;

namespace Zone8.Multiplayer.ConnectionManagement.UI
{
    public class SessionCreationUI : MonoBehaviour
    {
        [SerializeField] TMP_InputField _sessionNameInputField;
        [SerializeField] GameObject _loadingIndicatorObject;
        [SerializeField] ButtonManager _createButton;
        SessionUIMediator _sessionUIMediator;


        void Awake()
        {
            _sessionUIMediator = FindAnyObjectByType<SessionUIMediator>();
            _createButton.onClick.AddListener(OnCreateClick);
        }
        private void OnDestroy()
        {
            _createButton.onClick.RemoveListener(OnCreateClick);
        }

        private void OnCreateClick()
        {
            _sessionUIMediator.CreateSessionRequest(_sessionNameInputField.text, false);
        }
    }
}
