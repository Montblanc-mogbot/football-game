# Critical bank conversion packets

Updated: 2026-05-26

This note turns the six critical parity banks from `conversion-inventory.md` into bounded conversion packets that future coding tasks can execute without falling back to broad vertical-slice work.

## Shared validation baseline

Use the smallest relevant loop for the packet instead of blindly running everything:

- Build: `dotnet build src/TecmoSB.sln`
- Core scrimmage regression pack: `dotnet run --project src/TecmoSBGame -- --headless-scrimmage-pack`
- Determinism check when a packet changes authoritative football behavior: `dotnet run --project src/TecmoSBGame -- --headless-determinism-check <scenario> 3`
- Runtime evidence when a packet changes visible flow/UI: `dotnet run --project src/TecmoSBGame -- --runtime-capture 240`
- Save/season round trips when a packet touches meta persistence: `dotnet run --project src/TecmoSBGame -- --save-roundtrip artifacts/save-roundtrip` and/or `dotnet run --project src/TecmoSBGame -- --season-meta-flow artifacts/season-meta-flow`

---

## Bank5_6_off_def_play_data.asm

### Packet 5A — offensive script parity audit for one formation family
- **Source bank:** `Bank5_6_off_def_play_data.asm`
- **Responsibility:** verify that one bounded offensive play family in `content/playdata/bank5_6_play_data.yaml` preserves the original assignment/route/command intent instead of only producing a plausible modern play.
- **Scope:** choose one offensive family already exercised by headless scenarios; compare its extracted script structure against the disassembly and current compiled/runtime command path.
- **Acceptance:** leave behind one explicit mapping note or code fix showing which original commands/roles are now represented faithfully and which remain deferred.
- **Credible validation:** `dotnet build src/TecmoSB.sln`; run the smallest headless scenario that uses the audited play; if behavior changed, add/update an asserted scenario or determinism artifact.

### Packet 5B — defensive script parity audit for one pressure/coverage family
- **Source bank:** `Bank5_6_off_def_play_data.asm`
- **Responsibility:** port one bounded defensive script family so slot assignments, pursuit intent, or coverage directives match the original bank semantics more closely.
- **Scope:** stay inside one family such as a pressure call, man-coverage call, or zone shell already present in YAML.
- **Acceptance:** the chosen defensive family has an explicit original-to-runtime mapping and a deterministic scenario proving the intended behavior actually fires.
- **Credible validation:** `dotnet build src/TecmoSB.sln`; targeted pass or pressure scenario; `dotnet run --project src/TecmoSBGame -- --headless-determinism-check pressure 3` or the closest relevant named scenario.

### Packet 5C — play-data compiler gap closure
- **Source bank:** `Bank5_6_off_def_play_data.asm`
- **Responsibility:** close one concrete unsupported or lossy command translation gap in `PlayDataScriptCompiler` / runtime script execution.
- **Scope:** one command class only; do not broaden into general script-engine cleanup.
- **Acceptance:** the compiler/runtime can represent the chosen command without fallback hand-waving, and the limitation is removed from docs/comments if previously noted.
- **Credible validation:** `dotnet build src/TecmoSB.sln`; targeted scenario using that command; determinism check for the affected scenario.

---

## Bank12_13_sim_update_stats.asm

### Packet 12A — per-play stats parity for one uncovered event family
- **Source bank:** `Bank12_13_sim_update_stats.asm`
- **Responsibility:** port one missing bookkeeping family such as sacks, special-teams stats, return stats, or fumble attribution.
- **Scope:** implement the event capture, state update, and assertions for one family only.
- **Acceptance:** the chosen event class updates authoritative stats state correctly and survives repeated deterministic runs.
- **Credible validation:** `dotnet build src/TecmoSB.sln`; extend a focused headless stats scenario; `dotnet run --project src/TecmoSBGame -- --headless-stats`; determinism check for `stats`.

### Packet 12B — clock/quarter edge-case parity
- **Source bank:** `Bank12_13_sim_update_stats.asm`
- **Responsibility:** port one bounded timing rule cluster that is still likely simplified, such as incomplete-pass clock stops, out-of-bounds handling, scoring clock effects, or end-of-quarter carry logic.
- **Scope:** one rule cluster only, with explicit assertions.
- **Acceptance:** the affected timing rule is authoritative in match state and covered by a named scenario instead of implied by general scrimmage flow.
- **Credible validation:** `dotnet build src/TecmoSB.sln`; add or extend a quarter/drive scenario; run `--headless-quarter-flow` or a new dedicated clock scenario twice for identical output.

