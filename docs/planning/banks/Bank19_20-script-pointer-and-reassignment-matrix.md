# Bank19_20 — script-pointer and reassignment matrix

Updated: 2026-06-02

## Purpose

This note turns the Bank19_20 inventory into a more implementation-ready map.

The goal is to answer:

- which special script-pointer families live in `Bank19_20_on_field_gameplay_loop.asm`
- what gameplay event or transition causes each family to be assigned
- which side gets the assignment
- whether the assignment includes or excludes the man-controlled player
- what modern service boundary that suggests
- which transitions also need to stay visible when Bank21_22 runtime work resumes

This is still a Bank19_20 note.
It does **not** move the reassignment logic out of this bank.
It just makes the future split between coordinator and services much more explicit.

## Core Bank19_20 assignment primitives

These are the main host-side assignment helpers already visible in Bank19_20:

- `LOAD_P1_OFF_FORMATION`
- `LOAD_P2_OFF_FORMATION`
- `LOAD_P1_DEFENSE_PLAY_CODE_ADDRESSES`
- `LOAD_P2_DEFENSE_PLAY_CODE_ADDRESSES`
- `LOAD_PLAYER_SCRIPT_ADDR_INTO_PLAYER_RAM`
- `UPDATE_ALL_P1_PLAY_CODE_ADRR`
- `UPDATE_ALL_P1_PLAY_CODE_ADRR_EXCEPT_MAN`
- `UPDATE_ALL_P2_PLAY_CODE_ADRR`
- `UPDATE_ALL_P2_PLAY_CODE_ADRR_EXCEPT_MAN`
- `SET_ALL_PLAYER_SCRIPT_ADRR_EXCEPT_MAN`

## Modern reading of those helpers

### Initial play load
Used during kickoff / normal play / punt / FG / XP setup.

Responsibilities:
- choose offensive formation pointer family
- choose defensive execution pointer family
- copy the initial per-player script addresses into player RAM

Suggested modern home:
- `PlayAssignmentService`
- possibly with a narrower `FormationScriptLoader` and `DefensivePlayLoader`

### Mid-play reassignment
Used during interceptions, punt returns, onside recoveries, fumbles, touchdowns, chase-ball-carrier transitions, and similar state changes.

Responsibilities:
- retarget a whole side to a new script family
- optionally exclude the current man-controlled player
- seed Bank21_22 re-entry through `LOAD_UPDATE_PLAY_CODE_FUNCTIONS`

Suggested modern home:
- `PlayAssignmentService`
- called by `OnFieldPlayCoordinator`

### Man-player exception behavior
The `_EXCEPT_MAN` variants are a major source-level rule.

They mean Bank19_20 sometimes wants to retarget:
- all non-man-controlled players immediately
- while preserving the currently controlled player's special behavior/state

That should stay explicit in the MonoGame design.
A likely modern shape is a policy/option on reassignment calls, not a hidden side effect.

---

## Special script-pointer family inventory

## Offensive-side families declared near the top of the bank

| Source label | Address | Purpose |
| --- | --- | --- |
| `OFF_PLAYERS_CHEER_PLAY_PTRS[]` | `$AE00` | offense celebration scripts |
| `INT_RETURN_DEFENSE_PLAY_PTRS[]` | `$AE18` | offense side defending after opponent interception |
| `OFF_RECOVER_ONSIDE_PLAY_PTRS[]` | `$AE30` | offense side trying to recover an onside kick |
| `OFF_RECOVER_BALL_PLAY_PTRS[]` | `$AE48` | offense side chasing/recovering a loose ball |
| `PUNT_COVERAGE_PLAY_PTRS[]` | `$AE60` | punting side coverage scripts |
| `OFF_ONSIDE_KICK_RET_PLAY_PTRS[]` | `$AE78` | offense side returning its own onside recovery |
| `OFF_RECOVERS_OWN_FUM_PLAY_PTRS[]` | `$AEA8` | offense side after recovering its own fumble |
| `OFF_DEFENDS_LOST_FUM_PLAY_PTRS[]` | `$AEC0` | offense side after losing a fumble |
| `OFF_PLAYERS_CRY_PLAY_PTRS[]` | `$AF08` | offense disappointment/cry scripts |

## Defensive-side families declared near the top of the bank

| Source label | Address | Purpose |
| --- | --- | --- |
| `DEF_PLAYERS_CRY_PLAY_PTRS[]` | `$B600` | defense disappointment/cry scripts |
| `INT_RETURN_PLAY_PTRS[]` | `$B618` | defense side after making an interception |
| `DEF_RECOVER_ONSIDE_PLAY_PTRS[]` | `$B630` | defense side trying to recover an onside kick |
| `DEF_RECOVER_BALL_PLAY_PTRS[]` | `$B648` | defense side chasing/recovering a loose ball |
| `PUNT_RETURN_PLAY_PTRS[]` | `$B660` | return side scripts during punt return |
| `DEF_ONSIDE_KICK_RET_PLAY_PTRS[]` | `$B678` | defense side returning an onside recovery |
| `FUM_RET_DEF_PLAY_PTRS[]` | `$B6A8` | defense scripts during fumble return context |
| `DEF_RET_LOST_FUM_PLAY_PTRS[]` | `$B6C0` | defense side after taking possession on a fumble |
| `DEF_PLAYERS_CHEER_PLAY_PTRS[]` | `$B708` | defense celebration scripts |
| `CHASE_BALL_CARRIER_PLAY_PTRS[]` | `$B750` | aggressive pursuit scripts after ballcarrier transition |

