using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#if UNITY_EDITOR
using System.Security.Cryptography;
using System.Text;
#endif

using UnityEngine;

namespace Zone8.Utilities
{
    public class ProfileManager : Singleton<ProfileManager>
    {
        public const string AuthProfileCommandLineArg = "-AuthProfile";
        private string _profile = null;

        public string Profile
        {
            get
            {
                if (_profile == null)
                {
                    _profile = GetProfile();
                }

                return _profile;
            }
            set
            {
                _profile = value;
                ProfileChanged?.Invoke();
            }
        }

        public event Action ProfileChanged;

        private List<string> mavailableProfiles;

        public ReadOnlyCollection<string> AvailableProfiles
        {
            get
            {
                if (mavailableProfiles == null)
                {
                    LoadProfiles();
                }

                return mavailableProfiles.AsReadOnly();
            }
        }

        public void CreateProfile(string profile)
        {
            mavailableProfiles.Add(profile);
            SaveProfiles();
        }

        public void DeleteProfile(string profile)
        {
            mavailableProfiles.Remove(profile);
            SaveProfiles();
        }

        private string GetProfile()
        {
            var arguments = Environment.GetCommandLineArgs();
            for (int i = 0; i < arguments.Length; i++)
            {
                if (arguments[i] == AuthProfileCommandLineArg)
                {
                    var profileId = arguments[i + 1];
                    return profileId;
                }
            }

#if UNITY_EDITOR

            // When running in the Editor make a unique ID from the Application.dataPath.
            // This will work for cloning projects manually, or with Virtual Projects.
            // Since only a single instance of the Editor can be open for a specific
            // dataPath, uniqueness is ensured.
            var hashedBytes = new MD5CryptoServiceProvider()
                .ComputeHash(Encoding.UTF8.GetBytes(Application.dataPath));
            Array.Resize(ref hashedBytes, 16);
            // Authentication service only allows profile names of maximum 30 characters. We're generating a GUID based
            // on the project's path. Truncating the first 30 characters of said GUID string suffices for uniqueness.
            return new Guid(hashedBytes).ToString("N")[..30];
#else
            return "";
#endif
        }

        private void LoadProfiles()
        {
            mavailableProfiles = new List<string>();
            var loadedProfiles = ClientPrefs.GetAvailableProfiles();
            foreach (var profile in loadedProfiles.Split(',')) // this works since we're sanitizing our input strings
            {
                if (profile.Length > 0)
                {
                    mavailableProfiles.Add(profile);
                }
            }
        }

        private void SaveProfiles()
        {
            var profilesToSave = "";
            foreach (var profile in mavailableProfiles)
            {
                profilesToSave += profile + ",";
            }
            ClientPrefs.SetAvailableProfiles(profilesToSave);
        }

    }
}
