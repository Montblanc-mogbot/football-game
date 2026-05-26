# Tecmo Super Bowl — conversion inventory reset

Updated: 2026-05-26

## Purpose

This note recenters the project on the original goal:

**faithfully convert the original Tecmo Super Bowl NES program into MonoGame**

This is **not** a generic Tecmo-inspired football game plan.
The disassembly repo at `/home/montblanc/repos/football-game/reference/Tecmo_Super_Bowl_NES_Disassembly` is the behavioral source of truth.

## Key conversion principle

Do **not** treat every assembly bank as something that must be ported line-by-line.

Instead, classify each original responsibility as one of:

1. **MonoGame-trivialized**
   - NES hardware plumbing that can be replaced outright with normal MonoGame systems.
2. **Semantic-preservation**
   - original content/flow/meaning matters, but implementation can be modern.
3. **Behavioral reimplementation**
   - football logic, rules, AI, state transitions, and data semantics that must be re-created faithfully.

## What MonoGame mostly trivializes

These should generally **not** be literal port targets:

- PPU nametable writes, PPU transfer buffers, palette upload routines
- OAM DMA and low-level sprite memory plumbing
- MMC3 bank-switching and IRQ split-screen setup
- direct joypad register polling
- raw APU/DMC register programming
- SRAM copy/load/save macros as low-level memory mechanics
- tile/sprite transport mechanics used only to get pixels on screen

MonoGame replacements:

- SpriteBatch / atlases / layered render passes
- ordinary menu/HUD drawing code
- normal input mapping
- standard save serialization
- conventional audio event playback

## What still requires real faithful work

These are the high-value parity targets:

- on-field football simulation
- playbook/play-command behavior
- CPU decision rules and timing
- scoring / possession / down-distance / clock rules
- kickoff / punt / FG / XP / turnover transitions
- season / standings / playoffs / Pro Bowl / leaders logic
- data extraction for teams, formations, ratings, scripts, and tables
- screen/menu/game-flow behavior where it affects actual rules or progression

## Bank-by-bank inventory

| Bank / file | Primary responsibility | MonoGame trivialized? | Needs semantic preservation? | Needs behavioral reimplementation? | Priority |
| --- | --- | --- | --- | --- | --- |
| `Bank1_2_team_data.asm` | team/player data, attributes, names | no | yes | yes (data semantics) | high |
| `Bank3_formation_metatile_data.asm` | formations, some BG/metatile data | partial | yes | yes for formations, no for raw tile transport | high |
| `Bank4_def_spec_play_pointers_data.asm` | defense/special-teams play pointers | no | yes | yes | high |
| `Bank5_6_off_def_play_data.asm` | offensive/defensive player play scripts | no | yes | yes | critical |
| `Bank7_scene_scripts.asm` | cutscene/static scene scripts | partial | yes | partial | medium |
| `bank8_scene_scripts.asm` | season/static screen scripts | partial | yes | partial | medium |
| `Bank9_sprite_scripts.asm` | sprite animation scripts | partial | yes | partial | medium |
| `Bank10_sprite_scripts.asm` | sprite animation scripts | partial | yes | partial | medium |
| `Bank11_12_BG_metatile_tiles.asm` | background metatiles/tiles | mostly | yes | low | low-medium |
| `Bank12_13_sim_update_stats.asm` | sim bookkeeping, stats, clock, game/season logic | no | yes | yes | critical |
| `Bank14_pal_fall_player_anim.asm` | palettes, falling/player animation data | mostly | yes | partial | low-medium |
| `Bank15_faces_playbooks.asm` | face data, playbook presentation | mostly | yes | low | medium |
| `Bank16_menu_screens_slidebar.asm` | menus, slidebar screens, front-end flows | partial | yes | partial | high |
| `Bank17_18_main_game_loop.asm` | main game state machine / orchestration | no | yes | yes | critical |
| `Bank19_20_on_field_gameplay_loop.asm` | on-field play loop | no | yes | yes | critical |
| `Bank20_playcall.asm` | playcall UI + flow + CPU play selection | partial | yes | yes | critical |
| `Bank21_22_play_commands_on_field_logic.asm` | on-field play commands, behavior logic | no | yes | yes | critical |
| `Bank23_draw_field_ball_ani_coll_check.asm` | field draw logic, ball animation, collision checks | partial | yes | yes for collision/ball semantics; no for draw plumbing | high |
| `Bank24_draw_script_engine.asm` | scripted drawing/presentation engine | mostly | yes | partial | medium |
| `Bank25_leaders_player_data_pro_bowl_abbrev.asm` | leaders, player data views, Pro Bowl abbrevs | partial | yes | yes for data/meta flows | high |
| `Bank26_misc.asm` | schedule drawing, playoffs/meta/runtime support | partial | yes | yes | high |
| `Bank27_misc.asm` | misc season/preseason/meta support | partial | yes | yes | high |
| `Bank28_sound_engine.asm` | sound engine | mostly | yes | partial | medium |
| `Bank29_sound_data.asm` | music/SFX data | mostly | yes | low | low-medium |
| `Bank30_sound_data.asm` | music/SFX data | mostly | yes | low | low-medium |
| `Bank31_fixed_bank.asm` | fixed-bank support code / shared runtime glue | partial | yes | partial | medium |
| `Bank32_DMC_Samples_reset_vector.asm` | DMC samples, reset/vector data | mostly | yes | low | low |

## Priority buckets

### Critical parity banks
These define the real conversion core:

- `Bank5_6_off_def_play_data.asm`
- `Bank12_13_sim_update_stats.asm`
- `Bank17_18_main_game_loop.asm`
- `Bank19_20_on_field_gameplay_loop.asm`
- `Bank20_playcall.asm`
- `Bank21_22_play_commands_on_field_logic.asm`

### High-priority supporting banks
These matter early because they feed the core loop or real meta-state:

- `Bank1_2_team_data.asm`
- `Bank3_formation_metatile_data.asm`
- `Bank4_def_spec_play_pointers_data.asm`
- `Bank16_menu_screens_slidebar.asm`
- `Bank23_draw_field_ball_ani_coll_check.asm`
- `Bank25_leaders_player_data_pro_bowl_abbrev.asm`
- `Bank26_misc.asm`
- `Bank27_misc.asm`

### Mostly semantic/content banks
These should be preserved, but can be rebuilt with modern systems:

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

## Immediate planning consequence

Future work should stop treating "more playable vertical slice" as the main north star.

Instead, work should answer one of these questions:

1. **What original bank responsibility does this implement?**
2. **Is that responsibility behavioral, semantic, or MonoGame-trivialized?**
3. **What evidence shows the MonoGame version matches the original behavior or data meaning?**

## Recommended first follow-up tasks

1. create a parity-status matrix comparing each important bank against the current MonoGame repo (`documented` / `scaffolded` / `partial` / `substantial` / `missing`)
2. extract the non-trivial behavior owned by the critical banks into smaller conversion packets
3. separate repo areas that are modern delivery/plumbing from repo areas that claim behavioral parity
4. define acceptance for each critical bank in parity terms, not vertical-slice terms

## Guidance for subagents

When assigned Tecmo work:

- treat `/home/montblanc/repos/football-game/reference/Tecmo_Super_Bowl_NES_Disassembly` as the original program source
- read this file before planning conversion work
- do not assume every NES hardware routine needs a literal C# equivalent
- do not optimize for novelty or cleaner design ahead of parity
- prefer tasks framed as "port/validate responsibility X from bank Y" over broad gameplay feature work
- if a task touches rendering/audio/input persistence, distinguish hardware-mechanism code from actual gameplay/state semantics first