---

## Reassignment matrix by trigger/event

## 1) Ballcarrier crosses the line of scrimmage on a broken pass/scramble/run transition

### Trigger
- `CHASE_BALL_CARRIER_PLAY_PTRS[]`
- visible at `P1_OFFENSE_PAST_LOS_SET_D_CHASE` and `P2_OFFENSE_PAST_LOS_SET_D_CHASE`

### Assignment pattern
- defense side gets `CHASE_BALL_CARRIER_PLAY_PTRS[]`
- uses `_EXCEPT_MAN`

### Meaning
Once the offense is committed as a runner, Bank19_20 retargets defenders into a chase/pursuit behavior family.

### Modern boundary
- coordinator decides the runner has crossed the LOS and passing is no longer allowed
- `PlayAssignmentService` applies a `chaseBallCarrier` defensive reassignment

### Bank21_22 carry-forward relevance
Medium.
The actual chase behavior runs in Bank21_22, but the transition trigger is still host-owned.

---

## 2) Punt becomes a live return

### Trigger
- `PUNT_COVERAGE_PLAY_PTRS[]`
- `PUNT_RETURN_PLAY_PTRS[]`
- visible in both P1 and P2 punt flows

### Assignment pattern
- punting side gets `PUNT_COVERAGE_PLAY_PTRS[]`
- return side gets `PUNT_RETURN_PLAY_PTRS[]`
- both usually use `_EXCEPT_MAN`

### Meaning
Once the kick is away and the cutscene delay clears, Bank19_20 retargets both teams into coverage/return behavior families.

### Modern boundary
- coordinator owns the punt-in-flight to return transition
- `PlayAssignmentService` applies a paired `puntCoverage` / `puntReturn` reassignment

### Bank21_22 carry-forward relevance
High.
These scripts become the live per-player behavior immediately after the host transition.

---

## 3) Onside kick recovery race

### Trigger families
- recovery phase:
  - `OFF_RECOVER_ONSIDE_PLAY_PTRS[]`
  - `DEF_RECOVER_ONSIDE_PLAY_PTRS[]`
- return phase after successful recovery:
  - `OFF_ONSIDE_KICK_RET_PLAY_PTRS[]`
  - `DEF_ONSIDE_KICK_RET_PLAY_PTRS[]`

### Assignment pattern
#### Before recovery is known
- kicking/receiving sides are retargeted into onside recovery pursuit families
- uses `_EXCEPT_MAN`

#### After recovery is known
- recovering side gets the corresponding onside return family
- opposing side gets the corresponding defensive return family
- recovering side may switch from `_EXCEPT_MAN` to full update depending on control needs

### Meaning
Bank19_20 splits onside handling into:
1. loose-ball recovery race
2. post-recovery live return or immediate possession change

### Modern boundary
- `SpecialTeamsRecoveryService` or `PlayAssignmentService`
- coordinated by `OnFieldPlayCoordinator`

### Bank21_22 carry-forward relevance
High.
This is one of the clearest examples where host transitions install an entirely new Bank5_6 runtime context.

---

## 4) Interception return

### Trigger families
- intercepting defense gets `INT_RETURN_PLAY_PTRS[]`
- former offense gets `INT_RETURN_DEFENSE_PLAY_PTRS[]`

### Assignment pattern
- both sides usually get full `UPDATE_ALL_*_PLAY_CODE_ADRR`
- active player is included

### Meaning
After an interception, Bank19_20 immediately converts the play into a turnover-return context rather than merely flipping possession flags.

### Modern boundary
- coordinator detects interception outcome
- `PlayAssignmentService` applies a paired `interceptionReturn` / `interceptionReturnDefense` reassignment

### Bank21_22 carry-forward relevance
Very high.
This is one of the most important host-to-runtime bridge transitions in the bank.

---

## 5) Touchdown celebration / defeat reactions

### Trigger families
- offense TD path:
  - scoring side may get `OFF_PLAYERS_CHEER_PLAY_PTRS[]`
  - opponent may get `DEF_PLAYERS_CRY_PLAY_PTRS[]`
- defensive TD path:
  - scoring side may get `DEF_PLAYERS_CHEER_PLAY_PTRS[]`
  - opponent may get `OFF_PLAYERS_CRY_PLAY_PTRS[]`

### Assignment pattern
- scoring side often gets full update
- losing side often uses `_EXCEPT_MAN`

### Meaning
Bank19_20 treats touchdown aftermath as an explicit script-family swap, not only a banner/cutscene path.

### Modern boundary
- `OnFieldPlayCoordinator` owns the touchdown state transition
- `PlayAssignmentService` handles celebration/cry script installation
- `OnFieldPresentationService` handles banners/music/cutscenes

