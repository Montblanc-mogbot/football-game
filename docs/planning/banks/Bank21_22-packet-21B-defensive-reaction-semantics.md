# Bank21_22 packet 21B — defensive reaction semantics

Updated: 2026-06-03

## Purpose

This note documents one bounded defensive reaction family from `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm`:

- timed man-coverage assignment
- line-of-scrimmage mirror behavior before the runner clears the line
- aggressive and conservative chase reactions once the defense breaks to the ball carrier

The goal is to pin down the runtime semantics of this family with exact source references and keep the current Bank19_20 host/runtime seam explicit.

## Why this slice is bounded enough for packet 21B

This family is source-coherent because the commands all express one defensive responsibility chain:

1. Bank19_20 assigns or retargets a defender into a coverage/pursuit script.
2. Bank21_22 stores defender-local target metadata.
3. the command runtime loops frame-to-frame until the defender should mirror, chase, or dive.

That gives us one production-facing defensive reaction family without trying to cover every tackle, interception, or loose-ball branch in the bank.

## Commands covered

### Coverage entry
- `MAN_COVERAGE_TIGHT_COMMAND_START` (`0x`) — store a target receiver plus timed tight coverage behavior
- `MAN_COVERAGE_LOOSE_COMMAND_START` (`1x`) — same targeting shape, but sets the loose-coverage high bit before entering the same coverage runtime

### Pre-LOS tracking
- `MIRROR_BALL_CARRIER_WHILE_BEHIND_LOS_COMMAND_START` (`DB`) — vertically mirror the ball carrier while staying patient behind the line of scrimmage

### Post-read pursuit
- `CHASE_BALL_AGRESSIVE_COMMAND_START` (`DA`) — direct chase with frequent dive checks
- `CHASE_BALL_CARRIER_CONSERVATIVE_COMMAND_START` (`DD`) — pursuit that biases toward smaller turn corrections based on the carrier's path

## Source anchors inside Bank21_22

### Coverage-target setup
- `MAN_COVERAGE_TIGHT_COMMAND_START` / `MAN_COVERAGE_LOOSE_COMMAND_START` at `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:1462-1476`
- shared save path `SET_MAN_COVERAGE_DEFEND_TIME` at `...asm:1465-1471`

These commands do not perform coverage themselves. They only write defender-local state:
- target player slot to `EXTRA_PLAYER_RAM_1`
- coverage duration/selector to `EXTRA_PLAYER_RAM_3`
- optional loose-coverage flag via bit `$80`

Then they tail-jump into `DEFNDER_MAN_TO_MAN_PASS_COVERAGE_START`, which is the actual runtime-owned coverage loop.

### Mirror and chase handlers
- `MIRROR_BALL_CARRIER_WHILE_BEHIND_LOS_COMMAND_START` at `...asm:2644-2687`
- `CHASE_BALL_AGRESSIVE_COMMAND_START` at `...asm:2618-2635`
- `CHASE_BALL_CARRIER_CONSERVATIVE_COMMAND_START` at `...asm:2714-2772`
- `CHASE_CONSERVATIVE_TURN_TABLE` at `...asm:3364-3383`

### Shared helper semantics this family depends on
- `UPDATE_DIR_RESET_SPEED_GRTR_THAN_45DEG_CHANGE`
- `UPDATE_PLAYER_SPRITE_NORMAL`
- `INIT_PLAYER_VELOCITY_CUR_SPEED`
- `CHECK_FOR_DIVE[chance]`
- `GET_PLAYER_DIRECTION_TOWARDS_BALL`
- `GET_DIRECTION_TO_TARGET`
- `COPY_MAN_PLAYER_ADDR_TO_TEMP`
- `CHANGE_COM_BALL_CARRIER_DIRECTION_MAX_OF_NINE_DEGREES` at `...asm:3329-3362`

Those helpers make the family clearly runtime-driven rather than data-only: every command is a resumable loop that repeatedly recomputes direction, speed, and dive attempts.

## Semantics of the defensive reaction family

## 1. Man-coverage commands are stateful defender assignments, not one-shot moves

`MAN_COVERAGE_TIGHT_COMMAND_START` and `MAN_COVERAGE_LOOSE_COMMAND_START` are compact setup commands.
They:
- read the target offensive player slot from `PLAYER_COMMAND_ARG1`
- read a timing/control byte from `PLAYER_COMMAND_ARG2`
- store both into defender-local RAM
- jump immediately into `DEFNDER_MAN_TO_MAN_PASS_COVERAGE_START`