### Packet 12C — season bookkeeping parity slice
- **Source bank:** `Bank12_13_sim_update_stats.asm`
- **Responsibility:** deepen one meta/bookkeeping slice that affects season truth, such as standings tiebreak inputs, league leaders aggregation, schedule progression metadata, or stat persistence boundaries.
- **Scope:** one season bookkeeping slice; do not attempt the entire season layer.
- **Acceptance:** the chosen season bookkeeping responsibility is explicit in code and visible in saved/derived season outputs.
- **Credible validation:** `dotnet build src/TecmoSB.sln`; `dotnet run --project src/TecmoSBGame -- --season-roundtrip artifacts/season-roundtrip` or `--season-meta-flow artifacts/season-meta-flow` depending on the slice.

---

## Bank17_18_main_game_loop.asm

### Packet 17A — front-end to exhibition state-machine parity
- **Source bank:** `Bank17_18_main_game_loop.asm`
- **Responsibility:** tighten one bounded transition chain in the main flow controller, such as title → menu → team select → coin toss → kickoff, against the original game-loop semantics.
- **Scope:** one transition chain only.
- **Acceptance:** the chosen chain has explicit state ownership and no hidden debug-only glue; transition conditions are documented in code or a nearby note.
- **Credible validation:** `dotnet build src/TecmoSB.sln`; targeted runtime capture and/or a focused headless flow harness if added; manual notes only if the environment truly requires them.

### Packet 17B — post-play / next-play orchestration parity
- **Source bank:** `Bank17_18_main_game_loop.asm`
- **Responsibility:** port one bounded orchestration responsibility around play end, post-play presentation, continue flow, or kickoff/possession handoff.
- **Scope:** one orchestration seam where the repo still uses simplified glue.
- **Acceptance:** next-state ownership is authoritative and survives repeated multi-play scenarios without duplicate transitions.
- **Credible validation:** `dotnet build src/TecmoSB.sln`; `dotnet run --project src/TecmoSBGame -- --headless-drive`; `dotnet run --project src/TecmoSBGame -- --headless-scrimmage-pack`.

### Packet 17C — pause / timeout / interruption semantics
- **Source bank:** `Bank17_18_main_game_loop.asm`
- **Responsibility:** implement one bounded interruption flow that belongs to the main state machine rather than pure rendering, such as pause-state rules, timeout flow, or halftime return-to-play gating.
- **Scope:** one interruption type only.
- **Acceptance:** entering and leaving the interruption state preserves authoritative match flow and does not corrupt the next playable state.
- **Credible validation:** `dotnet build src/TecmoSB.sln`; targeted runtime capture or dedicated headless harness; re-run the nearest scenario pack to confirm no flow regression.

---

## Bank19_20_on_field_gameplay_loop.asm

### Packet 19A — snap-to-whistle phase parity for one play phase
- **Source bank:** `Bank19_20_on_field_gameplay_loop.asm`
- **Responsibility:** harden one phase boundary in the on-field loop, such as snap startup, live-ball transition, tackle whistle, or dead-ball reset.
- **Scope:** one phase boundary only.
- **Acceptance:** the chosen phase has explicit state transitions/events instead of timing by accident, and the result is asserted in a named scenario.
- **Credible validation:** `dotnet build src/TecmoSB.sln`; run the nearest focused scenario (`--headless-drive`, `--headless-pass-outcomes`, `--headless-fumble`, etc.); determinism check on the touched scenario.

### Packet 19B — ball-flight / loose-ball lifecycle parity
- **Source bank:** `Bank19_20_on_field_gameplay_loop.asm`
- **Responsibility:** port one bounded ball-state lifecycle that still feels simplified, such as punts after muffs, blocked-kick live-ball continuation, lateral-like loose-ball handling if represented, or recovery spotting.
- **Scope:** one ball lifecycle only.
- **Acceptance:** the lifecycle is authoritative from spawn through recovery/dead-ball resolution and has direct scenario coverage.
- **Credible validation:** `dotnet build src/TecmoSB.sln`; targeted kickoff/punt/field-goal/fumble scenario; determinism check on the relevant named scenario.

### Packet 19C — on-field control/AI handoff parity
- **Source bank:** `Bank19_20_on_field_gameplay_loop.asm`
- **Responsibility:** improve one bounded seam where user control, CPU control, and entity ownership transfer during a play.
- **Scope:** one handoff case such as selected defender takeover, ballcarrier control changes, or post-catch control transfer.
- **Acceptance:** the handoff rule is explicit and visibly testable instead of inferred from current movement side effects.
- **Credible validation:** `dotnet build src/TecmoSB.sln`; targeted scenario plus runtime capture if the effect is easiest to see visually.

