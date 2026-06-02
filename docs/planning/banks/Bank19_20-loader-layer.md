# Bank19_20 loader layer

Updated: 2026-06-02

## Purpose

This note describes the bridge between the generated Bank19_20 source-faithful artifact and the typed C# semantic model.

## Loader entry point

- `src/FootballGame/Conversion/OnField/Bank19OnFieldGameplayInventoryJsonLoader.cs`
- returns `Bank19OnFieldGameplayInventory`

## Input artifact

The loader consumes:
- `content/game-data/on-field/generated/bank19_20-section-map.json`

## Output semantic layer

The loader maps generated JSON into:
- `Bank19OnFieldGameplayInventory`
- `Bank19EntryPointRecord`
- `Bank19ScriptPointerFamilyRecord`
- `Bank19ExternalJumpConstantRecord`
- `Bank19CrossBankDependencyRecord`
- `Bank19SectionRecord`
- `Bank19SectionLabelRecord`
- `Bank19ModernOwner`
- `Bank19ResponsibilityGroup`

## Key rule

The loader removes raw JSON/DTO mechanics, but it does **not** remove bank semantics.
It preserves:
- explicit bank entrypoints
- special script-pointer family ordering
- explicit external jump constants
- explicit cross-bank dependency declarations
- source section ordering and line spans
- nested section depth/parent relationships
- section label lists with source lines
- controller vs supporting-service ownership
- explicit Bank21_22 carry-forward tagging

## Non-goals

This loader is not yet:
- the final MonoGame gameplay runtime
- a coordinator implementation
- a script interpreter
- a generalized bank-loader framework

It is specifically the Bank19_20 semantic bridge that keeps the extracted host-bank inventory usable for later runtime implementation.