The loose variant does one extra thing before the shared jump:
- ORs the target slot with `looser_coverage_flag = $80`

Runtime meaning:
- this should become a `ManCoverageAssignmentCommand`, not a generic move command
- the command sets up per-defender coverage state that the later coverage loop consumes
- the `loose` vs `tight` distinction belongs in the decoded command state, likely as a `coverageLeeway` or `coverageMode` flag rather than as a raw `$80` bit in production code

## 2. Mirror-behind-the-line is a patient pre-commit reaction, not full pursuit

`MIRROR_BALL_CARRIER_WHILE_BEHIND_LOS_COMMAND_START` expresses a very specific defensive posture.
It:
- compares defender Y against `BALL_Y`
- moves only up/down until the defender is within `half_yard = ONE_YARD / 2`
- uses a short initial delay (`INIT_MIRROR_BC_DELAY_FRAMES = 3`) while closing
- once aligned, stops movement, restores a left/right facing pose, and waits with a longer reaction delay (`MIRROR_BC_DELAY_FRAMES = 13`)
- restarts the loop if the ball carrier drifts outside the half-yard vertical window again

Source references:
- close-vertical loop at `...asm:2646-2666`
- stop-and-wait posture at `...asm:2669-2677`
- `@player_within_half_yard_ball_check` at `...asm:2679-2687`

Runtime meaning:
- this is best represented as a `MirrorBallCarrierBehindLineCommand`
- the command does not try to tackle, pathfind freely, or own run-fit strategy
- it is a resumable reaction state that oscillates between:
  - `closingVertically`
  - `holdingMirrorLane`
- the runtime should expose the semantic threshold (`withinHalfYardY`) and delays, not the raw temporary RAM layout

## 3. Aggressive chase is direct pursuit plus repeated dive pressure

`CHASE_BALL_AGRESSIVE_COMMAND_START` is structurally simple but behaviorally important.
It loops forever through:
- `GET_PLAYER_DIRECTION_TOWARDS_BALL`
- `UPDATE_DIR_RESET_SPEED_GRTR_THAN_45DEG_CHANGE`
- sprite/velocity refresh
- short delay (`dive_delay_frames = 5`, with `RETURN_IN_NUM_FRAMES_PLUS_0_TO_3` randomness)
- `CHECK_FOR_DIVE[chance]` with `dive_chance = $99` (commented as 60%)

If the dive is not taken, it immediately restarts chase.
If the dive is taken, it waits 30 frames and then restarts chase.
If the direction helper somehow errors, it waits 5 frames and retries.

Source references:
- main loop at `...asm:2618-2631`
- error retry at `...asm:2633-2635`

Runtime meaning:
- this should become an `AggressiveBallCarrierChaseCommand`
- the command is not “run to target once”; it is a perpetual pursuit loop with periodic dive opportunities
- the dive decision is command-owned continuation logic, not a Bank19_20 host decision

## 4. Conservative chase predicts the carrier more gently and limits turn severity

`CHASE_BALL_CARRIER_CONSERVATIVE_COMMAND_START` differs from the aggressive version in two ways.

### It prefers the live ball carrier when one exists
When `BALL_COLLISION` indicates the ball is not loose/in-flight, it:
- copies the current offense-controlled player address with `COPY_MAN_PLAYER_ADDR_TO_TEMP`
- checks whether that target is moving
- if moving, compares target direction with defender-to-target direction
- indexes `CHASE_CONSERVATIVE_TURN_TABLE` to bias toward a smaller adjustment

If the carrier is stationary, it falls back to `GET_DIRECTION_TO_TARGET` directly.
If the ball is loose/in-flight, it falls back to `GET_PLAYER_DIRECTION_TOWARDS_BALL`.

### It constrains turn changes before the same dive check
After the pursuit direction is chosen, it still:
- updates direction/speed/sprite/velocity
- waits 5 plus random frames
- uses the same 60% dive gate

But the chosen direction is less abrupt because `CHASE_CONSERVATIVE_TURN_TABLE` and the related angle math intentionally smooth the turn.

Source references:
- ball-vs-carrier branch at `...asm:2716-2724`
- moving-carrier turn-table path at `...asm:2728-2747`
- shared dive tail at `...asm:2757-2772`
- conservative-turn table at `...asm:3368-3383`

