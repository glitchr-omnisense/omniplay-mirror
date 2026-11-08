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

## Installing

Mirror ships from the Asset Store into `Assets/Mirror`, not as a UPM package, so this package cannot
name it as a dependency: its assembly `Omniplay.Mirror` references the assembly `Mirror` by name and
compiles only under the `MIRROR` define, which Mirror adds to the project itself. Without Mirror,
nothing of it compiles and nothing fails.

It runs on omniplay/dotnet (`io.omniplay.dotnet`, the assembly `Omniplay`): its `package.json`
depends on it by name, and a project installing from Git names both, the first satisfying the
second's dependency (Unity resolves no Git dependency of a package by itself):

```json
"io.omniplay.dotnet": "https://github.com/glitchr-omnisense/omniplay-dotnet.git#1.x",
"io.omniplay.mirror": "https://github.com/glitchr-omnisense/omniplay-mirror.git#1.x"
```

## Verified, and not

- Verified (2026-11-08): the package's sources compiled with omniplay/dotnet's, against Unity
  6000.3's `UnityEngine.CoreModule.dll` and a stub of Mirror whose signatures are copied from
  Mirror 93.0.0's sources (`NetworkAuthenticator`, `NetworkServer`, `NetworkClient`, the
  connections), netstandard2.1, warnings as errors: 0 errors, 0 warnings. The ticket's checks are
  omniplay/dotnet's, tested on omniplay's vectors (`TicketGate`, 36 tests).
- Not verified: inside a Unity editor (the package was not imported, Mirror's weaver did not run
  on its messages), a client connecting to a server.

## Licence

MIT.
