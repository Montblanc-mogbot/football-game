# Bank21_22 packet 21C — pass-target-not-close interception edge case

Updated: 2026-06-03

## Purpose

This note documents one command-driven interaction edge case from `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm`:

- a pass target can jump or dive, miss the ball, and still leave a defender-only interception window
- that follow-on window uses a distinct no-receiver-near pass-result path with an explicit source bug

The goal is to keep the runtime semantics and the Bank19_20 bridge assumptions explicit before any production `PlayerCommand*` runtime code exists.

## Why this is the right 21C slice

This is a true interaction edge case instead of a normal command family:

- it only appears after Bank19_20 has already primed a pass contest through `SET_PLAYERS_CLOSE_TO_PASS`
- it depends on Bank21_22 receiver and defender jump/dive command continuations interacting with shared pass-result state
- it includes a source-visible bug in the no-receiver-near interception calculation, so a modern runtime must not accidentally “clean it up” without an explicit decision

That makes it a good packet boundary: narrow, source-grounded, and important enough to preserve deliberately.

## Source anchors

### Bank19_20 host-side priming
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank19_20_on_field_gameplay_loop.asm:3580-3856` — `SET_PLAYERS_CLOSE_TO_PASS`
  - ranks the receiver plus nearby defenders
  - seeds `COMMAND_COUNTER = 1`
  - redirects participants to `JUMP_WR_JUMP_DIVE_CHECK_PASS - 1` or `JUMP_DEF_JUMP_DIVE_CHECK_PASS - 1`

### Bank21_22 bridge wrappers
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:206-210`
  - `BANK_JUMP_WR_JUMP_DIVE_CHECK_PASS`
  - `BANK_JUMP_DEF_JUMP_DIVE_CHECK_PASS`

### Bank21_22 offensive pass-contest flow
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:4759-5119`
  - `OFFENSE_JUMP_DIVE_CATCH_PASS_START`
  - `CHECK_FOR_INT_AFTER_WR_MISS_DIVE_CATCH`
  - `DO_RECEIVER_PASS_DIVE`
  - `DO_PASS_TARGET_JUMP`

### Bank21_22 defensive pass-contest flow
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:5125-5262`
  - `DEFENSE_JUMP_DIVE_CATCH_PASS_START`
  - `CHECK_DEFENDER_CLOSE_TO_PASS_BALL_COLLIDING`
  - `DO_DEFENDER_DIVE_FOR_PASS`
  - `DO_DEFENDER_JUMP_FOR_PASS`

