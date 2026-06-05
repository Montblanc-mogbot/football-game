# Bank21_22 completion audit — 2026-06-05

## Scope
- Audited the remaining Bank21_22 dispatcher targets after slices A-C against the source `GROUP_COMMAND_TABLE` in `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm`.
- Verified which command families are now represented in the production-facing runtime seam (`OnFieldPlayCoordinator`, `CommandRuntimeBoundaryHoldingArea`, `PlayerCommandRuntime`, and the bounded dispatcher/handler families under `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/`).
- Determined whether a final bounded leftover family was ready to implement without inventing a parallel integration path.

## Coverage confirmed by this audit
The current runtime seam now covers the major live dispatcher families that were the point of the active Bank21_22 task:
- defensive reactions (`MAN_COVERAGE_*`, `CHASE_BALL_*`, `MIRROR_BALL_CARRIER_*`)
- offensive exchange / snap receive (`RECEIVE_SNAP_*`, `HANDOFF`, `FAKE_HANDOFF`, `PITCH` plus explicit retarget continuations)
- pass contest / interception window (`OFFENSE_JUMP_DIVE_CATCH_PASS_START`, `DEFENSE_JUMP_DIVE_CATCH_PASS_START`, `CHECK_FOR_INT_AFTER_WR_MISS_DIVE_CATCH`)
- movement targets (`MOVE_RELATIVE`, `MOVE_ABS_VS_SNAP_LOC`, `MOVE_ABS_VS_MIDDLE`)
- player control handoff (`COM_CONTROL_BALL_CARRIER`, `MAN_TAKE_CONTROL`)
- control flow (`DO_ACTION_IF_COM`, `COM_JUMP_BASED_ON_JUICE`, `IF_COM_JUMP`, `BRANCH`, `JUMP`)
- presnap / targeting (`PRE_SNAP_MOTION_COMMAND_START`, `SET_TARGET_ORDER_COMMAND`)
- quarterback / pass control (`QB_DROPBACK_COMMAND_START`, `COM_WAIT_TO_PASS_COMMAND_START`, `COM_PASS_COMMAND_START`)
- special teams / return (`SET_AND_MOVE_KICKOFF_COMMAND_START`, `KICKOFF_COMMAND_START`, `PUNT_COMMAND_START`, `KICK_FG_COMMAND_START`, `KICK_XP_COMMAND_START`, `RETURN_KICK_PUNT_COMMAND_START`)

## Remaining dispatcher targets and blocker classification
The remaining `GROUP_COMMAND_TABLE` entries are real Bank21_22 commands, but they do **not** share the currently exposed host/runtime seam in a bounded way. They fall into four leftover buckets:

1. **Generic posture / wait / per-player stat mutators**
   - `CENTER_HIKE_COMMAND_START`, `SHOTGUN_HIKE_COMMAND_START`
   - `THREE_PT_STANCE_COMMAND_START`, `FORMATION_SHIFT_COMMAND_START`, `TWO_PT_STANCE_COMMAND_START`, `OFF_MOTION_COMMAND_START`, `QB_PRE_SNAP_STANCE_COMMAND_START`
   - `FACE_LOS_COMMAND_START`, `STAND_COMMAND_START`, `TURN_COMMAND_START`, `WAIT_COMMAND_START`
   - `SET_RS_COMMAND_START`, `SET_MS_COMMAND_START`, `INCR_DECR_RS_COMMAND_START`, `INCR_DECR_MS_COMMAND_START`, `SET_HITTING_POWER_COMMAND_START`, `CHANGE_HITTING_POWER_COMMAND_START`

   **Missing hook point:** these commands want a stable host-owned player-entity seam for facing, sprite stance, velocity-zeroing, timed wait loops, and per-player ratings/speed mutation. The current live seam only mirrors coarse command state back onto `OnFieldGameState`; it does not expose a bounded host-side player-motion/stat application interface that would let these commands land cleanly without inventing a second runtime integration layer.

2. **Blocking / collision-mask families**
   - `BLOCK_COMMAND_START`, `CHOP_BLOCK_COMMAND_START`, `PASS_BLOCK_COMMAND_START`
   - `MOVE_AND_BLOCK_RELATIVE_COMMAND_START`, `MOVE_AND_BLOCK_HASH_COMMAND_START`, `MOVE_AND_BLOCK_REL_BALL_CARRIER_COMMAND_START`
   - `CAN_COLLIDE_COMMAND_START`, `CAN_BLOCK_COMMAND_START`

   **Missing hook point:** these commands need a production-facing collision/block target-selection seam (nearby-player queries, blockable/collidable bitmask ownership, and movement-plus-engagement continuation ownership). None of that is currently exposed through `OnFieldPlayCoordinator` / `OnFieldGameState` beyond broad play-phase flags.

3. **Ball-recovery / misc / non-live utility commands**
   - `RECOVER_BALL_COMMAND_START`
   - `RANDOM_COMMAND_START`
   - `CELEBRATE_COMMAND_START`, `CRY_COMMAND_START`
   - `COVER_NEARBY_PLAYERS_COMMAND_START_UNUSED`, `WAIT_RANDOM_TIME_AFTER_SNAP_COMMAND_START_UNUSED`, `INIFITE_LOOP_COMMAND_START`, `UNUSED_COMMAND_START`

   **Missing hook point / priority note:** some are explicitly unused, while the live ones depend on either loose-ball/collision ownership or presentation/animation loops that are not yet modeled as a bounded runtime seam.

4. **No separate completion gap for the slice-A/B/C families**
   - The families targeted by slices A-C no longer have unresolved major dispatcher gaps after this audit.

## Why this slice stopped at docs/blocker form
The task asked for either the final bounded leftover implementation **or** exact blockers. After auditing the remaining dispatch targets, I do **not** think there is a safe “small last family” left that fits the current seam without first adding a new host dependency surface for:
- player-facing / stance / timed animation application,
- collision+block target ownership, or
- loose-ball/player-physics hooks.

Implementing one of the leftover groups now would require inventing a parallel integration path rather than using the existing `OnFieldPlayCoordinator` / `CommandRuntimeBoundaryHoldingArea` seam, which would violate the task direction.

## Validation
- Audited source dispatch targets from `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:331-417` plus the corresponding command bodies cited above.
- Audited live runtime coverage in:
  - `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs`
  - `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/CommandRuntimeBoundaryHoldingArea.cs`
  - `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/PlayerCommandRuntime.cs`
  - bounded dispatcher/handler files under `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/`
- `dotnet build /tmp/Bank21_22Subset.csproj` passed on 2026-06-05.
- `git diff --check` passed on 2026-06-05.
