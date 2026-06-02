# Bank19_20 code-quality review — 2026-06-02

Scope: `src/FootballGame/Gameplay/OnField/*` reviewed against `docs/coding-standards.md` for stability, principledness, and coding-standards adherence.

## Findings

1. **`OnFieldPlayCoordinator` is continuing to grow into a god class.**  
   `docs/coding-standards.md` says to keep methods split by responsibility and avoid giant convenience classes, and the Bank19_20 class-sketch note explicitly argues against one giant play engine. `OnFieldPlayCoordinator` currently owns nearly every host transition plus many outcome-specific orchestration details in a single 1,100+ line class, which raises regression risk and makes parity review harder as more edge cases land here. See `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs:11-1128` and `docs/planning/banks/Bank19_20-and-Bank21_22-monogame-class-sketch.md` (“Patterns to avoid”, “One giant PlayEngine class”).

2. **`AdvanceActivePlayPhase` can misroute turnover returns because it checks `PlayType` before `TurnoverReturnActive`.**  
   The method routes regular passing, kickoff, punt, and kick flows before checking whether an interception/turnover return is active. If `TurnoverReturnActive` is true while `PlayType` still reads `Regular` with manual passing enabled, or another prior play type remains set, the coordinator can skip the turnover-return handler and continue the wrong flow. That is a stability problem in the main dispatcher. See `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs:193-233`.

3. **Possession-change trace ownership is duplicated between the coordinator and `StatAccountingService`.**  
   `ApplyPossessionChange` records `CHECK_FOR_FIRST_DOWN_OR_TOD` and `UPDATE_HASHMARK_FOR_NEXT_SNAP` directly, then calls `StatAccountingService.ResetSeriesAfterTurnover` and `SpotBallAndUpdateHashForNextSnap`, which record the same routines again. Besides double-tracing, this blurs responsibility boundaries the standards ask to keep explicit. Either the coordinator or the service should own those source-routine recordings, not both. See `src/FootballGame/Gameplay/OnField/OnFieldPlayCoordinator.cs:1068-1078` and `src/FootballGame/Gameplay/OnField/Services/StatAccountingService.cs:31-40`.

4. **Several Bank19_20 services are still silent placeholders rather than guarded domain operations.**  
   The standards prefer loud failures on invalid assumptions and explicit validation over silent fallback. Many service methods currently just stamp routine history, assign loose string keys, and log an event without checking prerequisites or state invariants—for example `PlayAssignmentService`, `OnFieldPresentationService`, and `InjuryCutsceneService`. That leaves invalid combinations easy to represent and hard to detect during future runtime integration. See `src/FootballGame/Gameplay/OnField/Services/PlayAssignmentService.cs:21-95`, `src/FootballGame/Gameplay/OnField/Services/OnFieldPresentationService.cs:26-136`, and `src/FootballGame/Gameplay/OnField/Services/InjuryCutsceneService.cs:24-50`.

5. **Stringly-typed runtime keys are spreading through core gameplay state.**  
   `OnFieldGameState` stores formation, defensive play, banner, song side, and cutscene/reason identifiers as nullable strings, and multiple services build those keys with interpolation or raw literals. That weakens traceability and makes typo-driven bugs likely in exactly the host state the standards say should stay explicit and easy to verify. See `src/FootballGame/Gameplay/OnField/OnFieldGameState.cs:23-31`, `src/FootballGame/Gameplay/OnField/Services/PlayAssignmentService.cs:26-40`, `src/FootballGame/Gameplay/OnField/Services/OnFieldPresentationService.cs:30-31,39,47,56,63,79,85,91,97,104,111,117,123,129`, and `src/FootballGame/Gameplay/OnField/Services/InjuryCutsceneService.cs:30-33`.

## Overall

The Bank19_20 slice is traceable and much less placeholder-only than earlier passes, but the biggest code-health risks now are coordinator sprawl, dispatcher ordering hazards, duplicated routine ownership, and placeholder services that still lack invariant enforcement.