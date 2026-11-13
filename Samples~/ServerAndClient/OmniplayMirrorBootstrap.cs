using Mirror;
using Omniplay.Mirror;
using UnityEngine;

/// <summary>
/// On the NetworkManager's object, beside OmniplayAuthenticator (set as the manager's authenticator)
/// and OmniplayMirrorHeartbeat. A batch-mode build - the server omniplay/docker starts, its secret and
/// room in its environment - starts the server; a player's build connects with the seat the game's
/// backend answered, given here on the command line: -address host -ticket eyJ...
/// </summary>
public sealed class OmniplayMirrorBootstrap : MonoBehaviour
{
    public NetworkManager manager;

    void Start()
    {
        if (manager == null) manager = GetComponent<NetworkManager>();
        if (Application.isBatchMode)
        {
            manager.StartServer();
            return;
        }
        var address = Argument("-address");
        var ticket = Argument("-ticket");
        if (address == null || ticket == null)
        {
            Debug.Log("Omniplay sample: start with -address <host> -ticket <ticket> to join a server.");
            return;
        }
        ((OmniplayAuthenticator)manager.authenticator).ticket = ticket;
        manager.networkAddress = address;   // the host; the port is the transport's
        manager.StartClient();
    }

    static string Argument(string name)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == name) return args[i + 1];
        }
        return null;
    }
}
