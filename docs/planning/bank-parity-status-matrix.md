# Tecmo Super Bowl MonoGame bank parity status matrix

Updated: 2026-05-26

## Purpose

This note answers the project-level question: **how much of the real NES-to-MonoGame conversion is already done?**

It compares the original disassembly banks in `/home/montblanc/repos/football-game/reference/Tecmo_Super_Bowl_NES_Disassembly` against the current MonoGame repo at `/home/montblanc/repos/football-game`, using `conversion-inventory.md` as the baseline for what each bank is supposed to mean.

## Status scale

- `documented` — the bank is recognized and mapped, but current repo coverage is mainly notes/reference extraction or passive loaders with little claim of runtime parity.
- `scaffolded` — source data and/or structural code exists, but behavior is mostly plumbing, placeholders, or not yet meaningfully exercised.
- `partial` — some real runtime or content behavior exists, but large parts of the bank’s original responsibility are still absent or only loosely represented.
- `substantial` — the repo has meaningful implemented behavior/data coverage for the bank’s main responsibility, even if parity is not complete.
- `missing` — no credible repo area currently claims to cover the bank in a useful way.

## Repo-level reading of current progress

Broadly:

- **Core football simulation banks are the strongest area**: several critical banks are already `substantial`, especially the on-field play loop, play commands, playcall flow, and main match-state orchestration.
- **Data extraction / YAML conversion coverage is broad**: nearly every major bank now has a corresponding YAML/content representation and loader path.
- **Presentation/meta banks lag parity**: cutscenes, scripted presentation, menu completeness, leaders/records presentation depth, and some season/meta semantics are still mostly `documented`, `scaffolded`, or `partial`.
- **Audio/content banks are represented but not deeply parity-validated**: enough exists to prove hookup, not enough to claim faithful reproduction.

## Matrix