Runtime meaning:
- this should become a distinct `ConservativeBallCarrierChaseCommand`
- its identity is not just “lower dive chance” — the source keeps the same dive gate
- the real semantic difference is pursuit steering: smaller, carrier-informed angular corrections before the same chase/dive loop continues

## 5. This whole family is resumable command-runtime work, not coordinator flow

All four commands above:
- mutate defender-local continuation state
- wait on frame delays
- recompute movement repeatedly
- keep running until another condition or reassignment interrupts them

That means they belong naturally in the future `PlayerCommandRuntime` layer described in `docs/planning/banks/Bank21_22-runtime-representation.md`, not in `OnFieldPlayCoordinator`.

## Connection to the current Bank19_20 host/runtime boundary

## 1. Bank19_20 still owns assignment and retargeting

The current host-side installation seam remains `LOAD_UPDATE_PLAY_CODE_FUNCTIONS` in `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank19_20_on_field_gameplay_loop.asm:3243-3436`.
That is already mirrored in:
- `src/FootballGame/Gameplay/OnField/Services/PlayAssignmentService.cs`
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/CommandRuntimeBoundaryHoldingArea.cs`

For this defensive family, that means Bank19_20 should continue to own:
- choosing which defenders receive man-coverage or chase-family scripts
- bulk reassignment during turnovers, returns, and other host-side transition moments

The future runtime should only step the assigned commands after those pointers are installed.

## 2. Pre-snap defender selection remains host-owned, but it feeds directly into this family

`DEFENDER_CHANGE_BEFORE_HIKE` in `Bank19_20_on_field_gameplay_loop.asm:2718-2966` is still the host-side snap/pre-control seam.
Current MonoGame owner:
- `src/FootballGame/Gameplay/OnField/Services/PreSnapControlService.cs`

Why it matters for this packet:
- that host flow chooses the manually controlled defender before snap
- the chosen defender is exactly the kind of player who may then enter `ManCoverageAssignmentCommand` or `MirrorBallCarrierBehindLineCommand` once Bank21_22 stepping resumes

So the future seam should be:
- `PreSnapControlService` decides/control-hands-off
- `PlayerCommandRuntime` resumes the defender’s assigned defensive reaction command

## 3. Pass-collision setup is the current host entry into later defensive pass reactions

`SET_PLAYERS_CLOSE_TO_PASS` in `Bank19_20_on_field_gameplay_loop.asm:3580-3856` is the present Bank19_20 pass-contest bridge.
Current MonoGame owner:
- `src/FootballGame/Gameplay/OnField/Services/PassTargetingService.cs`

That routine ranks the receiver and nearby defenders, then redirects selected players to:
- `JUMP_WR_JUMP_DIVE_CHECK_PASS`
- `JUMP_DEF_JUMP_DIVE_CHECK_PASS`

Even though the jump/dive pass-contest handlers are outside this packet’s chosen family, the connection matters:
- the same Bank19_20 host-side service that orders pass defenders should remain host-owned
- the reaction after that ordering still belongs to the command runtime, just like the coverage and chase commands above

## Recommended production-facing runtime names for this family

To stay aligned with the production-facing naming rule, this packet should map to names like:
- `ManCoverageAssignmentCommand`
- `MirrorBallCarrierBehindLineCommand`
- `AggressiveBallCarrierChaseCommand`
- `ConservativeBallCarrierChaseCommand`
- `DefensiveReactionContinuationState`

Not:
- `Bank21_22ManCoverage`
- `DbCommand`
- `DaCommand`
- `DdCommand`

## Boundary summary

### Bank19_20 host side keeps owning
- script-family installation and reassignment via `PlayAssignmentService`
- pre-snap defender-control handoff via `PreSnapControlService`
- pass-collision candidate ordering via `PassTargetingService`

### Future player-command runtime should own
- defender-local coverage target/timer state
- mirror/chase loop stepping
- directional steering updates inside the command
- dive-attempt timing and continuation waits
- eventual interruption/completion when the defender is reassigned or the play transitions elsewhere

## Bottom line

This defensive packet shows that Bank21_22 does not merely contain generic “AI pursuit.”
It contains a command-driven defensive reaction family with distinct semantics:
- assignment into timed man coverage
- patient mirror behavior behind the line
- aggressive direct chase
- conservative carrier-informed pursuit smoothing

The current Bank19_20 seam already supports this split cleanly:
- Bank19_20 chooses and retargets the defenders
- the future `PlayerCommandRuntime` should execute the defensive reaction commands frame by frame
