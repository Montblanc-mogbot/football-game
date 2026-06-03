# Bank21_22 packet 21A — snap exchange and backfield transfer semantics

Updated: 2026-06-03

## Purpose

This note documents one bounded offensive command semantics slice from `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm`:

- snap initiation and snap receipt
- immediate backfield transfer commands built on top of that exchange (`handoffToPlayer`, `fakeHandoffToPlayer`, `pitchToPlayer`)

The goal is to state exactly how this slice should be entered and stepped from the current Bank19_20 host flow without editing runtime code yet.

## Why this slice is bounded enough for packet 21A

This family is compact and source-coherent because it starts at the offensive exchange boundary:

- the host decides when the ball is snapped
- Bank21_22 interprets the center/QB/holder exchange commands
- those commands can then retarget a second offensive player into a receive-handoff or receive-pitch continuation

That keeps the packet focused on one offensive runtime chain instead of trying to cover the whole pass/run command language.

## Commands covered

### Snap initiators
- `CENTER_HIKE_COMMAND_START` (`D2`) — under-center snap trigger
- `SHOTGUN_HIKE_COMMAND_START` (`D3`) — shotgun snap trigger
- `RECEIVE_SNAP_CENTER_COMMAND_START` (`D4`) — QB under-center receive
- `RECEIVE_SNAP_SHOTGUN_COMMAND_START` (`D5`) — QB shotgun receive
- `RECEIVE_SNAP_FG_XP_COMMAND_START` (`D6`) — holder long-snap receive for FG/XP

### Immediate backfield transfers
- `HANDOFF_COMMAND_START` (`5x`)
- `FAKE_HANDOFF_COMMAND_START` (`6x`)
- `PITCH_BALL_COMMAND_START` (`7x`)
- `RB_RECEIVES_HANDOFF_START`
- `RB_FAKE_HANDOFF_ANIMATION`
- `WAIT_FOR_PLAYER_RECEIVES_PITCH`

## Source anchors inside Bank21_22

### Decode and pointer advance
`DO_NEXT_PLAYER_COMMAND` is the entrypoint that fetches the current command from `PLAY_CODE_ADDR`, decodes group vs single command shape, advances the script pointer, and dispatches into the handler tables.

Source anchors:
- `DO_NEXT_PLAYER_COMMAND`
- `GROUP_COMMAND_TABLE`
- `SINGLE_COMMAND_TABLE`
- `MULTI_PLAYER_COMMAND_LENGTH_TABLE`
- `SINGLE_PLAYER_COMMAND_LENGTH_TABLE`

This matters for packet 21A because every command below assumes the player has already been entered through this per-player interpreter step.

### Host-visible command retargeting helpers
The backfield-transfer commands rely on these Bank21_22 helpers to redirect the target runner into a continuation command:

- `UPDATE_CURRENT_PLAYER_COMMAND_ADDR`
- `UPDATE_LOCAL_PLAYER_COMMAND_ADDR_IF_VALID`
- `SET_PLAYER_BALL_CARR_UPDATE_POSS_STATUS`
- `UPDATE_MAN_CONTROL_PLAYER_PTR`

These are the concrete routines that turn a host-assigned Bank5_6 script pointer into a multi-player exchange sequence.

## Semantics of the snap-exchange slice

## 1. Snap initiation stays host-owned in Bank19_20

Bank21_22 does not decide when the play begins.

The host bank owns the snap gate:
- `CHECK_FOR_P2_HIKE_P1_PLAYER_CHANGE` and related `DEFENDER_CHANGE_BEFORE_HIKE` flow decide pre-snap defender switching and, for man offense, the actual hike button gate.
- `CHECK_FOR_SNAP_P2DEF_CHANGE_P1PUNT`, `CHECK_FOR_SNAP_P1DEF_CHANGE_P2PUNT`, and `COM_VS_COM_SNAP_DELAY` do the equivalent for punt snap timing.
- `SET_BALL_SNAPPED_START_CLOCK_EXCEPT_XP` is the host routine that flips the snap/play-status bit and starts the clock when appropriate.

So the modern runtime boundary should stay explicit:

- **OnField host/coordinator responsibility:** decide when the snap occurs and stamp the snapped state.
- **Player command runtime responsibility:** react to that snapped state when the player’s current command is a snap-related instruction.

## 2. `CENTER_HIKE_COMMAND_START` is a blocker on host snap state, not the actual snap decision

`CENTER_HIKE_COMMAND_START`:
- faces the snapping player toward the line
- swaps to the center-hike sprite
- loops until `PLAY_STATUS` reports the snapped bit
- then restores normal sprite state and waits a short post-snap delay before continuing

Important detail: this command does **not** set possession or move the ball to the QB. It is the snapping player’s local animation/wait gate.

Runtime meaning:
- enter a `CenterSnapInitiatorCommand`
- wait on host `BallSnapped` state
- once set, finish a short post-snap animation delay
- complete and allow the center to continue into the next command

## 3. `SHOTGUN_HIKE_COMMAND_START` stages the long-snap release, then still waits on host snap state