| Bank | Inventory role | Current status | Evidence in repo | Notes |
| --- | --- | --- | --- | --- |
| `Bank1_2_team_data.asm` | team/player data, attributes, names | `substantial` | `content/teamdata/bank1_2_team_data.yaml`; `src/TecmoSB/TeamDataModels.cs`; `src/TecmoSB/TeamDataYamlLoader.cs`; `src/TecmoSBGame/SimArch/Spawning/TeamRoster.cs` | Core roster/team data is extracted and actively consumed by runtime spawning; parity of every original table/semantic is not proven, but this is beyond mere scaffolding. |
| `Bank3_formation_metatile_data.asm` | formations + some metatile data | `substantial` | `content/formations/bank3_formation_metatile_data.yaml`; `src/TecmoSB/FormationData*`; `src/TecmoSBGame/SimArch/Spawning/FormationSpawner.cs`; `FormationPositioningSystem.cs` | Formation semantics are actively used by the runtime. Raw background/metatile concerns are intentionally not literal parity targets. |
| `Bank4_def_spec_play_pointers_data.asm` | defense/special-teams play pointers | `partial` | `content/defenseplays/bank4_defense_special_pointers.yaml`; `src/TecmoSB/DefPlayListPointer*`; `src/TecmoSB/DefensePlay*`; kickoff/punt/FG systems in `src/TecmoSBGame/SimArch/Systems/` | Data extraction exists and special teams are now playable, but pointer-level parity to the original defensive/special-teams selection machinery is not yet clearly demonstrated. |
| `Bank5_6_off_def_play_data.asm` | offensive/defensive play scripts | `substantial` | `content/playdata/bank5_6_play_data.yaml`; `src/TecmoSB/PlayData*`; `PlayDataScriptCompiler.cs`; `PlayScriptSystem.cs`; `PlaySpawner*.cs` | This bank feeds real runtime behavior and is part of the implemented scrimmage core. Still not safe to call complete parity. |
| `Bank7_scene_scripts.asm` | cutscene/static scene scripts | `scaffolded` | `content/scenescripts/bank7_scene_scripts.yaml`; `src/TecmoSB/SceneScript*` | Bank 7 is extracted and modeled, but current repo evidence does not show broad runtime scene-script execution as a parity feature. |
| `bank8_scene_scripts.asm` | season/static screen scripts | `documented` | `src/TecmoSB/SceneScript*`; no dedicated `content/scenescripts/bank8_*.yaml` equivalent found | The project clearly knows bank 8 matters, but current repo coverage appears weaker than bank 7 and does not yet present a distinct bank-8 conversion artifact. |
| `Bank9_sprite_scripts.asm` | sprite animation scripts | `partial` | `content/spritescripts_bank9/bank9_sprite_scripts.yaml`; `src/TecmoSB/Bank9SpriteScript*`; `src/TecmoSB/SpriteScript*`; `SpriteScriptPlayer.cs` | There is a real loader/interpreter path, so this is beyond documentation, but parity depth is still unclear. |
| `Bank10_sprite_scripts.asm` | sprite animation scripts | `partial` | `content/spritescripts/banks/bank10/index.yaml`; `src/TecmoSB/SpriteScript*`; `SpriteScriptPlayer.cs` | Same general state as bank 9: meaningful content/runtime path exists, but broad animation parity is not yet established. |
| `Bank11_12_BG_metatile_tiles.asm` | background metatiles/tiles | `scaffolded` | `content/bgmetatiles/bank11_12.yaml`; `src/TecmoSB/BgMetatile*`; field/menu renderers | Data conversion exists, but MonoGame intentionally replaces the raw NES delivery mechanism, so current coverage is mostly structural/reference-level. |
| `Bank12_13_sim_update_stats.asm` | sim bookkeeping, stats, clock, game/season logic | `partial` | `StatsSystem.cs`; `GameClockSystem.cs`; `DownDistanceSystem.cs`; `State/StatsState.cs`; season persistence/services under `src/TecmoSBGame/Persistence/` | Important rule/state/stat paths are implemented and tested, but this bank carries a lot of season/meta bookkeeping that the repo only partially covers today. |
| `Bank14_pal_fall_player_anim.asm` | palettes + falling/player animation data | `partial` | `content/palettes/bank14.yaml`; `content/animations/roll_animation.yaml`; `src/TecmoSB/Palette*`; `src/TecmoSB/RollAnimation*`; `AnimationSystem.cs` | Real asset/data conversion exists and the runtime has animation handling, but parity confidence is still moderate. |
| `Bank15_faces_playbooks.asm` | face data + playbook presentation | `partial` | `content/faces/faces.yaml`; `src/TecmoSB/Face*`; `content/playcall/playlist.yaml`; `src/TecmoSB/PlayList*`; playcall renderers | Face/playbook data is present and parts of playbook presentation are used, but this bank’s broader presentation responsibility is not fully realized. |
| `Bank16_menu_screens_slidebar.asm` | menus and front-end flows | `partial` | `content/menuscripts/banks/bank16/index.yaml`; `content/menuscripts/main_menu.yaml`; `MainMenuRenderer.cs`; `TitleScreenRenderer.cs`; `TeamSelectRenderer.cs`; `CoinTossRenderer.cs`; `SimArch/Flow/GameFlowController.cs` | The launch-to-exhibition path is now real, but many original front-end/menu semantics still look incomplete. |
| `Bank17_18_main_game_loop.asm` | main game state machine / orchestration | `substantial` | `content/gameloop/bank17_18_main_game_loop.yaml`; `src/TecmoSB/GameLoop*`; `SimArch/Flow/GameFlowController.cs`; `PlayLifecycleSystem.cs`; `Program.cs` headless harnesses | The repo now has a serious authoritative game-flow/match-state path with validation scenarios, which makes this one of the strongest converted areas. |
| `Bank19_20_on_field_gameplay_loop.asm` | on-field play loop | `substantial` | `content/onfieldloop/bank19_20_on_field_gameplay_loop.yaml`; `src/TecmoSB/OnFieldLoop*`; `src/TecmoSBGame/SimArch/Sim.cs`; headless scrimmage/kickoff/punt/FG scenarios | This is one of the clearest substantial banks: there is a functioning on-field simulation loop with deterministic validation coverage. |
| `Bank20_playcall.asm` | playcall UI/flow + CPU play selection | `substantial` | `content/playcall/bank20_playcall.yaml`; `src/TecmoSB/Playcall*`; `SimArch/Systems/PlayCall/PlayCallSystem.cs`; `PlayCallPublishSelectionSystem.cs`; playcall renderers/overlay | The bank’s core gameplay-facing responsibility is live in runtime, though final UI polish and full CPU parity remain open. |
| `Bank21_22_play_commands_on_field_logic.asm` | play commands + on-field behavior logic | `substantial` | `content/playcommands/bank21_22_play_commands_on_field_logic.yaml`; `src/TecmoSB/PlayCommand*`; `PlayExecutionSystem.cs`; route/blocking/coverage/pass/tackle systems | This is the heart of the football behavior port and it is meaningfully implemented, though still not equivalent to a full parity claim. |
| `Bank23_draw_field_ball_ani_coll_check.asm` | field draw logic, ball animation, collision checks | `partial` | `content/field/bank23_field_ball_anim_collision.yaml`; `src/TecmoSB/Field*`; `FieldRenderer.cs`; `CollisionDetectionSystem.cs`; `BallSystem.cs`; `TackleResolutionSystem.cs` | Collision/ball semantics are real and important, but field rendering and animation are still a mix of parity-minded and modern replacement paths. |
| `Bank24_draw_script_engine.asm` | scripted drawing/presentation engine | `scaffolded` | `content/drawscripts/bank24_draw_script_engine.yaml`; `src/TecmoSB/DrawScript*`; `DrawScriptRunner.cs` | Clear conversion scaffolding exists, but there is not enough evidence of broad presentation-engine parity to rate this higher. |
| `Bank25_leaders_player_data_pro_bowl_abbrev.asm` | leaders/player-data views/Pro Bowl abbrevs | `partial` | `content/leaders/bank25_leaders_player_data_pro_bowl_abbrev.yaml`; `src/TecmoSB/Leaders*`; `SeasonPresentationService.cs`; `SeasonMetaRenderer.cs` | Meta presentation exists and is no longer just a note, but the bank’s full leaders/records/Pro Bowl behavior still looks incomplete. |
| `Bank26_misc.asm` | schedule drawing, playoffs, meta/runtime support | `partial` | `content/misc/bank26_misc.yaml`; `src/TecmoSB/MiscBank*`; season models/managers/presentation under `Persistence/` | The season/meta layer is real enough to exceed scaffolding, but still far from a faithful full conversion of this bank’s broad responsibilities. |
| `Bank27_misc.asm` | misc season/preseason/meta support | `partial` | `content/misc/bank27_misc.yaml`; `src/TecmoSB/MiscBank27*`; season meta flow and save/resume support | Same general assessment as bank 26: meaningful work exists, but not enough to call substantial parity. |
| `Bank28_sound_engine.asm` | sound engine | `partial` | `content/sound/bank28_sound_engine.yaml`; `src/TecmoSB/SoundEngine*`; `src/TecmoSBGame/Audio/*`; `SimArchAudioBridge.cs`; `SoundSystem.cs` | Sound hookup and data representation exist, but this is still a lightweight modern bridge rather than a deeply validated equivalent of the original sound engine. |
| `Bank29_sound_data.asm` | music/SFX data | `scaffolded` | `content/sounddata/bank29_sound_data.yaml`; `src/TecmoSB/SoundData*` | Bank data is extracted and modeled, but the repo does not yet show strong parity use of this data in runtime. |
| `Bank30_sound_data.asm` | music/SFX data | `scaffolded` | `content/sounddata/bank30_sound_data.yaml`; `src/TecmoSB/SoundData*` | Same state as bank 29. |
| `Bank31_fixed_bank.asm` | fixed-bank support code / shared runtime glue | `partial` | `content/fixedbank/bank31_fixed_bank.yaml`; `src/TecmoSB/FixedBank*`; `GameLoopMachine.cs`; `OnFieldLoopMachine.cs`; shared match/play state systems | Shared runtime glue clearly exists, but this bank is diffuse enough that parity coverage is still partial. |
| `Bank32_DMC_Samples_reset_vector.asm` | DMC samples, reset/vector data | `documented` | `content/dmcsamples/bank32_dmc_samples.yaml`; `src/TecmoSB/DmcSamples*` | The content is recognized and extracted, but MonoGame does not need literal reset/vector behavior and current runtime use is minimal. |

