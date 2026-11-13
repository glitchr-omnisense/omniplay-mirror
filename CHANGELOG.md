# Changelog

## [Unreleased]

## [1.0.0] - 2026-11-13

The first release (the 0.1.0 prepared before was never published).

### Added
- `io.omniplay.mirror`, assembly `Omniplay.Mirror` (only under Mirror's `MIRROR` define):
  `OmniplayAuthenticator` (the ticket as the client's first message, `TicketRequest` and
  `TicketAnswer`, one ticket one connection, a timeout), `OmniplayMirrorHeartbeat`. Depends on
  `io.omniplay.dotnet` 1.0.0.
- The sample **Server and client** (`Samples~/ServerAndClient`): `OmniplayMirrorFirstCall`, the
  README's first call (a ticket PHP signed, checked by `TicketGate`), and `OmniplayMirrorBootstrap`
  (the server in batch mode, a client joining with `-address` and `-ticket`).
- The manifest's documentation, changelog and licence URLs.
- `Tests~/Compile`: the package, its sample and omniplay/dotnet compiled against stubs of
  UnityEngine and Mirror (or a real `UnityEngine.CoreModule.dll`), the first call run. CI (that, the
  manifest, the metas, the asmdefs) and a release workflow on a tag (the GitHub release with the
  UPM tarball).
