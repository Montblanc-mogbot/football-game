# Tecmo Super Bowl assembly bank overview

Updated: 2026-05-26

## Purpose

This note is a clean assembly-first overview of the major Tecmo Super Bowl disassembly banks.

It intentionally does **not** evaluate prior MonoGame implementation progress.
The goal is to understand the original program structure well enough to plan a fresh conversion.

## Reading rule

For each bank, ask:
1. what original responsibility it owns
2. whether that responsibility is mostly behavioral logic, semantic/content meaning, or NES-specific delivery plumbing
3. whether the bank should be attacked early in a fresh MonoGame conversion

## Bank overview

| Bank | Primary responsibility | Conversion type | Fresh-start priority |
| --- | --- | --- | --- |
| `Bank1_2_team_data.asm` | teams, players, ratings, names, attributes | data semantics | high |
| `Bank3_formation_metatile_data.asm` | formations plus some field/metatile support data | mixed (formations matter, raw tile transport less so) | high |
| `Bank4_def_spec_play_pointers_data.asm` | defensive and special-teams play pointer tables | behavioral/data semantics | high |
| `Bank5_6_off_def_play_data.asm` | offensive and defensive player play scripts | behavioral | critical |
| `Bank7_scene_scripts.asm` | cutscene/static scene scripts | semantic/content | medium |
| `bank8_scene_scripts.asm` | season/static screen scripts | semantic/content | medium |
| `Bank9_sprite_scripts.asm` | sprite animation scripts | semantic/content | medium |
| `Bank10_sprite_scripts.asm` | sprite animation scripts | semantic/content | medium |
| `Bank11_12_BG_metatile_tiles.asm` | background metatiles/tiles | mostly NES delivery/content | low-medium |
| `Bank12_13_sim_update_stats.asm` | sim bookkeeping, stats, clock, game/season logic | behavioral | critical |
| `Bank14_pal_fall_player_anim.asm` | palettes, falling/player animation data | mixed, mostly content/delivery | low-medium |
| `Bank15_faces_playbooks.asm` | face data, playbook presentation | semantic/content | medium |
| `Bank16_menu_screens_slidebar.asm` | menus, front-end flows, slidebar screens | mixed | high |
| `Bank17_18_main_game_loop.asm` | main game state machine and orchestration | behavioral | critical |
| `Bank19_20_on_field_gameplay_loop.asm` | on-field gameplay loop | behavioral | critical |
| `Bank20_playcall.asm` | playcall UI/flow and CPU play selection | behavioral/mixed | critical |
| `Bank21_22_play_commands_on_field_logic.asm` | play commands and on-field behavior logic | behavioral | critical |
| `Bank23_draw_field_ball_ani_coll_check.asm` | field drawing, ball animation, collision checks | mixed | high |
| `Bank24_draw_script_engine.asm` | scripted drawing/presentation engine | semantic/content | medium |
| `Bank25_leaders_player_data_pro_bowl_abbrev.asm` | leaders, player-data views, Pro Bowl abbreviations | mixed | high |
| `Bank26_misc.asm` | schedules, playoffs, meta/runtime support | behavioral/mixed | high |
| `Bank27_misc.asm` | misc season/preseason/meta support | behavioral/mixed | high |
| `Bank28_sound_engine.asm` | sound engine | mostly delivery with some semantic cue mapping | medium |
| `Bank29_sound_data.asm` | music/SFX data | content | low-medium |
| `Bank30_sound_data.asm` | music/SFX data | content | low-medium |
| `Bank31_fixed_bank.asm` | fixed-bank support/shared runtime glue | mixed | medium |
| `Bank32_DMC_Samples_reset_vector.asm` | DMC samples, reset/vector data | mostly platform-specific delivery | low |

## First practical grouping for a fresh conversion

### Core behavioral conversion first
- `Bank5_6_off_def_play_data.asm`
- `Bank12_13_sim_update_stats.asm`
- `Bank17_18_main_game_loop.asm`
- `Bank19_20_on_field_gameplay_loop.asm`
- `Bank20_playcall.asm`
- `Bank21_22_play_commands_on_field_logic.asm`

### Supporting gameplay/meta banks next
- `Bank1_2_team_data.asm`
- `Bank3_formation_metatile_data.asm`
- `Bank4_def_spec_play_pointers_data.asm`
- `Bank16_menu_screens_slidebar.asm`
- `Bank23_draw_field_ball_ani_coll_check.asm`
- `Bank25_leaders_player_data_pro_bowl_abbrev.asm`
- `Bank26_misc.asm`
- `Bank27_misc.asm`

### Semantic/content banks after core behavior is understood
- `Bank7_scene_scripts.asm`
- `bank8_scene_scripts.asm`
- `Bank9_sprite_scripts.asm`
- `Bank10_sprite_scripts.asm`
- `Bank14_pal_fall_player_anim.asm`
- `Bank15_faces_playbooks.asm`
- `Bank24_draw_script_engine.asm`
- `Bank28_sound_engine.asm`
- `Bank29_sound_data.asm`
- `Bank30_sound_data.asm`
- `Bank31_fixed_bank.asm`
- `Bank32_DMC_Samples_reset_vector.asm`

## Planning rule

Do not ask "what old code already exists?" first.
Ask "what did the original bank do, and does MonoGame require faithful reimplementation of that responsibility or only a modern replacement mechanism?"