## Summary by status

### Substantial
- `Bank1_2_team_data.asm`
- `Bank3_formation_metatile_data.asm`
- `Bank5_6_off_def_play_data.asm`
- `Bank17_18_main_game_loop.asm`
- `Bank19_20_on_field_gameplay_loop.asm`
- `Bank20_playcall.asm`
- `Bank21_22_play_commands_on_field_logic.asm`

### Partial
- `Bank4_def_spec_play_pointers_data.asm`
- `Bank9_sprite_scripts.asm`
- `Bank10_sprite_scripts.asm`
- `Bank12_13_sim_update_stats.asm`
- `Bank14_pal_fall_player_anim.asm`
- `Bank15_faces_playbooks.asm`
- `Bank16_menu_screens_slidebar.asm`
- `Bank23_draw_field_ball_ani_coll_check.asm`
- `Bank25_leaders_player_data_pro_bowl_abbrev.asm`
- `Bank26_misc.asm`
- `Bank27_misc.asm`
- `Bank28_sound_engine.asm`
- `Bank31_fixed_bank.asm`

### Scaffolded
- `Bank7_scene_scripts.asm`
- `Bank11_12_BG_metatile_tiles.asm`
- `Bank24_draw_script_engine.asm`
- `Bank29_sound_data.asm`
- `Bank30_sound_data.asm`