`SHOTGUN_HIKE_COMMAND_START` does more than the under-center variant:
- sets the ball collision/status to “ready to leave hand”
- seeds the ball position from the current snapping player
- waits until `PLAY_STATUS` reports snapped
- plays the shotgun snap sound
- waits a longer exchange delay (`shotgun_snap_delay_frames = $1E`) before continuing

Important detail: this still does not make the QB the ball carrier. It stages the snap-release side and leaves possession to the later receive command.

Runtime meaning:
- enter a `ShotgunSnapInitiatorCommand`
- seed a snap-release ball state from the snapper
- wait on host `BallSnapped`
- hold through the source-timed snap travel window
- complete without yet granting offensive possession to the QB runtime actor

## 4. `RECEIVE_SNAP_*` commands are the point where the receiver becomes ball carrier

### Under center
`RECEIVE_SNAP_CENTER_COMMAND_START`:
- immediately updates the manual-control pointer and displayed player name to the current receiver
- loops until snapped state is visible
- calls `SET_PLAYER_BALL_CARR_UPDATE_POSS_STATUS`
- waits a short receive delay
- continues to the next command

### Shotgun
`RECEIVE_SNAP_SHOTGUN_COMMAND_START`:
- also pre-assigns manual-control pointer and displayed name
- waits for snapped state
- calls `SET_SHOTGUN_LOCATION_DO_ANIMATION`
- waits until ball collision confirms the ball reached the QB
- ends the ball animation, then calls `SET_PLAYER_BALL_CARR_UPDATE_POSS_STATUS`
- waits a short receive delay
- continues

### FG/XP holder
`RECEIVE_SNAP_FG_XP_COMMAND_START` is the same structural pattern for the holder:
- face/pose as holder
- wait for snap
- start the long-snap ball animation
- wait for ball collision with holder
- mark holder as ball carrier
- wait for the kick to occur, then drop back out of ball-carrier state

Runtime meaning:
- the command runtime needs distinct `UnderCenterSnapReceiveCommand`, `ShotgunSnapReceiveCommand`, and `FieldGoalSnapReceiveCommand` semantics.
- all three are resumable wait commands.
- the transition from “snap in progress” to “this player now owns the ball” happens inside these handlers, not in the host.

## 5. `HANDOFF_COMMAND_START` and `FAKE_HANDOFF_COMMAND_START` are two-player command redirections with shared QB staging

`HANDOFF_COMMAND_START` and `FAKE_HANDOFF_COMMAND_START` only tag the target slot and hand off to `HANDOFF_COMMAND_LOGIC`.

`HANDOFF_COMMAND_LOGIC` then:
- stores the target player slot in the current player’s RAM
- stops the QB in place
- clears the displayed-name icon and starts a handoff icon timer
- turns the QB toward the target runner
- plays the shared handoff/toss start animation

Then the flow splits:

### Regular handoff
If the target is valid and not collided/on-ground:
- the QB is marked not-man and not-ball-carrier
- the target runner is retargeted via `UPDATE_LOCAL_PLAYER_COMMAND_ADDR_IF_VALID` to `RB_RECEIVES_HANDOFF_START`
- the QB stays in the handoff animation through the source-timed delay and then resumes normal command stepping

`RB_RECEIVES_HANDOFF_START` then:
- calls `SET_PLAYER_BALL_CARR_UPDATE_POSS_STATUS`
- updates the manual-control pointer to the runner
- updates the visible control icon for the correct side
- plays two receive-handoff animation phases with source-timed delays
- returns to normal command stepping

### Fake handoff
If the high bit is set on the target slot:
- the target runner is retargeted to `RB_FAKE_HANDOFF_ANIMATION`
- displayed-name status is restored to not-changing
- no ball-carrier transfer occurs in the target continuation
- both players simply play the shared fake exchange timing and then resume command stepping

Runtime meaning:
- the command runtime needs a `BackfieldHandoffCommand` with a `isFake` variant flag.
- that command must be able to retarget another offensive player into a continuation command while the current player also remains in a timed continuation.
- the actual possession transfer belongs to the continuation entered by the target runner, not to the initial decode of opcode `5x`/`6x`.

## 6. `PITCH_BALL_COMMAND_START` is a two-stage live ball transfer rather than an immediate carrier swap

`PITCH_BALL_COMMAND_START` stores the pitch target and jumps into `PITCH_COMMAND_LOGIC`.

`PITCH_COMMAND_LOGIC`:
- stops the QB and explicitly marks the QB as current ball carrier first
- turns the QB toward the target runner
- stages a first ball animation for leaving the hand
- waits until that release phase finishes
- computes the final pitch path and starts the moving-ball task toward the target
- clears the QB’s man-control and ball-carrier state
- if the target is valid and upright, retargets that target runner to `WAIT_FOR_PLAYER_RECEIVES_PITCH`
- waits a fixed completion delay before the passer resumes command stepping