---

## Bank20_playcall.asm

### Packet 20A — offensive playcall menu parity slice
- **Source bank:** `Bank20_playcall.asm`
- **Responsibility:** port one bounded offense-facing playcall behavior such as grouping/page navigation, cursor behavior, or roster/playbook mapping.
- **Scope:** one menu behavior slice only.
- **Acceptance:** the chosen behavior follows original playcall semantics closely enough that future tasks can build on it without rethinking the menu model.
- **Credible validation:** `dotnet build src/TecmoSB.sln`; runtime capture and/or a focused harness around playcall state publication; re-run the scrimmage pack if the flow feeds live plays.

### Packet 20B — CPU play-selection parity slice
- **Source bank:** `Bank20_playcall.asm`
- **Responsibility:** port one bounded CPU play-selection rule from the original bank, such as situation-based choice constraints or anti-repetition logic.
- **Scope:** one CPU selection rule only.
- **Acceptance:** a deterministic scenario can show the CPU selecting differently because of the new rule, not because of generic randomness.
- **Credible validation:** `dotnet build src/TecmoSB.sln`; add/update a deterministic harness that logs selected plays under fixed situations; repeat-run check for identical outputs.

### Packet 20C — special-teams playcall parity slice
- **Source bank:** `Bank20_playcall.asm`
- **Responsibility:** port one bounded kickoff/punt/FG/PAT selection flow that is currently simplified.
- **Scope:** one special-teams selection path only.
- **Acceptance:** the selected special-teams flow uses explicit playcall state and feeds the correct downstream kickoff/punt/field-goal setup.
- **Credible validation:** `dotnet build src/TecmoSB.sln`; relevant `--headless-kickoff`, `--headless-punt`, or `--headless-field-goal` scenario.

---

## Bank21_22_play_commands_on_field_logic.asm

### Packet 21A — one offensive command-family parity port
- **Source bank:** `Bank21_22_play_commands_on_field_logic.asm`
- **Responsibility:** deepen one offensive command family such as route timing, lead-block logic, handoff pathing, or receiver adjustment behavior.
- **Scope:** one command family only.
- **Acceptance:** the chosen command family is represented explicitly in runtime logic and changes a named scenario in an intentional, repeatable way.
- **Credible validation:** `dotnet build src/TecmoSB.sln`; targeted run/pass scenario; determinism check for the affected scenario.

### Packet 21B — one defensive reaction-family parity port
- **Source bank:** `Bank21_22_play_commands_on_field_logic.asm`
- **Responsibility:** deepen one defensive behavior family such as pursuit angle updates, zone handoff rules, contain behavior, or tackle approach logic.
- **Scope:** one defensive family only.
- **Acceptance:** the defensive reaction is no longer a generic nearest-target approximation for the chosen case and is covered by an asserted scenario.
- **Credible validation:** `dotnet build src/TecmoSB.sln`; targeted pressure/coverage/run-defense scenario; determinism check on the chosen scenario.

### Packet 21C — collision/interaction semantics for one command-driven edge case
- **Source bank:** `Bank21_22_play_commands_on_field_logic.asm`
- **Responsibility:** port one interaction edge case where play commands and on-field physics meet, such as contested catches, gang-tackle resolution, or block-shed timing.
- **Scope:** one edge case only.
- **Acceptance:** the edge case is modeled as an intentional rules path with scenario evidence, not an emergent accident from generic movement/collision code.
- **Credible validation:** `dotnet build src/TecmoSB.sln`; extend the nearest existing scenario or add one targeted headless case; run determinism check.

---

## Suggested execution order

1. Bank5 packets first when the repo reveals a concrete script/compiler mismatch.
2. Bank21 packets next when on-field command semantics are the limiting factor.
3. Bank19 and Bank17 packets when gameplay flow breaks between command execution and authoritative match state.
4. Bank20 packets when play selection/menu semantics, not core simulation, are the next parity gap.
5. Bank12 packets continuously in parallel as bookkeeping gaps surface from the behavior ports above.

## Planning rule for future tasks

Future Tecmo tasks should name the packet id or create a sibling packet in this format. Avoid broad asks like "improve gameplay" when the real unit of work is a bank responsibility with a specific validation path.
