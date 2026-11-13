# Server and client (omniplay/mirror)

- `OmniplayMirrorFirstCall`: on any object, logs a ticket the PHP application signed checked by
  omniplay/dotnet's `TicketGate` - `player-42`, `True`, `True`. No server, no account, no key.
- `OmniplayMirrorBootstrap`: on the NetworkManager's object, with `OmniplayAuthenticator` set as the
  manager's authenticator and `OmniplayMirrorHeartbeat`. A batch-mode build starts the server (its
  secret and room from the environment omniplay/docker hands it); a player's build joins with
  `-address <host> -ticket <ticket>`, the seat the game's backend answered.