`WAIT_FOR_PLAYER_RECEIVES_PITCH` then:
- repeatedly sets the target runner as ball carrier / updates manual-control pointer / updates displayed name while waiting for ball collision
- once the ball collides, ends the animation and returns to normal command stepping

Runtime meaning:
- the pitch is not a synchronous possession swap.
- the runtime needs a `PitchBallCommand` that creates a transient in-flight ball state and a `ReceivePitchContinuation` on the target runner.
- the source explicitly allows the pitch target retarget to be skipped when the target is invalid, collided, or on the ground.

## Recommended Bank19_20 → Bank21_22 runtime boundary for this packet

## Host entry responsibilities that stay in the current Bank19_20 flow

### 1. Initial script-family installation
Bank19_20’s `LOAD_UPDATE_PLAY_CODE_FUNCTIONS` is still the host-side seed step that copies the selected Bank5_6 script pointers into each player’s `PLAY_CODE_ADDR`, sets `COMMAND_COUNTER`, and points resumable execution back to `JUMP_DO_NEXT_PLAYER_COMMAND`.

That means the modern host should continue to own:
- initial offensive/defensive script installation
- bulk mid-play script reassignment
- re-entry of affected players into the Bank21_22-style stepper

### 2. Snap gating before per-player snap commands resolve
For this packet, the current host flow that should enter the runtime remains:
- `DEFENDER_CHANGE_BEFORE_HIKE` for regular pre-snap defender-selection and manual hike timing
- `CHECK_SNAP_PUNT` for punt snap timing
- `SET_BALL_SNAPPED_START_CLOCK_EXCEPT_XP` as the authoritative state flip the command runtime watches

So the command runtime should be stepped only after the host has already installed the player scripts and is driving normal per-player updates.

### 3. Pass-context preparation remains host-owned
`SET_PLAYERS_CLOSE_TO_PASS` is not part of this packet’s exchange semantics, but it remains a host-owned follow-on preparation step after the ball is live and before later pass-interaction commands need ranked receiver/defender context.

That matters because the snap/handoff/pitch slice should not absorb pass-target-ranking responsibilities that still live in Bank19_20.

## Recommended production-facing runtime names for this slice

These are recommendations only; no code changes are proposed here.

### Host-side concepts
- `OnFieldPlayCoordinator` — current Bank19_20 host analogue
- `PlayAssignmentService` — current Bank19_20 script-family installer/reassigner
- `BallSnapGate` or `SnapTransitionState` — host-owned snapped/not-snapped state visible to command handlers

### Command-runtime concepts
- `PlayerCommandRuntime.StepNextCommand()` — `DO_NEXT_PLAYER_COMMAND` analogue
- `CenterSnapInitiatorCommand`
- `ShotgunSnapInitiatorCommand`
- `UnderCenterSnapReceiveCommand`
- `ShotgunSnapReceiveCommand`
- `FieldGoalSnapReceiveCommand`
- `BackfieldHandoffCommand`
- `ReceiveHandoffContinuationCommand`
- `FakeHandoffContinuationCommand`
- `PitchBallCommand`
- `ReceivePitchContinuationCommand`

### Shared runtime services implied by the source
- `BallStateService`
- `ManualControlService`
- `PlayerPresentationService`
- `PlayerCommandRedirector`

## Stepping contract from the current Bank19_20 host flow

A source-faithful stepping contract for this packet looks like this:

1. Bank19_20 installs offensive and defensive script entry pointers through `LOAD_UPDATE_PLAY_CODE_FUNCTIONS`.
2. Each player’s per-frame update re-enters Bank21_22 through the `JUMP_DO_NEXT_PLAYER_COMMAND`/`DO_NEXT_PLAYER_COMMAND` path.
3. Pre-snap host logic in `DEFENDER_CHANGE_BEFORE_HIKE` or `CHECK_SNAP_PUNT` decides when to call `SET_BALL_SNAPPED_START_CLOCK_EXCEPT_XP`.
4. Any active `CENTER_HIKE_COMMAND_START`, `SHOTGUN_HIKE_COMMAND_START`, or `RECEIVE_SNAP_*` command keeps yielding until that snapped state becomes visible.
5. The receive command, not the host, is the place where the QB/holder becomes ball carrier.
6. If the next offensive command is `HANDOFF_COMMAND_START`, `FAKE_HANDOFF_COMMAND_START`, or `PITCH_BALL_COMMAND_START`, the current player command can redirect a second offensive player into a continuation command through `UPDATE_LOCAL_PLAYER_COMMAND_ADDR_IF_VALID`.
7. Both players then continue stepping independently through `DO_NEXT_PLAYER_COMMAND` once their timed continuation states finish.

## Main packet takeaway

For packet 21A, the cleanest bounded offensive semantics slice is:

- **host-owned snap gate in Bank19_20**
- **Bank21_22-owned snap receipt and immediate backfield transfer continuations**

That preserves the source boundary exactly where it matters:
- Bank19_20 decides when the live exchange begins.
- Bank21_22 decides how the center/QB/holder/runner commands wait, transfer control, transfer possession, and resume command stepping across multiple players.
