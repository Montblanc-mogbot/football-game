using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace FootballGame.Conversion.OnField;

/// <summary>
/// Loads the generated Bank19_20 inventory JSON into the typed conversion-side semantic model.
/// </summary>
public static class Bank19OnFieldGameplayInventoryJsonLoader
{
    public static Bank19OnFieldGameplayInventory LoadFromGeneratedFile(string generatedFilePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(generatedFilePath);

        InventoryRoot root = DeserializeFile<InventoryRoot>(generatedFilePath);

        return new Bank19OnFieldGameplayInventory
        {
            EntryPoints = root.EntryPoints
                .Select(entry => new Bank19EntryPointRecord
                {
                    SourceLabel = entry.SourceLabel,
                    TargetLabel = entry.TargetLabel,
                    Line = entry.Line,
                    Notes = entry.Notes,
                })
                .ToArray(),
            ScriptPointerFamilies = root.ScriptPointerFamilies
                .Select(family => new Bank19ScriptPointerFamilyRecord
                {
                    SourceLabel = family.SourceLabel,
                    Address = family.Address,
                    TeamSide = family.TeamSide,
                    Purpose = family.Purpose,
                    Line = family.Line,
                })
                .ToArray(),
            ExternalJumpConstants = root.ExternalJumpConstants
                .Select(constant => new Bank19ExternalJumpConstantRecord
                {
                    Symbol = constant.Symbol,
                    Value = constant.Value,
                    Line = constant.Line,
                })
                .ToArray(),
            ExternalDependencies = root.ExternalDependencies
                .Select(dependency => new Bank19CrossBankDependencyRecord
                {
                    Symbol = dependency.Symbol,
                    SourceBank = dependency.SourceBank,
                    DependencyKind = dependency.DependencyKind,
                    Notes = dependency.Notes,
                })
                .ToArray(),
            Sections = root.Sections
                .Select(section => new Bank19SectionRecord
                {
                    SectionName = section.SectionName,
                    SourceStartLine = section.SourceStartLine,
                    SourceEndLine = section.SourceEndLine,
                    SourceStartMarker = section.SourceStartMarker,
                    SourceEndMarker = section.SourceEndMarker,
                    LineCount = section.LineCount,
                    Depth = section.Depth,
                    ParentSectionName = section.ParentSectionName,
                    ModernOwner = ParseModernOwner(section.ModernOwner),
                    ResponsibilityGroup = ParseResponsibilityGroup(section.ResponsibilityGroup),
                    PrimaryEntryLabels = section.PrimaryEntryLabels.ToArray(),
                    Labels = section.Labels
                        .Select(label => new Bank19SectionLabelRecord
                        {
                            Label = label.Label,
                            Line = label.Line,
                        })
                        .ToArray(),
                    ExternalDependencySymbols = section.ExternalDependencySymbols.ToArray(),
                    Notes = section.Notes,
                    CarryForwardToBank21_22 = section.CarryForwardToBank21_22,
                    Bank21_22CarryForwardReason = section.Bank21_22CarryForwardReason,
                    Bank21_22BridgeSymbols = section.Bank21_22BridgeSymbols.ToArray(),
                })
                .ToArray(),
        };
    }

    private static T DeserializeFile<T>(string path)
    {
        string json = File.ReadAllText(path);
        T? value = JsonSerializer.Deserialize<T>(json, JsonOptions);
        return value ?? throw new InvalidOperationException($"Failed to deserialize {path} into {typeof(T).Name}.");
    }

    private static Bank19ModernOwner ParseModernOwner(string value)
    {
        return value switch
        {
            "controller" => Bank19ModernOwner.Controller,
            "supporting-service" => Bank19ModernOwner.SupportingService,
            _ => throw new InvalidOperationException($"Unsupported Bank19 modern owner '{value}'."),
        };
    }

    private static Bank19ResponsibilityGroup ParseResponsibilityGroup(string value)
    {
        return value switch
        {
            "play-phase-routing" => Bank19ResponsibilityGroup.PlayPhaseRouting,
            "play-outcome" => Bank19ResponsibilityGroup.PlayOutcome,
            "task-coordination" => Bank19ResponsibilityGroup.TaskCoordination,
            "pre-snap-control" => Bank19ResponsibilityGroup.PreSnapControl,
            "play-assignment" => Bank19ResponsibilityGroup.PlayAssignment,
            "roster-skill-hydration" => Bank19ResponsibilityGroup.RosterSkillHydration,
            "presentation" => Bank19ResponsibilityGroup.Presentation,
            "cpu-decision-support" => Bank19ResponsibilityGroup.CpuDecisionSupport,
            "pass-targeting" => Bank19ResponsibilityGroup.PassTargeting,
            "stats-and-distance" => Bank19ResponsibilityGroup.StatsAndDistance,
            "injury-and-cutscene" => Bank19ResponsibilityGroup.InjuryAndCutscene,
            _ => throw new InvalidOperationException($"Unsupported Bank19 responsibility group '{value}'."),
        };
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    private sealed record InventoryRoot
    {
        public required List<EntryPointDto> EntryPoints { get; init; }

        public required List<ScriptPointerFamilyDto> ScriptPointerFamilies { get; init; }

        public required List<ExternalJumpConstantDto> ExternalJumpConstants { get; init; }

        public required List<ExternalDependencyDto> ExternalDependencies { get; init; }

        public required List<SectionDto> Sections { get; init; }
    }

    private sealed record EntryPointDto
    {
        public required string SourceLabel { get; init; }

        public required string TargetLabel { get; init; }

        public required int Line { get; init; }

        public required string Notes { get; init; }
    }

    private sealed record ScriptPointerFamilyDto
    {
        public required string SourceLabel { get; init; }

        public required string Address { get; init; }

        public required string TeamSide { get; init; }

        public required string Purpose { get; init; }

        public required int Line { get; init; }
    }

    private sealed record ExternalJumpConstantDto
    {
        public required string Symbol { get; init; }

        public required string Value { get; init; }

        public required int Line { get; init; }
    }

    private sealed record ExternalDependencyDto
    {
        public required string Symbol { get; init; }

        public required string SourceBank { get; init; }

        public required string DependencyKind { get; init; }

        public required string Notes { get; init; }
    }

    private sealed record SectionDto
    {
        public required string SectionName { get; init; }

        public required int SourceStartLine { get; init; }

        public required string SourceStartMarker { get; init; }

        public required string ModernOwner { get; init; }

        public required string ResponsibilityGroup { get; init; }

        public required string Notes { get; init; }

        public required bool CarryForwardToBank21_22 { get; init; }

        public string? Bank21_22CarryForwardReason { get; init; }

        public required List<string> Bank21_22BridgeSymbols { get; init; }

        public int? Depth { get; init; }

        public string? ParentSectionName { get; init; }

        public required int SourceEndLine { get; init; }

        public required string SourceEndMarker { get; init; }

        public required int LineCount { get; init; }

        public required int GlobalLabelCount { get; init; }

        public required List<LabelDto> Labels { get; init; }

        public required List<string> PrimaryEntryLabels { get; init; }

        public required List<string> ExternalDependencySymbols { get; init; }
    }

    private sealed record LabelDto
    {
        public required string Label { get; init; }

        public required int Line { get; init; }
    }
}