### Shared pass-result calculation path
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:5454-5590`
  - `NORMAL_PASS_RESULT_CALCULATION`
  - `TARGET_JUMP_DIVE_PASS_CALC_START`
  - `PASS_CALCULATION_TARGET_NOT_CLOSE_TO_BALL`
- `reference/Tecmo_Super_Bowl_NES_Disassembly/Bank21_22_play_commands_on_field_logic.asm:5552-5554`
  - source comment: `***MAJOR BUG!! $DC NOT LOADED WITH PASS CONTROL VALUE`

## Edge-case runtime semantics

## 1. Bank19_20 creates a one-shot pass-contest continuation, not a full pass result

`SET_PLAYERS_CLOSE_TO_PASS` does host-side ranking only.
It does **not** resolve the catch, tip, or interception itself.

Instead it primes a one-step command continuation for:
- the current pass target via `JUMP_WR_JUMP_DIVE_CHECK_PASS`
- the selected nearby defenders via `JUMP_DEF_JUMP_DIVE_CHECK_PASS`

Runtime meaning:
- the host still owns participant ordering
- the command runtime owns the actual pass-contest execution and result resolution after that priming step

## 2. The receiver-side continuation can explicitly fall into a defender-only interception check

In `OFFENSE_JUMP_DIVE_CATCH_PASS_START`, the target receiver:
- updates toward the projected ball location
- may jump if the final-ball distance falls within the jump band and `CHECK_IF_PLAYER_CLOSE_ENOUGH_TO_JUMP_FOR_BALL` passes
- may dive if no defenders are marked close and the pass-timer/final-ball-distance checks fall within the dive window
- otherwise can stop at the final location and wait for collision

The important edge-case branch is after a failed receiver dive or miss:
- `DO_RECEIVER_PASS_DIVE` can call `PASS_CALCULATION_TARGET_NOT_CLOSE_TO_BALL` when the target is not close enough to catch (`...asm:4975`, `...asm:4991`)
- `CHECK_FOR_INT_AFTER_WR_MISS_DIVE_CATCH` then only resumes normal command stepping if `BALL_CAUGHT_BITFLAG` is set; otherwise it pauses the receiver (`...asm:4911-4919`)

Runtime meaning:
- a failed receiver dive/jump is not automatically just an incompletion
- it can hand control to a separate “receiver missed, defenders may still steal this” resolution path

## 3. The no-receiver-near path is defender-priority interception logic, not generic loose-ball logic

`PASS_CALCULATION_TARGET_NOT_CLOSE_TO_BALL` runs when the target is not close enough to catch.
Its behavior is specific:

- if all three defender pass-status entries still read `NO_PASS_COLLISION_STATUS`, the ball becomes tipped/incomplete immediately (`...asm:5556-5560`)
- otherwise it checks the first, second, then third prioritized defenders in order (`...asm:5562-5588`)
- each defender gets a pass-control/reception/interception roll through `PC_REC_INT_ADJUST_BY_RAND_FOR_PASS_OUTCOME`
- if the outcome is below `TARGET_NOT_CLOSE_INT_THRESHOLD`, the flow goes straight to `INTERCEPTION_NORMAL`
- if no prioritized defender qualifies, the ball falls through to tipped/incomplete

Runtime meaning:
- this is a ranked defender-only interception window driven by the already-primed pass-collision ordering
- it is not equivalent to “ball is now free and anyone can recover it”
- defender priority order from the host bridge remains semantically important

## 4. The source bug must stay explicit

The source itself flags this path as buggy:

- `PASS_CALCULATION_TARGET_NOT_CLOSE_TO_BALL` is marked `***MAJOR BUG!! $DC NOT LOADED WITH PASS CONTROL VALUE` (`...asm:5552-5554`)
- each defender check reloads `LDX PASS_CONTROL_VALUE` with the note `*** BUG` (`...asm:5562`, `...asm:5572`, `...asm:5581`)

The nearby-target pass-result paths call `ADD_PASS_CONTROL_SKILL_AND_REC_SKILL` before defender-impact checks (`...asm:5470`), but this no-target-near branch does not refresh the same scratch state.
So its interception math depends on stale scratch/register state instead of a fully reloaded pass-control value.

Runtime implication:
- the first production `PlayerCommand*` implementation must make this a conscious policy choice
- acceptable future choices are:
  1. preserve the bug intentionally for parity
  2. fix it intentionally and document the divergence
- what is **not** acceptable is silently normalizing this into the clean nearby-target path and losing the fact that the original game had a separate buggy branch

## 5. Receiver and defenders do not all resume identically after this edge case

The source distinguishes outcomes after the miss window:
- receiver-side `CHECK_FOR_INT_AFTER_WR_MISS_DIVE_CATCH` pauses the receiver unless the ball was actually caught by offense (`...asm:4911-4919`)
- defender-side `PASS_RESULT_ALREADY_DO_NEXT_DEF_COMMAND` resumes the defender command stream once ball collision or pass result is known (`...asm:5212-5215`)
- actual interception finalization then flows through `PASS_INTERCEPTED` (`...asm:5610-5652`)

Runtime meaning:
- the command runtime needs per-participant continuations, not one shared pass-contest coroutine that completes every actor the same way
- offensive and defensive participants observe the same shared ball result, but their own command continuations branch differently afterward

## Bank19_20 bridge assumptions that must remain explicit

### 1. `PassTargetingService` still owns ranking, not result resolution

Current code:
- `src/FootballGame/Gameplay/OnField/Services/PassTargetingService.cs`
- `OrderPassCollisionPlayers` records `OnFieldRoutine.SET_PLAYERS_CLOSE_TO_PASS`

This service should keep owning:
- choosing the current receiver candidate
- ordering the first/second/third defender priorities
- priming the contest participants for command-runtime entry

It should **not** absorb the Bank21_22 catch/tip/interception semantics.
Those belong on the command-runtime side.

### 2. `CommandRuntimeBoundaryHoldingArea` must continue to carry the pass-contest seam

Current file:
- `src/FootballGame/Gameplay/OnField/CommandRuntimeBridge/CommandRuntimeBoundaryHoldingArea.cs`

Its current deferred routine/symbol set already contains the exact seam this packet relies on:
- `OnFieldRoutine.SET_PLAYERS_CLOSE_TO_PASS`
- `JUMP_WR_JUMP_DIVE_CHECK_PASS`
- `JUMP_DEF_JUMP_DIVE_CHECK_PASS`

That bridge must stay explicit until real command-runtime code exists, because this edge case starts with host priming and only finishes inside Bank21_22 continuations.

### 3. Bank19_20 should not flatten this into ordinary pass-outcome host flags

The current Bank19_20 review notes already describe pass-target ordering as host prep rather than final result ownership:
- `docs/planning/banks/Bank21_22-runtime-representation.md`
- `docs/reviews/Bank19_20-runtime-code-review-2026-06-03.md`

That assumption should remain explicit when live runtime code is added:
- host side may know a pass contest is active
- host side may queue presentation or return-phase routing after a final outcome
- but the specific miss-dive → defender-only interception window still belongs to `PlayerCommandRuntime` semantics

## Recommended production-facing naming for this slice

If/when implemented, keep the runtime names gameplay-facing rather than packet/bank-facing, for example:
- `ReceiverPassContestContinuation`
- `DefenderPassContestContinuation`
- `PassResultResolver`
- `ReceiverMissedBallInterceptionWindow`

The important thing is the boundary and behavior, not the source bank number.

## Bottom line

This edge case is: **receiver miss or failed dive/jump can still hand the pass to a ranked defender-only interception window, and that window includes a source-visible bug**.

That means the future runtime must keep three things explicit:
- Bank19_20 owns pass-contest participant ranking and command-pointer priming
- the command runtime owns the miss/jump/dive/interception semantics after entry
- the buggy no-receiver-near interception math must be preserved or fixed only by explicit policy, never by accidental cleanup
