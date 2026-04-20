using Zone8.Utilities;
using System;
using System.Collections.Generic;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace Zone8.UnityServices.Sessions
{
    /// <summary>
    /// A local wrapper around a session's remote data, with additional functionality for providing that data to UI
    /// elements and tracking local player objects.
    /// </summary>
    [Serializable]
    public sealed class LocalSession : Singleton<LocalSession>
    {
        private Dictionary<string, LocalSessionUser> _sessionUsers = new();
        public Dictionary<string, LocalSessionUser> SessionUsers => _sessionUsers;

        private SessionData _data;

        public event Action<LocalSession> Changed;

        public string SessionID
        {
            get => _data.SessionID;
            set
            {
                _data.SessionID = value;
                OnChanged();
            }
        }

        public string SessionCode
        {
            get => _data.SessionCode;
            set
            {
                _data.SessionCode = value;
                OnChanged();
            }
        }

        public string RelayJoinCode
        {
            get => _data.RelayJoinCode;
            set
            {
                _data.RelayJoinCode = value;
                OnChanged();
            }
        }

        public struct SessionData
        {
            public string SessionID { get; set; }
            public string SessionCode { get; set; }
            public string RelayJoinCode { get; set; }
            public string SessionName { get; set; }
            public bool Private { get; set; }
            public int MaxPlayerCount { get; set; }

            public SessionData(SessionData existing)
            {
                SessionID = existing.SessionID;
                SessionCode = existing.SessionCode;
                RelayJoinCode = existing.RelayJoinCode;
                SessionName = existing.SessionName;
                Private = existing.Private;
                MaxPlayerCount = existing.MaxPlayerCount;
            }

            public SessionData(string sessionCode)
            {
                SessionID = null;
                SessionCode = sessionCode;
                RelayJoinCode = null;
                SessionName = null;
                Private = false;
                MaxPlayerCount = -1;
            }
        }

        public void AddUser(LocalSessionUser user)
        {
            if (!_sessionUsers.ContainsKey(user.ID))
            {
                DoAddUser(user);
                OnChanged();
            }
        }

        private void DoAddUser(LocalSessionUser user)
        {
            _sessionUsers.Add(user.ID, user);
            user.Changed += OnChangedUser;
        }

        public void RemoveUser(LocalSessionUser user)
        {
            DoRemoveUser(user);
            OnChanged();
        }

        private void DoRemoveUser(LocalSessionUser user)
        {
            if (!_sessionUsers.ContainsKey(user.ID))
            {
                Debug.LogWarning($"Player {user.DisplayName}({user.ID}) does not exist in session: {SessionID}");
                return;
            }

            _sessionUsers.Remove(user.ID);
            user.Changed -= OnChangedUser;
        }

        private void OnChangedUser(LocalSessionUser user)
        {
            OnChanged();
        }

        private void OnChanged()
        {
            Changed?.Invoke(this);
        }

        public void CopyDataFrom(SessionData data, Dictionary<string, LocalSessionUser> currUsers)
        {
            _data = data;

            if (currUsers == null)
            {
                _sessionUsers = new Dictionary<string, LocalSessionUser>();
            }
            else
            {
                List<LocalSessionUser> toRemove = new List<LocalSessionUser>();
                foreach (var oldUser in _sessionUsers)
                {
                    if (currUsers.ContainsKey(oldUser.Key))
                    {
                        oldUser.Value.CopyDataFrom(currUsers[oldUser.Key]);
                    }
                    else
                    {
                        toRemove.Add(oldUser.Value);
                    }
                }

                foreach (var remove in toRemove)
                {
                    DoRemoveUser(remove);
                }

                foreach (var currUser in currUsers)
                {
                    if (!_sessionUsers.ContainsKey(currUser.Key))
                    {
                        DoAddUser(currUser.Value);
                    }
                }
            }

            OnChanged();
        }

        public Dictionary<string, SessionProperty> GetDataForUnityServices() =>
            new()
            {
                { "RelayJoinCode", new SessionProperty(RelayJoinCode) }
            };

        public void ApplyRemoteData(ISession session)
        {
            var info = new SessionData(); // Technically, this is largely redundant after the first assignment, but it won't do any harm to assign it again.
            info.SessionID = session.Id;
            info.SessionName = session.Name;
            info.MaxPlayerCount = session.MaxPlayers;
            info.SessionCode = session.Code;
            info.Private = session.IsPrivate;

            if (session.Properties != null)
            {
                info.RelayJoinCode = session.Properties.TryGetValue("RelayJoinCode", out var property) ? property.Value : null; // By providing RelayCode through the session properties with Member visibility, we ensure a client is connected to the session before they could attempt a relay connection, preventing timing issues between them.
            }
            else
            {
                info.RelayJoinCode = null;
            }

            var localSessionUsers = new Dictionary<string, LocalSessionUser>();
            foreach (var player in session.Players)
            {
                if (player.Properties != null)
                {
                    if (localSessionUsers.ContainsKey(player.Id))
                    {
                        localSessionUsers.Add(player.Id, localSessionUsers[player.Id]);
                        continue;
                    }
                }

                // If the player isn't connected to Relay, get the most recent data that the session knows.
                // (If we haven't seen this player yet, a new local representation of the player will have already been added by the LocalSession.)
                var incomingData = new LocalSessionUser
                {
                    IsHost = session.Host.Equals(player.Id),
                    DisplayName = player.Properties != null && player.Properties.TryGetValue("DisplayName", out var property) ? property.Value : default,
                    ID = player.Id
                };

                localSessionUsers.Add(incomingData.ID, incomingData);
            }

            CopyDataFrom(info, localSessionUsers);
        }

        public void Reset(LocalSessionUser localUser)
        {
            CopyDataFrom(new SessionData(), new Dictionary<string, LocalSessionUser>());
            AddUser(localUser);
        }
    }
}
