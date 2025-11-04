using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Multiplayer;

namespace Zone8.UnityServices.Sessions
{
    public enum EConnectionType
    {
        Relay,
        DistributedAuthority
    }

    /// <summary>
    /// Wrapper for all the interactions with the Sessions API.
    /// </summary>
    public class MultiplayerServicesInterface
    {
        readonly int _maxSessionsToShow = 16;
        readonly int _maxPlayers = 8;

        readonly EConnectionType _connectionType = EConnectionType.Relay;
        readonly List<FilterOption> _filterOptions;
        readonly List<SortOption> _sortOptions;

        public MultiplayerServicesInterface(int maxPlayers, int maxSessionsToShow, List<FilterOption> filterOptions = null, List<SortOption> sortOption = null, EConnectionType connectionType = EConnectionType.Relay)
        {
            _maxPlayers = maxPlayers;
            _maxSessionsToShow = maxSessionsToShow;
            _connectionType = connectionType;

            if (sortOption != null) _sortOptions = sortOption;
            if (filterOptions != null) _filterOptions = filterOptions;

        }

        public async Task<ISession> CreateSession(SessionOptions sessionOptions)
        {
            if (_connectionType == EConnectionType.Relay)
            {
                sessionOptions = sessionOptions.WithRelayNetwork();
            }
            else if (_connectionType == EConnectionType.DistributedAuthority)
            {
                sessionOptions = sessionOptions.WithDistributedAuthorityNetwork();
            }

            return await MultiplayerService.Instance.CreateSessionAsync(sessionOptions);
        }

        public async Task<ISession> CreateOrJoinSession(string sessionId, SessionOptions sessionOptions)
        {

            if (_connectionType == EConnectionType.Relay)
            {
                sessionOptions = sessionOptions.WithRelayNetwork();
            }
            else if (_connectionType == EConnectionType.DistributedAuthority)
            {
                sessionOptions = sessionOptions.WithDistributedAuthorityNetwork();
            }

            return await MultiplayerService.Instance.CreateOrJoinSessionAsync(sessionId, sessionOptions);
        }

        public async Task<ISession> JoinSessionByCode(string sessionCode, Dictionary<string, PlayerProperty> localUserData)
        {
            var joinSessionOptions = new JoinSessionOptions
            {
                PlayerProperties = localUserData
            };
            return await MultiplayerService.Instance.JoinSessionByCodeAsync(sessionCode, joinSessionOptions);
        }

        public async Task<ISession> JoinSessionById(string sessionId, Dictionary<string, PlayerProperty> localUserData)
        {
            var joinSessionOptions = new JoinSessionOptions
            {
                PlayerProperties = localUserData
            };
            return await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId, joinSessionOptions);
        }

        public async Task<ISession> QuickJoinSession(Dictionary<string, PlayerProperty> localUserData)
        {
            var quickJoinOptions = new QuickJoinOptions
            {
                Filters = _filterOptions,
                CreateSession = true // create a Session if no matching Session was found
            };

            var sessionOptions = new SessionOptions
            {
                MaxPlayers = _maxPlayers,
                PlayerProperties = localUserData
            };

            if (_connectionType == EConnectionType.Relay)
            {
                sessionOptions = sessionOptions.WithRelayNetwork();
            }
            else if (_connectionType == EConnectionType.DistributedAuthority)
            {
                sessionOptions = sessionOptions.WithDistributedAuthorityNetwork();
            }

            return await MultiplayerService.Instance.MatchmakeSessionAsync(quickJoinOptions, sessionOptions);
        }

        public async Task<QuerySessionsResults> QuerySessions()
        {
            var querySessionOptions = new QuerySessionsOptions
            {
                Count = _maxSessionsToShow,
                FilterOptions = _filterOptions,
                SortOptions = _sortOptions
            };
            return await MultiplayerService.Instance.QuerySessionsAsync(querySessionOptions);
        }

        public async Task<ISession> ReconnectToSession(string sessionId)
        {
            return await MultiplayerService.Instance.ReconnectToSessionAsync(sessionId);
        }

        public async Task<QuerySessionsResults> QueryAllSessions()
        {
            return await MultiplayerService.Instance.QuerySessionsAsync(new QuerySessionsOptions());
        }
    }
}
