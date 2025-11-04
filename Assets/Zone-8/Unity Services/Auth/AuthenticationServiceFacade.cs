using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Zone8.Events;
using Zone8.UnityServices.infastructure;
using Zone8.Utilities;

namespace Zone8.UnityServices.Auth
{
    public static class AuthenticationServiceFacade
    {

        public static InitializationOptions GenerateAuthenticationOptions(string profile)
        {
            try
            {
                var unityAuthenticationInitOptions = new InitializationOptions();
                if (profile.Length > 0)
                {
                    unityAuthenticationInitOptions.SetProfile(profile);
                }

                return unityAuthenticationInitOptions;
            }
            catch (Exception e)
            {
                var reason = e.InnerException == null ? e.Message : $"{e.Message} ({e.InnerException.Message})";
                EventBus<UnityServiceErrorMessage>.Raise(new UnityServiceErrorMessage("Authentication Error", reason, UnityServiceErrorMessage.Service.Authentication, e));
                throw;
            }
        }

        public static async Task InitializeAndSignInAsync(InitializationOptions initializationOptions)
        {
            try
            {
                await Unity.Services.Core.UnityServices.InitializeAsync(initializationOptions);

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }
            }
            catch (Exception e)
            {
                var reason = e.InnerException == null ? e.Message : $"{e.Message} ({e.InnerException.Message})";
                Logger.Log("Authentication Error " + reason);
                EventBus<UnityServiceErrorMessage>.Raise(new UnityServiceErrorMessage("Authentication Error", reason, UnityServiceErrorMessage.Service.Authentication, e));
            }
        }

        public static async Task SwitchProfileAndReSignInAsync(string profile)
        {
            if (AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SignOut();
            }

            AuthenticationService.Instance.SwitchProfile(profile);

            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            catch (Exception e)
            {
                var reason = e.InnerException == null ? e.Message : $"{e.Message} ({e.InnerException.Message})";
                Logger.Log("Authentication Error " + reason);
                EventBus<UnityServiceErrorMessage>.Raise(new UnityServiceErrorMessage("Authentication Error", reason, UnityServiceErrorMessage.Service.Authentication, e));
            }
        }

        public static async Task<bool> EnsurePlayerIsAuthorized()
        {
            if (AuthenticationService.Instance.IsAuthorized)
            {
                return true;
            }

            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                return true;
            }
            catch (AuthenticationException e)
            {
                var reason = e.InnerException == null ? e.Message : $"{e.Message} ({e.InnerException.Message})";
                EventBus<UnityServiceErrorMessage>.Raise(new UnityServiceErrorMessage("Authentication Error", reason, UnityServiceErrorMessage.Service.Authentication, e));
                Logger.Log("Authentication Error " + reason);
                //not rethrowing for authentication exceptions - any failure to authenticate is considered "handled failure"
                return false;
            }
            catch (Exception e)
            {
                //all other exceptions should still bubble up as unhandled ones
                var reason = e.InnerException == null ? e.Message : $"{e.Message} ({e.InnerException.Message})";
                Logger.Log("Authentication Error " + reason);
                EventBus<UnityServiceErrorMessage>.Raise(new UnityServiceErrorMessage("Authentication Error", reason, UnityServiceErrorMessage.Service.Authentication, e));
                return false;
            }
        }

        public static async Task<string> GetPlayerId()
        {
            if (Unity.Services.Core.UnityServices.State != ServicesInitializationState.Initialized)
            {
                Logger.LogError("Unity Services not initialized, using device GUID as player ID");
                return ClientPrefs.GetGuid();
            }

            bool isAuthorized = await EnsurePlayerIsAuthorized();
            return isAuthorized ? AuthenticationService.Instance.PlayerId : ClientPrefs.GetGuid();
        }
    }
}
