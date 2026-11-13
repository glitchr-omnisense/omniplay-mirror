# omniplay/mirror

[omnisense/omniplay](https://github.com/glitchr-omnisense/omniplay) inside a Unity server running
[Mirror](https://mirror-networking.com): the UPM package `io.omniplay.mirror`.

- **`OmniplayAuthenticator`** (a `NetworkAuthenticator`): the client sends the ticket the application
  handed its player as its first message (set `ticket` before `StartClient()`); the server lets it
  in when the ticket holds for this server's room with its secret and was not used before
  (omniplay/dotnet's `TicketGate`), keeps it in the connection's `authenticationData` (`Ticket`:
  its claims, the player as `Ticket.Player`), and refuses it otherwise - a second after telling the
  client, as Mirror's `BasicAuthenticator` does - or when no ticket comes within `timeout` (10 s).
  Events `Admitted` and `Refused`.
- **`OmniplayMirrorHeartbeat`**: while Mirror's server runs, the heartbeat to the application every
  `every` seconds (10): the authenticated connections but the host's own, and `health` (set it to
  `Health.Draining` once the server takes nobody new).

The secret, the room (the server's id), the heartbeat route and the service come from the server's
environment - `OMNIPLAY_TICKET_SECRET`, `OMNIPLAY_ROOM`, `OMNIPLAY_HEARTBEAT_URL`... as
omniplay/docker hands them - unless set in code (`Secret`, `Room`, `Environment`). A server started
by hand gives its address through `Registration` and is registered by its first heartbeat.

```csharp
// The client, with what the application answered when the player asked to play:
var authenticator = (OmniplayAuthenticator)NetworkManager.singleton.authenticator;
authenticator.ticket = seat.ticket;
NetworkManager.singleton.networkAddress = host;   // seat.address is "host:port": the port goes to the transport
NetworkManager.singleton.StartClient();
```

## Install and a first call

Mirror ships from the Asset Store into `Assets/Mirror`, not as a UPM package, so this package cannot
name it as a dependency: its assembly `Omniplay.Mirror` references the assembly `Mirror` by name and
compiles only under the `MIRROR` define, which Mirror adds to the project itself. Without Mirror,
nothing of it compiles and nothing fails.

It runs on omniplay/dotnet (`io.omniplay.dotnet`, the assembly `Omniplay`): its `package.json`
depends on it by name, and a project installing from Git names both, the first satisfying the
second's dependency (Unity resolves no Git dependency of a package by itself):

```json
"io.omniplay.dotnet": "https://github.com/glitchr-omnisense/omniplay-dotnet.git#v1.0.0",
"io.omniplay.mirror": "https://github.com/glitchr-omnisense/omniplay-mirror.git#v1.0.0"
```

`#v1.0.0` pins the release (`#1.x` follows the branch); Unity 2021.3 or later. The package's sample
(Package Manager, the package, Samples, **Server and client**) holds the first call below and a
bootstrap that starts the server in batch mode or joins one. The first call needs no server, no
account, no key and no network: a ticket the PHP application signed (omniplay's `Tickets`, a
recorded exchange from its vectors) checked by omniplay/dotnet's `TicketGate`, which the package
lets players in through. Put it on any object and press Play:

```csharp
using Omniplay;
using UnityEngine;

public sealed class OmniplayMirrorFirstCall : MonoBehaviour
{
    // A ticket the PHP application signed (omnisense/omniplay's Tickets, recorded in its vectors):
    const string FromPhp = "eyJzdWIiOiJwbGF5ZXItNDIiLCJuYW1lIjoiQWRhIiwicm9vbSI6ImV1LTEtYTNmOSIsImV4cCI6NDEwMjQ0NDgwMH0.0bb9f63e3f6c9d7237c18855738470bae0f54410d4d1177ee87cc65c4608a57c";
    const string Secret = "s3cr3t-shared-with-the-servers";   // OMNIPLAY_TICKET_SECRET on a real server

    void Start()
    {
        var gate = new TicketGate(Secret, "eu-1-a3f9");   // what lets players in on the server eu-1-a3f9
        Debug.Log(gate.Admit(FromPhp)?.Player);           // player-42: let in
        Debug.Log(gate.Admit(FromPhp) == null);           // True: one ticket, one connection
        Debug.Log(new TicketGate(Secret, "eu-1-b7c2").Admit(FromPhp) == null);   // True: another server refuses it
    }
}
```

## With Mirror's Steam transport

The authenticator does not depend on the transport: the ticket is Mirror's first message whatever
carries it. With a Steam transport (FizzySteamworks over Steamworks.NET, a community transport),
clients connect through Steam's relay to a SteamID rather than to an IP and a port: the client's
`networkAddress` is then the server's SteamID, which the server gives as its address
(`Registration`), or which the lobby's metadata carries beside the ticket (omniplay/steam's README:
lobbies stay in the game). The player's own Steam identity is verified by the platform before the
seat, with omniplay/steam. Not verified, and no code here for it.

## Verified, and not

No Unity editor was opened for this release: the package is verified statically and by compiling it.

- Verified (2026-11-13, .NET SDK 10.0.404 in a one-off container): the package's sources, its
  sample's and omniplay/dotnet 1.0.0's compiled together, netstandard2.1, warnings as errors - 0
  errors, 0 warnings - against Unity 6000.3's `UnityEngine.CoreModule.dll` and a stub of Mirror
  whose signatures are copied from Mirror 93.0.0's sources (`NetworkAuthenticator`, `NetworkServer`, `NetworkClient`, the connections, `NetworkManager`) (`Tests~/Compile`); compiled again against the
  UnityEngine stub, the sample's first call run: `player-42`, `True`, `True`. The manifest, the
  changelog's section, every asset's `.meta` and unique GUIDs, the asmdefs' references: checked.
  The ticket's checks are omniplay/dotnet's, tested on omniplay's vectors (38 tests).
- The CI does the same on GitHub's runners, against the UnityEngine stub only (no Unity there).
- Not verified: inside a Unity editor (the package was not imported, the sample not run in Play
  mode, Mirror's weaver not run on its messages), a client connecting to a server, the heartbeat answered by an application running
  omniplay's `Heartbeats`.

## Releasing

A pushed tag `vX.Y.Z` equal to `package.json`'s version runs `.github/workflows/release.yml`: the CI
on the tag, the changelog's section checked, the GitHub release with the UPM tarball (`npm pack`).
Projects install the tag by Git URL (`#vX.Y.Z`). Tag omniplay/dotnet's same version first: this
package depends on it.

## Licence

MIT.
