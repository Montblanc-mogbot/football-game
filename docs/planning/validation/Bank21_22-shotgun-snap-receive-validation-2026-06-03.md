# Bank21_22 packet 21A shotgun snap receive validation — 2026-06-03

## Scope
- Task: Port the next live packet 21A alternate-exchange slice
- Chosen bounded variant: `RECEIVE_SNAP_SHOTGUN_COMMAND_START` (`D5`) from `Bank21_22_play_commands_on_field_logic.asm`
- Runtime boundary preserved: Bank19_20 host still owns the snap gate (`CHECK_SNAP_PUNT` request + `BallSnapped` requirement); Bank21_22 runtime now owns the shotgun long-snap receive semantics.

## Source references
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:2450-2472`
  - `RECEIVE_SNAP_SHOTGUN_COMMAND_START` retargets manual control/displayed-name ownership, waits for the snapped bit, starts `SET_SHOTGUN_LOCATION_DO_ANIMATION`, then loops until `BALL_COLLISION` indicates the ball reached the quarterback.
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:2474-2498`
  - `SET_SHOTGUN_LOCATION_DO_ANIMATION` sets final ball location, uses loft `$06` and speed `$40`, starts the moving-ball task, and marks the special shotgun-snap state before the receive command resumes and finishes with the shared 4-frame delay.

## Production-facing runtime wiring
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/ShotgunSnapReceiveCommandHandler.cs`
  - Adds a production-facing handler for `ShotgunSnapReceiveCommand`.
  - Records that the runtime waited on the host snap gate, created an in-flight ball state, resolved the ball animation on collision, assigned the QB as ball carrier, and held the shared 4-frame post-receive delay.
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/OffensiveExchangeCommandDispatcher.cs`
  - Registers `ShotgunSnapReceiveCommandHandler` alongside the existing packet-21A receive/exchange handlers.
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandRuntime.cs`
  - Treats `ShotgunSnapReceiveCommand` as a first-class offensive-exchange runtime command so it can be stepped directly from the live seam or later explicit continuation work.
- `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs:1307-1324`
  - Changes the current `CHECK_SNAP_PUNT` live sample entry from `FieldGoalSnapReceiveCommand` to `ShotgunSnapReceiveCommand`, keeping the host-side trigger routine explicit while moving the shotgun receive semantics into the runtime layer.

## Bank19_20 / Bank21_22 seam check
- `src/FootballGame/Gameplay/OnField/Services/PreSnapControlService.cs:24-35`
  - Still records `CHECK_SNAP_PUNT`, sets `BallSnapped = false`, and queues the host request; no shotgun receive semantics were moved into the host layer.
- `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs:1147-1169`
  - Still requires `state.BallSnapped` before advancing any pending runtime request, so snap timing ownership remains on the Bank19_20 side.

## Verification
- `git diff --check` ✅
- Direct code inspection ✅
  - Confirmed the live `CHECK_SNAP_PUNT` seam now references `ShotgunSnapReceiveCommand` instead of the FG/XP holder receive sample.
  - Confirmed the new handler carries the source-visible shotgun-specific animation/collision semantics without collapsing the host/runtime split.

## Notes
- `FieldGoalSnapReceiveCommandHandler` remains in production-facing runtime code for the separate FG/XP holder path; this slice only changes which bounded packet-21A variant is currently used as the live sample entry.
