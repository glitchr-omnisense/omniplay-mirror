using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Omniplay.Mirror
{
    /// <summary>A client's first message: the ticket the application handed its player.</summary>
    public struct TicketRequest : NetworkMessage
    {
        public string ticket;
    }

    /// <summary>The server's answer to it.</summary>
    public struct TicketAnswer : NetworkMessage
    {
        public bool accepted;
        public string reason;
    }

    /// <summary>
    /// Mirror's authenticator for omnisense/omniplay's tickets. The client sends its ticket (set
    /// <see cref="ticket"/> before connecting) as its first message; the server lets it in when the
    /// ticket holds for this server's room with its secret and was not used before (TicketGate), keeps
    /// the ticket in the connection's authenticationData (its claims, the player as Ticket.Player), and
    /// refuses it otherwise - a second after telling it, as Mirror's BasicAuthenticator does - or when
    /// no ticket comes within the timeout. The secret and room come from the server's environment
    /// (OMNIPLAY_TICKET_SECRET, OMNIPLAY_ROOM: what omniplay/docker hands it) unless set in code.
    /// </summary>
    [AddComponentMenu("Network/Authenticators/Omniplay Authenticator")]
    public sealed class OmniplayAuthenticator : NetworkAuthenticator
    {
        [Header("Client")]
        [Tooltip("The ticket the application handed the player, sent as the first message.")]
        public string ticket;

        [Header("Server")]
        [Tooltip("The environment variable holding the ticket secret.")]
        public string secretVariable = "OMNIPLAY_TICKET_SECRET";

        [Tooltip("Seconds a client is given to send its ticket.")]
        public float timeout = 10f;

        private readonly HashSet<NetworkConnectionToClient> refusing = new HashSet<NetworkConnectionToClient>();
        private TicketGate gate;

        /// <summary>The ticket secret, when not the environment's.</summary>
        public string Secret { get; set; }

        /// <summary>The room tickets must name - this server's id - when not the environment's.</summary>
        public string Room { get; set; }

        /// <summary>A client let in, with its ticket.</summary>
        public event Action<NetworkConnectionToClient, Ticket> Admitted;

        /// <summary>A client refused.</summary>
        public event Action<NetworkConnectionToClient> Refused;

        public override void OnStartServer()
        {
            var environment = ServerEnvironment.Read(null, secretVariable);
            gate = new TicketGate(Secret ?? environment.Secret, Room ?? environment.Room);
            if (!gate.Configured)
            {
                Debug.LogError("Omniplay: no ticket secret or room (" + secretVariable + ", OMNIPLAY_ROOM): every client will be refused.");
            }
            NetworkServer.RegisterHandler<TicketRequest>(OnTicketRequest, false);
        }

        public override void OnStopServer()
        {
            NetworkServer.UnregisterHandler<TicketRequest>();
            refusing.Clear();
        }

        public override void OnServerAuthenticate(NetworkConnectionToClient conn)
        {
            StartCoroutine(Timeout(conn));
        }

        private void OnTicketRequest(NetworkConnectionToClient conn, TicketRequest message)
        {
            if (conn.isAuthenticated || refusing.Contains(conn)) return;
            var admitted = gate?.Admit(message.ticket);
            if (admitted != null)
            {
                conn.authenticationData = admitted;
                conn.Send(new TicketAnswer { accepted = true });
                ServerAccept(conn);
                Admitted?.Invoke(conn, admitted);
                return;
            }
            Refuse(conn, "The ticket does not hold for this server.");
        }

        private void Refuse(NetworkConnectionToClient conn, string reason)
        {
            refusing.Add(conn);
            conn.Send(new TicketAnswer { accepted = false, reason = reason });
            conn.isAuthenticated = false;
            Refused?.Invoke(conn);
            StartCoroutine(Disconnect(conn, 1f));
        }

        private IEnumerator Timeout(NetworkConnectionToClient conn)
        {
            yield return new WaitForSeconds(timeout);
            if (!conn.isAuthenticated && !refusing.Contains(conn)) Refuse(conn, "No ticket came.");
        }

        private IEnumerator Disconnect(NetworkConnectionToClient conn, float delay)
        {
            yield return new WaitForSeconds(delay);
            ServerReject(conn);
            yield return null;
            refusing.Remove(conn);
        }

        public override void OnStartClient()
        {
            NetworkClient.RegisterHandler<TicketAnswer>(OnTicketAnswer, false);
        }

        public override void OnStopClient()
        {
            NetworkClient.UnregisterHandler<TicketAnswer>();
        }

        public override void OnClientAuthenticate()
        {
            NetworkClient.Send(new TicketRequest { ticket = ticket ?? "" });
        }

        private void OnTicketAnswer(TicketAnswer message)
        {
            if (message.accepted)
            {
                ClientAccept();
                return;
            }
            Debug.LogWarning("Omniplay: the server refused the ticket: " + message.reason);
            ClientReject();
        }
    }
}
