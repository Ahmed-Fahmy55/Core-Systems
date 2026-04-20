using Zone8.Utilities;
using System;
using System.Collections.Generic;
using Unity.Services.Multiplayer;

namespace Zone8.UnityServices.Sessions
{
    /// <summary>
    /// Data for a local session user instance. This will update data and is observed to know when to push local user
    /// changes to the entire session.
    /// </summary>
    [Serializable]
    public class LocalSessionUser : Singleton<LocalSessionUser>
    {
        UserData _userData;

        public event Action<LocalSessionUser> Changed;

        public LocalSessionUser()
        {
            _userData = new UserData(isHost: false, displayName: null, id: null);
        }

        public struct UserData
        {
            public bool IsHost { get; set; }
            public string DisplayName { get; set; }
            public string ID { get; set; }

            public UserData(bool isHost, string displayName, string id)
            {
                IsHost = isHost;
                DisplayName = displayName;
                ID = id;
            }
        }

        public void ResetState()
        {
            _userData = new UserData(false, _userData.DisplayName, _userData.ID);
        }

        /// <summary>
        /// Used for limiting costly OnChanged actions to just the members which actually changed.
        /// </summary>
        [Flags]
        public enum UserMembers
        {
            IsHost = 1,
            DisplayName = 2,
            ID = 4,
        }

        UserMembers m_LastChanged;

        public bool IsHost
        {
            get => _userData.IsHost;
            set
            {
                if (_userData.IsHost != value)
                {
                    _userData.IsHost = value;
                    m_LastChanged = UserMembers.IsHost;
                    OnChanged();
                }
            }
        }

        public string DisplayName
        {
            get => _userData.DisplayName;
            set
            {
                if (_userData.DisplayName != value)
                {
                    _userData.DisplayName = value;
                    m_LastChanged = UserMembers.DisplayName;
                    OnChanged();
                }
            }
        }

        public string ID
        {
            get => _userData.ID;
            set
            {
                if (_userData.ID != value)
                {
                    _userData.ID = value;
                    m_LastChanged = UserMembers.ID;
                    OnChanged();
                }
            }
        }

        public void CopyDataFrom(LocalSessionUser session)
        {
            var data = session._userData;
            var lastChanged = // Set flags just for the members that will be changed.
                (_userData.IsHost == data.IsHost ? 0 : (int)UserMembers.IsHost) |
                (_userData.DisplayName == data.DisplayName ? 0 : (int)UserMembers.DisplayName) |
                (_userData.ID == data.ID ? 0 : (int)UserMembers.ID);

            if (lastChanged == 0) // Ensure something actually changed.
            {
                return;
            }

            _userData = data;
            m_LastChanged = (UserMembers)lastChanged;

            OnChanged();
        }

        void OnChanged()
        {
            Changed?.Invoke(this);
        }

        public Dictionary<string, PlayerProperty> GetDataForUnityServices() =>
            new()
            {
                { "DisplayName", new PlayerProperty(DisplayName, VisibilityPropertyOptions.Member) },
            };
    }
}
