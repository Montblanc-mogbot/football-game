# First bounded conversion packet

Updated: 2026-05-26

## Selected packet
- **Packet id:** `5A`
- **Source bank:** `Bank5_6_off_def_play_data.asm`
- **Packet name:** offensive script model extraction

## Why this goes first

`5A` is the first packet that turns the assembly-first planning work into a concrete conversion artifact without requiring the whole runtime first.
It is small enough to stay bounded, but important enough to anchor later work on command vocabulary (`5C`) and on-field command semantics (`21A`).

## Expected output/artifact

Produce a source-grounded packet note plus a first modern representation for one offensive play family:

1. **Packet analysis note** under `docs/planning/packets/5A-offensive-script-model.md`
   - names the exact offensive play family inspected in `Bank5_6_off_def_play_data.asm`
   - documents the relevant source labels/data shape
   - explains the original command sequence semantics in plain language
   - states what is still unknown or deferred
2. **Initial modern data artifact** in the MonoGame codebase
   - one small, explicit data model that can represent that play family's command structure
   - no broad runtime/execution engine yet
3. **Validation note**
   - explains how the modern representation was checked against the source-bank structure

## Old-code reuse decision

- **Old code may be consulted?** No, not for this packet.
- This packet should be built directly from the disassembly and current planning docs.
- If later work wants to compare against an older MonoGame repo, that must be a separate explicit task.

## Follow-on dependency

If `5A` lands cleanly, the immediate next packet should be `5C` so the first extracted offensive script representation and the shared command vocabulary evolve together.
