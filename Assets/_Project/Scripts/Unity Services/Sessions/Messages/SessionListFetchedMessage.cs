using Zone8.Events;
using System.Collections.Generic;
using Unity.Services.Multiplayer;

namespace Zone8.UnityServices.Sessions
{
    public struct SessionListFetchedMessage : IEvent
    {
        public readonly IList<ISessionInfo> LocalSessions;

        public SessionListFetchedMessage(IList<ISessionInfo> localSessions)
        {
            LocalSessions = localSessions;
        }
    }
}