### Bank21_22 carry-forward relevance
Medium.
The runtime executes these families, but the trigger is clearly host-owned.

---

## 6) Loose-ball recovery race during fumble

### Trigger families
- `OFF_RECOVER_BALL_PLAY_PTRS[]`
- `DEF_RECOVER_BALL_PLAY_PTRS[]`

### Assignment pattern
- both sides get recovery/chase families
- uses `_EXCEPT_MAN`

### Meaning
When the ball is loose, Bank19_20 does an immediate broad reassignment so the field behaves like a loose-ball scramble rather than the original play call.

### Modern boundary
- coordinator detects the fumble event
- `PlayAssignmentService` applies `recoverLooseBall` families to both sides

### Bank21_22 carry-forward relevance
High.
This is a runtime-context reset, not a minor tweak.

---

## 7) Own-fumble recovery vs opponent-fumble recovery

### Trigger families after the ball is recovered
#### Recovering your own fumble
- recovering offense gets `OFF_RECOVERS_OWN_FUM_PLAY_PTRS[]`
- opposing side gets `FUM_RET_DEF_PLAY_PTRS[]`

#### Recovering opponent fumble
- new possessing side gets `DEF_RET_LOST_FUM_PLAY_PTRS[]`
- former possessing side gets `OFF_DEFENDS_LOST_FUM_PLAY_PTRS[]`

### Assignment pattern
- recovering/new-possessing side often gets full update
- other side often uses `_EXCEPT_MAN`

### Meaning
Bank19_20 distinguishes:
- “play continues with original possession restored”
- “turnover return now underway”

That distinction matters because the script families differ.

### Modern boundary
- coordinator asks a fumble/possession outcome service what happened
- `PlayAssignmentService` applies either `recoverOwnFumble` or `turnoverAfterFumble` families

### Bank21_22 carry-forward relevance
Very high.
This is another major host-installed runtime context switch.

---

## 8) Initial play setup families

### Trigger helpers
- `LOAD_P1_OFF_FORMATION`
- `LOAD_P2_OFF_FORMATION`
- `LOAD_P1_DEFENSE_PLAY_CODE_ADDRESSES`
- `LOAD_P2_DEFENSE_PLAY_CODE_ADDRESSES`

### Assignment pattern
- offense loads Bank3/B5-side formation/play family
- defense loads Bank4/B6-side defensive play family
- `LOAD_PLAYER_SCRIPT_ADDR_INTO_PLAYER_RAM` copies the starting per-player script addresses into RAM

### Meaning
This is the canonical first assignment for a play before any mid-play reassignment happens.

### Modern boundary
- `PlayAssignmentService` initial-load path
- called from `OnFieldPlayCoordinator` during kickoff/play-start/FG/XP/punt setup

### Bank21_22 carry-forward relevance
Very high.
This is the initial handoff that seeds the script runner at all.

---

## Update policy patterns visible in the source

## Full update vs `_EXCEPT_MAN`
A stable pattern in Bank19_20 is:

- **full update** when the host wants to force a new immediate runtime context for everyone, including the active/man-controlled player
- **`_EXCEPT_MAN` update** when the host wants to retarget everyone else but preserve the special current control context for the man player

That suggests the future service API should probably make this explicit, for example with a reassignment policy like:
- `includeManControlledPlayer`
- `excludeManControlledPlayer`

rather than hiding it in different methods with unclear meaning.

## Paired side updates
Many of the important transitions are inherently paired:
- one family for the possessing/returning side
- one family for the defending/pursuing side

That suggests the service should have a concept closer to:
- assign both sides for transition X

instead of requiring the coordinator to micromanage every individual per-side reassignment call.

---

## Recommended modern service split

Based on this matrix, the cleanest service picture inside the Bank19_20 conversion is probably:

### `PlayAssignmentService`
Owns:
- initial formation/defensive-play script loads
- bulk player script address installation
- paired offense/defense reassignment transitions
- man-player inclusion/exclusion policy

### `OnFieldPlayCoordinator`
Owns:
- deciding when the event happened
- deciding which transition family applies
- invoking the service with the right reassignment policy

### Possibly narrower helpers later
If needed later, `PlayAssignmentService` could split into:
- `InitialPlayScriptLoader`
- `TransitionScriptReassignmentService`
- `ManControlAssignmentPolicy`

But right now one explicit assignment service is enough.

---

## Bank21_22 carry-forward set reinforced by this matrix

The most important Bank19_20-originated bridge areas still are:

- `LOAD_UPDATE_PLAY_CODE_FUNCTIONS`
- `DEFENDER_CHANGE_BEFORE_HIKE`
- `CHECK_SNAP_PUNT`
- `SET_PLAYERS_CLOSE_TO_PASS`

This matrix strengthens why they matter:
- they are the host-side mechanisms that actually install or prime the next Bank21_22 runtime context
- they define when the interpreter should resume from a new script family
- they define when the active player should be treated specially versus bulk-retargeted

## Practical takeaway

Bank19_20 is the game's **script assignment authority**.

Bank21_22 is the **script execution authority**.

That split now looks concrete enough to guide the MonoGame implementation without losing any Bank19_20 content.
