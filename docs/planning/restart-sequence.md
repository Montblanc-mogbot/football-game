# Fresh-start restart sequence

Updated: 2026-05-26

This is the ordered restart path for the clean MonoGame conversion.
It assumes **no old-code reuse by default** and treats the disassembly as the source of truth.

## Phase 1 — establish the smallest gameplay-parity backbone

1. **Bank1_2 team/player data semantics**
   - Lock down how teams, players, ratings, and names are represented so later packets share one faithful data base.
2. **Packet 5A (`Bank5_6_off_def_play_data.asm`) — offensive script model extraction**
   - Prove one offensive play family can be expressed from source-bank structure.
3. **Packet 5C (`Bank5_6_off_def_play_data.asm`) — shared play-command vocabulary**
   - Define the smallest command language needed by the extracted play scripts.
4. **Packet 21A (`Bank21_22_play_commands_on_field_logic.asm`) — offensive command semantics slice**
   - Tie the command vocabulary to actual on-field meaning instead of placeholder runtime guesses.
5. **Packet 17A (`Bank17_18_main_game_loop.asm`) — front-end state sequence slice**
   - Map the real source-game entry flow before inventing a new game-state structure.
6. **Packet 19A (`Bank19_20_on_field_gameplay_loop.asm`) — snap-to-live-ball phase slice**
   - Establish the first authoritative on-field lifecycle transition.
7. **Packet 12A (`Bank12_13_sim_update_stats.asm`) — clock and quarter bookkeeping slice**
   - Add the smallest real timing/state bookkeeping needed to support play flow.

## Phase 2 — make the first playable parity loop coherent

8. **Bank3 formations + Bank4 defense/special-teams pointer semantics**
   - Bring in the formation and pointer data the play packets depend on.
9. **Packet 20A (`Bank20_playcall.asm`) — offense playcall flow slice**
   - Recreate the original offense-facing selection semantics.
10. **Packet 19B (`Bank19_20_on_field_gameplay_loop.asm`) — whistle/dead-ball phase slice**
    - Complete one end-of-play transition.
11. **Packet 17B (`Bank17_18_main_game_loop.asm`) — post-play orchestration slice**
    - Connect dead-ball outcomes back into the next-play loop.
12. **Packet 12B (`Bank12_13_sim_update_stats.asm`) — per-play stat bookkeeping slice**
    - Add the first bounded stats/state outputs needed after a play resolves.
13. **Bank23 collision/ball-semantics slice**
    - Recreate the collision and ball-behavior responsibilities that make the loop credible.

## Phase 3 — widen support around the core loop

14. **Packet 5B / 21B** — defensive script model + defensive reaction semantics.
15. **Packet 20B / 20C** — CPU/defense and special-teams playcall slices.
16. **Packet 12C, 17C, 19C, 21C** — bookkeeping, interruption, control-handoff, and edge-case packets.
17. **Banks 16, 25, 26, 27** — menus, leaders, schedules, playoffs, and broader meta-state once the core loop is grounded.

## Rule for future implementation tasks

Start from the topmost unfinished item in this sequence unless a new task explicitly justifies a different packet.
Every implementation task should name the source bank or packet id it advances.
