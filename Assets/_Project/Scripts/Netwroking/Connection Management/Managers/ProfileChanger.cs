using Zone8.UnityServices.Auth;
using System.Threading.Tasks;
using UnityEngine;

namespace Zone8.Multiplayer.ConnectionManagement
{
    public class ProfileChanger : MonoBehaviour
    {
        bool _isChanging = false;
        private void Start()
        {
            Debug.Log("PlayerID is:" + AuthenticationServiceFacade.GetPlayerId());
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.S)) _ = SwitchProfile();
        }

        async Task SwitchProfile()
        {
            if (_isChanging) return;
            _isChanging = true;
            await AuthenticationServiceFacade.SwitchProfileAndReSignInAsync(Random.Range(0, 50).ToString());
            Debug.Log("PlayerID is:" + AuthenticationServiceFacade.GetPlayerId());
            _isChanging = false;
        }
    }
}
