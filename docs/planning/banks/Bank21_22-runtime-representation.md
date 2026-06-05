# Bank21_22 — runtime representation

Updated: 2026-06-03

## Purpose

This note defines the production-facing runtime names and ownership boundaries for the future execution layer that will consume Bank5_6 reaction scripts through `Bank21_22_play_commands_on_field_logic.asm`.

The goal is to keep the Bank21_22 runtime naming coherent with the already-checked-in Bank19_20 host flow code:
- use gameplay-facing names rather than `Bank21_22*` production type names
- keep the Bank19_20 host/runtime boundary explicit
- identify the exact hook-up points already visible in current code and source

## Naming direction

Bank21_22 is the per-player behavior-command execution layer, not a bank-shaped gameplay coordinator.
So the production names should describe **what the runtime does**, not which source bank it came from.

Recommended production-facing names:

- `PlayerCommandRuntime`
  - top-level service that advances player command execution within the live on-field host
- `PlayerCommandExecutionContext`
  - per-player mutable runtime state for command stepping
- `PlayerCommandPointer`
  - source-faithful cursor over the current Bank5_6 command stream for one player
- `PlayerCommandDecoder`
  - reads opcode/operands, distinguishes group vs single commands, and returns a typed command descriptor
- `PlayerCommandDefinition`
  - decoded command identity plus typed operands and control-flow metadata
- `PlayerCommandContinuation`
  - resumable multi-frame state for commands that wait, animate, or poll conditions
- `PlayerCommandDispatcher`
  - maps decoded commands to the handler that owns their semantics
- `PlayerCommandServices`
  - facade or dependency bundle exposing ball/player/control/presentation/query helpers needed by command handlers

These names stay aligned with current runtime naming such as `OnFieldPlayCoordinator`, `OnFieldGameState`, and `PlayAssignmentService`.
They avoid introducing `Bank21_22` into the production runtime surface while still remaining source-traceable in docs/comments.

## Why these names fit the actual source boundary

`DO_NEXT_PLAYER_COMMAND` in `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:240` is mechanically a per-player stepper:
- choose offense/defense script bank
- read the player's `PLAY_CODE_ADDR`
- decode opcode and operands
- advance the per-player command pointer by command length
- dispatch into the command logic table

That maps more naturally to `PlayerCommandRuntime` + `PlayerCommandDecoder` + `PlayerCommandDispatcher` than to a bank-numbered type.

## Exact Bank19_20 hook-up points already present

The current integration seam is already visible in both source and MonoGame code.

### 1. Bulk command-pointer installation

Source anchor:
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank19_20_on_field_gameplay_loop.asm:3243-3436` (`LOAD_UPDATE_PLAY_CODE_FUNCTIONS`)

What it does:
- copies per-player script pointers into player RAM
- seeds `COMMAND_COUNTER`
- seeds the resume target to `JUMP_DO_NEXT_PLAYER_COMMAND - 1`

Cross-bank bridge symbols:
- `JUMP_DO_NEXT_PLAYER_COMMAND` (`Bank19_20...asm:22`)
- Bank21_22 entry wrapper at `reference/.../Bank21_22_play_commands_on_field_logic.asm:200-201`

Current MonoGame hook:
- `src/FootballGame/Gameplay/OnField/Services/PlayAssignmentService.cs`
- `LoadKickoffScripts`, `LoadPlaySelectionScripts`, and all `ReassignFor*` methods record `OnFieldRoutine.LOAD_UPDATE_PLAY_CODE_FUNCTIONS`

Runtime implication:
- `PlayAssignmentService` should stay the owner of **which script families get assigned**
- the future `PlayerCommandRuntime` should own **what happens after those assignments are installed and stepped**

### 2. Pre-snap defender handoff into command execution

Source anchor:
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank19_20_on_field_gameplay_loop.asm:2718-2966` (`DEFENDER_CHANGE_BEFORE_HIKE`)

What it does:
- chooses the manually controlled defender before snap
- writes that defender's `PLAY_CODE_ADDR` to `$8018`
- seeds `COMMAND_COUNTER = 1`
- writes the per-player command return address to `$8000 - 1`
- snaps the ball and resumes command processing

Current MonoGame hook:
- `src/FootballGame/Gameplay/OnField/Services/PreSnapControlService.cs`
- `PrepareRegularPlayForSnap` records `OnFieldRoutine.DEFENDER_CHANGE_BEFORE_HIKE`

Runtime implication:
- pre-snap control remains host-owned in Bank19_20 / `PreSnapControlService`
- but the snap transition must eventually create or resume a `PlayerCommandExecutionContext` for the active defender and the rest of the field

### 3. Punt snap gating before scripted execution resumes

