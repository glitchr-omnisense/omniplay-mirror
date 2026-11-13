// A stub of the Mirror API this package and its sample use, the signatures copied from Mirror
// 93.0.0's sources (NetworkAuthenticator, NetworkServer, NetworkClient, the connections,
// NetworkManager), so that the package compiles without Mirror (the CI). Not shipped: Unity ignores
// Tests~. Mirror's weaver, which generates the messages' serializers, is not run.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror
{
    public interface NetworkMessage { }

    public static class Channels
    {
        public const int Reliable = 0;
    }

    public abstract class NetworkConnection
    {
        public bool isAuthenticated;
        public object authenticationData;

        public void Send<T>(T message, int channelId = Channels.Reliable) where T : struct, NetworkMessage { }
    }

    public class NetworkConnectionToClient : NetworkConnection { }

    public class LocalConnectionToClient : NetworkConnectionToClient { }

    public static class NetworkServer
    {
        public static readonly Dictionary<int, NetworkConnectionToClient> connections = new Dictionary<int, NetworkConnectionToClient>();

        public static bool active { get; internal set; }

        public static void RegisterHandler<T>(Action<NetworkConnectionToClient, T> handler, bool requireAuthentication = true) where T : struct, NetworkMessage { }

        public static bool UnregisterHandler<T>() where T : struct, NetworkMessage => true;
    }

    public static class NetworkClient
    {
        public static void RegisterHandler<T>(Action<T> handler, bool requireAuthentication = true) where T : struct, NetworkMessage { }

        public static bool UnregisterHandler<T>() where T : struct, NetworkMessage => true;

        public static void Send<T>(T message, int channelId = Channels.Reliable) where T : struct, NetworkMessage { }
    }

    public abstract class NetworkAuthenticator : MonoBehaviour
    {
        public virtual void OnStartServer() { }

        public virtual void OnStopServer() { }

        public virtual void OnServerAuthenticate(NetworkConnectionToClient conn) { }

        protected void ServerAccept(NetworkConnectionToClient conn) { }

        protected void ServerReject(NetworkConnectionToClient conn) { }

        public virtual void OnStartClient() { }

        public virtual void OnStopClient() { }

        public virtual void OnClientAuthenticate() { }

        protected void ClientAccept() { }

        protected void ClientReject() { }
    }

    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager singleton { get; internal set; }

        public NetworkAuthenticator authenticator;

        public string networkAddress = "localhost";

        public void StartServer() { }

        public void StartClient() { }
    }
}
