using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace FootballGame.Conversion.PlayCommands;

/// <summary>
/// Loads the generated Bank21_22 command-runtime inventory JSON into typed conversion models.
/// </summary>
public static class Bank21_22PlayCommandInventoryJsonLoader
{
    public static Bank21_22PlayCommandInventory LoadFromGeneratedFiles(string sectionMapPath, string summaryPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(sectionMapPath);
        ArgumentException.ThrowIfNullOrEmpty(summaryPath);

        SectionMapRoot sectionMap = DeserializeFile<SectionMapRoot>(sectionMapPath);
        SummaryRoot summary = DeserializeFile<SummaryRoot>(summaryPath);

        return new Bank21_22PlayCommandInventory
        {
            Sections = sectionMap.Sections
                .Select(section => new Bank21_22SectionRecord
                {
                    SectionName = section.SectionName,
                    SourceStartLine = section.SourceStartLine,
                    SourceEndLine = section.SourceEndLine,
                    SourceStartMarker = section.SourceStartMarker,
                    SourceEndMarker = section.SourceEndMarker,
                    LineCount = section.LineCount,
                    Category = section.Category,
                    Notes = section.Notes,
                    PrimaryEntryLabels = section.PrimaryEntryLabels.ToArray(),
                    Labels = section.Labels
                        .Select(label => new Bank21_22LabelRecord
                        {
                            Label = label.Label,
                            Line = label.Line,
                        })
                        .ToArray(),
                })
                .ToArray(),
            Constants = summary.Constants
                .Select(constant => new Bank21_22ConstantRecord
                {
                    Name = constant.Name,
                    Value = constant.Value,
                    Comment = constant.Comment,
                    Line = constant.Line,
                })
                .ToArray(),
            CommandDispatcher = new Bank21_22CommandDispatcherRecord
            {
                SectionName = summary.CommandDispatcher.SectionName,
                SourceStartLine = summary.CommandDispatcher.SourceStartLine,
                GroupCommandCount = summary.CommandDispatcher.GroupCommandCount,
                SingleCommandCount = summary.CommandDispatcher.SingleCommandCount,
                GroupDispatchTargets = summary.CommandDispatcher.GroupDispatchTargets.ToArray(),
                SingleDispatchTargetsSample = summary.CommandDispatcher.SingleDispatchTargetsSample.ToArray(),
                BridgeJumpExports = summary.BridgeJumpExports.ToArray(),
            },
        };
    }

    private static T DeserializeFile<T>(string path)
    {
        string json = File.ReadAllText(path);
        T? value = JsonSerializer.Deserialize<T>(json, JsonOptions);
        return value ?? throw new InvalidOperationException($"Failed to deserialize {path} into {typeof(T).Name}.");
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    private sealed record SectionMapRoot
    {
        public required List<SectionDto> Sections { get; init; }
    }

    private sealed record SectionDto
    {
        public required string SectionName { get; init; }

        public required int SourceStartLine { get; init; }

        public required int SourceEndLine { get; init; }

        public required string SourceStartMarker { get; init; }

        public required string SourceEndMarker { get; init; }

        public required int LineCount { get; init; }

        public required List<string> PrimaryEntryLabels { get; init; }

        public required List<LabelDto> Labels { get; init; }

        public required string Category { get; init; }

        public required string Notes { get; init; }
    }

    private sealed record LabelDto
    {
        public required string Label { get; init; }

        public required int Line { get; init; }
    }

    private sealed record SummaryRoot
    {
        public required List<ConstantDto> Constants { get; init; }

        public required CommandDispatcherDto CommandDispatcher { get; init; }

        public required List<string> BridgeJumpExports { get; init; }
    }

    private sealed record ConstantDto
    {
        public required string Name { get; init; }

        public required string Value { get; init; }

        public required string Comment { get; init; }

        public required int Line { get; init; }
    }

    private sealed record CommandDispatcherDto
    {
        public required string SectionName { get; init; }

        public required int SourceStartLine { get; init; }

        public required int GroupCommandCount { get; init; }

        public required int SingleCommandCount { get; init; }

        public required List<string> GroupDispatchTargets { get; init; }

        public required List<string> SingleDispatchTargetsSample { get; init; }
    }
}
