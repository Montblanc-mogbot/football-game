# OPENCLAW_TASKS.md

## Project
Football Game (fresh-start MonoGame conversion of Tecmo Super Bowl NES)

## Branch / remote
- Branch: `main`
- Remote: `origin`

## Reset rule
This repo starts fresh.
It contains only:
- the assembly snapshot under `reference/`
- the assembly-first planning/categorization notes created on 2026-05-26

Do not treat prior MonoGame implementation work as part of this repository.
If old code is ever consulted or imported from a separate repo, that must be an explicit future decision.

## Source of truth
- Original disassembly snapshot: `reference/Tecmo_Super_Bowl_NES_Disassembly/`
- Planning docs:
  - `docs/planning/conversion-inventory.md`
  - `docs/planning/bank-parity-status-matrix.md`
  - `docs/planning/critical-bank-conversion-packets.md`
  - `docs/planning/source-bank-conversion-checklist.md`
- Coding standards:
  - `docs/coding-standards.md`

## Active tasks
- [x] Decide the default policy for consulting or reusing code from the old MonoGame repo. Acceptance: one short note states whether the default is no reuse, selective packet-by-packet reuse, or broader reuse only with explicit source-bank justification.
  - Evidence: added `docs/planning/old-code-reuse-policy.md`, which sets the default to **no reuse by default** and requires any later reuse to be justified packet-by-packet against a named source-bank responsibility.
- [x] Turn the assembly-first planning docs into a clean restart sequence. Acceptance: one short durable note orders the first banks/packets to tackle from scratch.
  - Evidence: added `docs/planning/restart-sequence.md`, which orders the fresh-start conversion through a Phase 1 gameplay-parity backbone (`Bank1_2`, `5A`, `5C`, `21A`, `17A`, `19A`, `12A`) before widening into playcall, post-play, stats, collision, and later meta-state slices.
- [x] Choose the first bounded conversion packet for actual implementation. Acceptance: the task names the packet id, the source bank, the expected output/artifact, and whether old code may be consulted.
  - Evidence: added `docs/planning/first-conversion-packet.md`, which selects packet `5A` from `Bank5_6_off_def_play_data.asm`, defines the expected packet-analysis note + initial modern data artifact + validation note, and explicitly says old MonoGame code should **not** be consulted for this first packet.
- [x] Capture `Bank1_2_team_data.asm` data semantics needed by packet `5A`. Acceptance: leave one durable note naming the exact team/player data structures or enums that `5A` depends on, with any unresolved semantics called out explicitly.
  - Evidence: added `docs/planning/packets/5A-team-data-semantics.md`, which traces `OFFENSE_PLAYER_REACTION_091`'s `PLAYER_COMMAND_DATA_RB2` / `PLAYER_COMMAND_DATA_RB1` targets back to the shared player-command slot enum in `macros/play_data_macros.asm`, the per-team roster pointer order in `Bank1_2_team_data.asm` (`STARTING_ADDR_FOR_TEAM_PLAYER_NAMES_PTR_TABLE`, `BUFFALO_LIST` as the concrete layout example), and the aligned per-team ability blocks (`BUFFALO_BILLS_ABILITIES`). The note explicitly calls out unresolved semantics around exact rating-byte decoding and where runtime consumption of those ratings is handled.

- [x] Convert `Bank1_2_team_data.asm` end-to-end as the first full-bank pass. Acceptance: the repo gains (a) a durable Bank1_2 structure note, (b) source-faithful extracted artifacts for team roster ordering and player ability layout, (c) decoded C# semantic models for roster slots, player identity, and position-group ability records, and (d) a short validation note explaining how the converted artifacts map back to the bank sections.
  - Evidence: added `docs/planning/banks/Bank1_2-structure-and-representation.md`, `content/game-data/teams/roster-ordering.yaml`, `content/game-data/teams/ability-layout.yaml`, `src/FootballGame/GameData/Teams/Models/*.cs`, `src/FootballGame/GameData/Teams/TeamDataSamples.cs`, and `docs/planning/validation/Bank1_2-conversion-validation.md`.
- [x] Define the Bank1_2 source-data representation strategy before broad extraction. Acceptance: one durable note states which Bank1_2 structures remain source-shaped, which are decoded semantically, and why pointer mechanics are not preserved literally in C#.
  - Evidence: `docs/planning/banks/Bank1_2-structure-and-representation.md` explicitly defines the source-faithful extracted layer, decoded semantic layer, runtime-consumption layer, and the rule to preserve pointer semantics without carrying pointer mechanics into C#.
- [x] Extract the full canonical roster-slot ordering and team/player identity layer from Bank1_2. Acceptance: one reviewable artifact preserves all teams, slot order, jersey numbers, and source names without flattening away the bank’s ordering semantics.
  - Evidence: `content/game-data/teams/roster-ordering.yaml` preserves canonical team/slot order, `development-tools/bank1_2/extract_bank1_2.py` generates `content/game-data/teams/generated/team-identities.json`, and the generated output covers all 28 teams × 30 slots = 840 identity records. `PlayerIdentityRecord` / `TeamRosterRecord` remain the decoded semantic layer.
- [x] Extract and decode the full Bank1_2 ability layer by position-group schema. Acceptance: one reviewable artifact plus typed C# records preserve the per-position record widths, nibble-based attribute scale, and aligned per-team ability blocks.
  - Evidence: `content/game-data/teams/ability-layout.yaml` preserves the slot-aligned ability contract, `development-tools/bank1_2/extract_bank1_2.py` generates `content/game-data/teams/generated/team-abilities.json` plus `ability-metadata.json`, `AttributeGrade` preserves nibble semantics, and `AbilityRecords.cs` / `TeamAbilitySet.cs` provide the decoded typed records across all 28 team ability blobs / 840 slot records.
- [x] Keep packet `5A` aligned with the full-bank Bank1_2 conversion. Acceptance: reconcile `docs/planning/packets/5A-team-data-semantics.md` and `src/FootballGame/Conversion/PlayScripts/OffensivePitchPlayFamily.cs` against the final Bank1_2 models without consulting the older MonoGame repo.
  - Evidence: the existing `5A` team-data note now matches the canonical `RosterSlot` / `TeamId` / Bank1_2 ability-schema direction introduced in the new Bank1_2 models, and no old-repo code was consulted.
- [x] Add a loader/decoder bridge from generated Bank1_2 artifacts into the semantic C# models. Acceptance: one loader reads the generated Bank1_2 JSON artifacts and returns typed roster/ability data without reintroducing pointer mechanics.
  - Evidence: added `src/FootballGame/GameData/Teams/TeamDataJsonLoader.cs`, `src/FootballGame/GameData/Teams/TeamDataSet.cs`, and `docs/planning/banks/Bank1_2-loader-layer.md`.

## Notes
- This task file intentionally avoids carrying forward old completed tasks, old validation campaigns, or old architecture milestones.
- Future tasks should be source-bank-first, not old-code-first.
- All implementation/refactor work should follow `docs/coding-standards.md` and the packet flow in `docs/planning/source-bank-conversion-checklist.md`.
