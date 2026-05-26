# Bank1_2 loader layer

Updated: 2026-05-26

## Purpose

This note describes the bridge between the generated Bank1_2 source-faithful artifacts and the decoded C# semantic models.

## Loader entry point

- `src/FootballGame/Conversion/Bank12/Bank12JsonLoader.cs`
- returns `Bank12DataSet`

## Input artifacts

The loader consumes:
- `content/reference/bank12/generated/team-identities.json`
- `content/reference/bank12/generated/team-abilities.json`

## Output semantic layer

The loader maps generated JSON into:
- `TeamRosterRecord`
- `PlayerIdentityRecord`
- `TeamAbilitySet`
- `QuarterbackAbilityRecord`
- `SkillPositionAbilityRecord`
- `OffensiveLineAbilityRecord`
- `DefenderAbilityRecord`
- `KickerPunterAbilityRecord`

## Key rule

The loader removes pointer mechanics, but it does **not** remove bank semantics.
It preserves:
- canonical team order
- canonical roster-slot order
- exact source labels
- exact source-name payloads
- role-specific ability schemas
- source-faithful attribute grade meaning

## Non-goals

This loader is not yet:
- a runtime gameplay service
- a write-back path to assembly
- a generalized bank parser framework

It is specifically the Bank1_2 semantic bridge needed by later bank work.