### Documented
- `bank8_scene_scripts.asm`
- `Bank32_DMC_Samples_reset_vector.asm`

### Missing
- None at the major-bank level. Every major bank in the current inventory has at least some corresponding repo footprint.

## Practical answer for future agents

If someone asks **"how much of the real conversion is already done?"** the grounded answer is:

- The repo has **broad bank coverage** at the data/scaffolding level.
- The repo has **substantial progress on the core football gameplay conversion**, especially the critical on-field and orchestration banks.
- The repo is **not yet close to full parity across the whole original game**, because presentation, scene/meta systems, audio parity, and parts of season/front-end behavior are still mostly partial or scaffolded.
- So the honest status is: **the core game conversion is well underway, but the full bank-by-bank conversion is still only partially complete overall.**

## Validation notes

This matrix was cross-checked against:

1. the disassembly bank list present in `/home/montblanc/repos/football-game/reference/Tecmo_Super_Bowl_NES_Disassembly`
2. the project baseline in `conversion-inventory.md`
3. the current MonoGame repo structure under `content/`, `src/TecmoSB/`, and `src/TecmoSBGame/`
4. the currently advertised executable/runtime surface in `src/TecmoSBGame/Program.cs`

Internal consistency check used for this note:
- every major bank from the conversion inventory appears in the matrix
- every matrix entry cites at least one concrete repo area claiming to represent that bank
- no bank was marked `missing` if corresponding content/loader/runtime files were present
- higher statuses (`partial` / `substantial`) were reserved for banks with visible runtime consumption or validated scenario coverage, not just extracted YAML files
