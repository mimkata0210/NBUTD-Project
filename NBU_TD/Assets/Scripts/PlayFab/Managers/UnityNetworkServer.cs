
namespace PlayFab.Networking
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Mirror;
    using UnityEngine.Events;

    public class UnityNetworkServer : NetworkManager
    {
        public static UnityNetworkServer Instance { get; private set; }
        #region Serialize.
        [SerializeField]
        private Configuration config;
        #endregion

        #region Public.
        public PlayerEvent OnPlayerAdded = new PlayerEvent();
        public PlayerEvent OnPlayerRemoved = new PlayerEvent();
        public int MaxConnections { get { return maxConnections; } }
        #endregion



        public List<UnityNetworkConnection> Connections
        {
            get { return _connections; }
            private set { _connections = value; }
        }
        private List<UnityNetworkConnection> _connections = new List<UnityNetworkConnection>();

        public class PlayerEvent : UnityEvent<string> { }

        #region Public.
        /// <summary>
        /// Dispatched after a client has it's player instantiated.
        /// </summary>
        public static event Action<NetworkConnection> RelayOnServerAddPlayer;
        #endregion

        // Use this for initialization
        public override void Awake()
        {

            base.Awake();
            if(Instance != this && Instance != null)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
            }
            if (config.buildType == BuildType.REMOTE_SERVER)
            {
                Debug.Log("[UnityNetworkServer].AddRemoteServerListeners");
                NetworkServer.RegisterHandler<ReceiveAuthenticateMessage>(OnReceiveAuthenticate);

                //_netManager.transport.port = Port;
            }
        }

        public override void Start()
        {
            base.Start();
        }

        public void StartListen()
        {
            NetworkServer.Listen(maxConnections);
        }

        public override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            NetworkServer.Shutdown();
        }

        private void OnReceiveAuthenticate(NetworkConnection nconn, ReceiveAuthenticateMessage message)
        {
            var conn = _connections.Find(c => c.ConnectionId == nconn.connectionId);
            if (conn != null)
            {
                conn.PlayFabId = message.PlayFabId;
                conn.IsAuthenticated = true;
                OnPlayerAdded.Invoke(message.PlayFabId);
            }
        }

        public override void OnServerConnect(NetworkConnection conn)
        {
            base.OnServerConnect(conn);

            Debug.LogWarning("Client Connected");
            var uconn = _connections.Find(c => c.ConnectionId == conn.connectionId);
            if (uconn == null)
            {
                _connections.Add(new UnityNetworkConnection()
                {
                    Connection = conn,
                    ConnectionId = conn.connectionId,
                    LobbyId = PlayFabMultiplayerAgentAPI.SessionConfig.SessionId
                });
            }
        }

        public override void OnServerError(NetworkConnection conn, Exception exception)
        {
            base.OnServerError(conn, exception);
            Debug.Log(string.Format("Unity Network Connection Status: code - {0}", exception));
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            base.OnServerDisconnect(conn);

            var uconn = _connections.Find(c => c.ConnectionId == conn.connectionId);
            if (uconn != null)
            {
                if (!string.IsNullOrEmpty(uconn.PlayFabId))
                {
                    OnPlayerRemoved.Invoke(uconn.PlayFabId);
                }
                _connections.Remove(uconn);
            }
        }

        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            base.OnServerAddPlayer(conn);
            RelayOnServerAddPlayer?.Invoke(conn);
            // Add player terain 
            WorldManager.Instance.CreateRoad(conn, conn.identity.gameObject);
            MinionManager.Instance.AddNewPlayerSpawnQue(conn);
        }

    }

    [Serializable]
    public class UnityNetworkConnection
    {
        public bool IsAuthenticated;
        public string PlayFabId;
        public string LobbyId;
        public int ConnectionId;
        public NetworkConnection Connection;
    }

    public class CustomGameServerMessageTypes
    {
        public const short ReceiveAuthenticate = 900;
        public const short ShutdownMessage = 901;
        public const short MaintenanceMessage = 902;
    }

    public struct ReceiveAuthenticateMessage : NetworkMessage
    {
        public string PlayFabId;
    }

    public struct ShutdownMessage : NetworkMessage { }

    [Serializable]
    public struct MaintenanceMessage : NetworkMessage
    {
        public DateTime ScheduledMaintenanceUTC;
    }

    public static class MaintenanceMessageFunctions
    {
        public static MaintenanceMessage Deserialize(this NetworkReader reader)
        {
            MaintenanceMessage msg = new MaintenanceMessage();

            var json = PlayFab.PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
            msg.ScheduledMaintenanceUTC = json.DeserializeObject<DateTime>(reader.ReadString());

            return msg;
        }

        public static void Serialize(this NetworkWriter writer, MaintenanceMessage value)
        {
            var json = PlayFab.PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
            var str = json.SerializeObject(value.ScheduledMaintenanceUTC);
            writer.Write(str);
        }
    }

}
