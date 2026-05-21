using Michsky.UI.Heat;
using TMPro;
using UnityEngine;

namespace Zone8.Multiplayer.ConnectionManagement.UI
{
    public class IPJoiningUI : MonoBehaviour
    {
        [SerializeField] TMP_InputField _ipInputField;
        [SerializeField] TMP_InputField _portInputField;
        [SerializeField] ButtonManager _joinButton;


        IPUIMediator _ipuiMediator;

        void Awake()
        {
            _ipuiMediator = FindAnyObjectByType<IPUIMediator>();
            _ipInputField.text = IPUIMediator.k_DefaultIP;
            _portInputField.text = IPUIMediator.k_DefaultPort.ToString();

            _joinButton.onClick.AddListener(OnJoinButtonPressed);
            _ipInputField.onValueChanged.AddListener(_ipInputField => SanitizeIPInputText());
            _portInputField.onValueChanged.AddListener(_portInputField => SanitizePortText());
        }


        private void OnJoinButtonPressed()
        {
            _ipuiMediator.JoinWithIP(_ipInputField.text, _portInputField.text);
        }

        /// <summary>
        /// Added to the InputField component's OnValueChanged callback for the Room/IP UI text.
        /// </summary>
        private void SanitizeIPInputText()
        {
            _ipInputField.text = IPUIMediator.SanitizeIP(_ipInputField.text);
            _joinButton.Interactable(IPUIMediator.AreIpAddressAndPortValid(_ipInputField.text, _portInputField.text));
        }

        /// <summary>
        /// Added to the InputField component's OnValueChanged callback for the Port UI text.
        /// </summary>
        private void SanitizePortText()
        {
            _portInputField.text = IPUIMediator.SanitizePort(_portInputField.text);
            _joinButton.Interactable(IPUIMediator.AreIpAddressAndPortValid(_ipInputField.text, _portInputField.text));
        }
    }
}