Source anchor:
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank19_20_on_field_gameplay_loop.asm:2968-3243` (`CHECK_SNAP_PUNT`)

What it does:
- waits for manual or CPU punt snap timing
- marks the ball snapped
- returns after the snap gate is satisfied

Current MonoGame hook:
- `src/FootballGame/Gameplay/OnField/Services/PreSnapControlService.cs`
- `PreparePuntForSnap` records `OnFieldRoutine.CHECK_SNAP_PUNT`

Runtime implication:
- punt snap timing remains a host-flow concern
- after that gate clears, the same future `PlayerCommandRuntime` should resume the player command layer rather than duplicating punt-specific interpreter entry logic elsewhere

### 4. Pass-collision retargeting into command handlers

Source anchor:
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank19_20_on_field_gameplay_loop.asm:3580-3856` (`SET_PLAYERS_CLOSE_TO_PASS`)

What it does:
- ranks nearby defenders and the target receiver
- writes `COMMAND_COUNTER = 1`
- writes either `JUMP_DEF_JUMP_DIVE_CHECK_PASS - 1` or `JUMP_WR_JUMP_DIVE_CHECK_PASS - 1` into player command state

Cross-bank bridge symbols:
- `JUMP_WR_JUMP_DIVE_CHECK_PASS` (`Bank19_20...asm:23`)
- `JUMP_DEF_JUMP_DIVE_CHECK_PASS` (`Bank19_20...asm:24`)
- Bank21_22 wrappers at `reference/.../Bank21_22_play_commands_on_field_logic.asm:206-209`

Current MonoGame hook:
- `src/FootballGame/Gameplay/OnField/Services/PassTargetingService.cs`
- `OrderPassCollisionPlayers` records `OnFieldRoutine.SET_PLAYERS_CLOSE_TO_PASS`

Runtime implication:
- `PassTargetingService` should continue to own host-side ranking/query prep
- the future command runtime should own the resumed jump/dive command execution that follows that priming step

## Current code boundary artifacts that already model this seam

### `CommandRuntimeBoundaryHoldingArea`

Current file:
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/CommandRuntimeBoundaryHoldingArea.cs`

This already captures the right four deferred Bank19_20 bridge routines:
- `DEFENDER_CHANGE_BEFORE_HIKE`
- `CHECK_SNAP_PUNT`
- `LOAD_UPDATE_PLAY_CODE_FUNCTIONS`
- `SET_PLAYERS_CLOSE_TO_PASS`

It also preserves the cross-bank jump symbols:
- `JUMP_DO_NEXT_PLAYER_COMMAND`
- `JUMP_WR_JUMP_DIVE_CHECK_PASS`
- `JUMP_DEF_JUMP_DIVE_CHECK_PASS`

That holding area should remain the explicit seam until real Bank21_22 runtime code exists.

### `OnFieldRoutineOwnershipMap`

Current file:
- `src/FootballGame/Gameplay/OnField/OnFieldRoutineOwnershipMap.cs`

This is already the correct host-side ownership map:
- `PlayAssignmentService` owns command-pointer installation / reassignment triggers
- `PreSnapControlService` owns pre-snap snap gates and defender switching
- `PassTargetingService` owns pass-collision candidate ordering

Those owners should stay host-side even after Bank21_22 runtime implementation begins.
The new runtime layer should plug into them rather than absorb them.

## Recommended production split

### Host-owned Bank19_20 side

Keep these responsibilities with the existing host/runtime layer:
- play-family entry and dead-ball routing in `OnFieldPlayCoordinator`
- script-family selection and reassignment in `PlayAssignmentService`
- snap gating and manual defender pre-snap control in `PreSnapControlService`
- pass-target / nearby-defender ordering in `PassTargetingService`

### New Bank21_22 execution side

Move these responsibilities into the future command runtime layer:
- per-player command pointer state
- group vs single opcode decoding
- command-length advancement
- typed opcode dispatch
- multi-frame resumable command execution
- branch/jump/control-flow semantics *(bounded live slice now implemented for `DO_ACTION_IF_COM`, `COM_JUMP_BASED_ON_JUICE`, `IF_COM_JUMP`, `BRANCH`, and `JUMP`; see `docs/planning/validation/Bank21_22-control-flow-command-runtime-validation.md`)*
- command-local wait/timer/continuation state

## Integration shape to target next

When Bank21_22 runtime code starts, the safest first production-facing seam is:

1. `PlayAssignmentService` installs a script family for each player
2. `PreSnapControlService` / `PassTargetingService` can request targeted command-pointer redirects
3. `OnFieldPlayCoordinator` advances the live play host phase
4. `PlayerCommandRuntime` steps eligible player execution contexts for that frame
5. command handlers call back into focused gameplay services rather than mutating unrelated host state directly

That keeps current Bank19_20 host ownership intact while giving Bank21_22 a clearly named, production-facing execution layer.

## Bottom line

The coherent production-facing name for the Bank21_22 layer is not a `Bank21_22*` class family.
It is a `PlayerCommand*` runtime split rooted in the actual source behavior of `DO_NEXT_PLAYER_COMMAND`.

The key existing integration points are already present and should remain explicit:
- `PlayAssignmentService` for script installation
- `PreSnapControlService` for snap-time handoff
- `PassTargetingService` for pass-collision retargeting
- `CommandRuntimeBoundaryHoldingArea` for the still-deferred host/interpreter seam
