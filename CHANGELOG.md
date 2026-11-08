# Changelog

## Unreleased

### Added
- `io.omniplay.mirror`, assembly `Omniplay.Mirror` (only under Mirror's `MIRROR` define):
  `OmniplayAuthenticator` (the ticket as the client's first message, `TicketRequest` and
  `TicketAnswer`, one ticket one connection, a timeout), `OmniplayMirrorHeartbeat`. Depends on
  `io.omniplay.dotnet`.
