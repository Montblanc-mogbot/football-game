using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

using FootballGame.Conversion.Bank12.Models;

namespace FootballGame.Conversion.Bank12;

/// <summary>
/// Loads the generated Bank1_2 JSON artifacts into the decoded semantic models.
/// </summary>
public static class Bank12JsonLoader
{
    public static Bank12DataSet LoadFromGeneratedDirectory(string generatedDirectoryPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(generatedDirectoryPath);

        string identityPath = Path.Combine(generatedDirectoryPath, "team-identities.json");
        string abilityPath = Path.Combine(generatedDirectoryPath, "team-abilities.json");

        IdentityRoot identities = DeserializeFile<IdentityRoot>(identityPath);
        AbilityRoot abilities = DeserializeFile<AbilityRoot>(abilityPath);

        IReadOnlyList<TeamRosterRecord> teamRosters = identities.Teams
            .OrderBy(team => team.Order)
            .Select(MapTeamRoster)
            .ToArray();

        IReadOnlyList<TeamAbilitySet> teamAbilitySets = abilities.Teams
            .OrderBy(team => team.Order)
            .Select(MapTeamAbilitySet)
            .ToArray();

        return new Bank12DataSet
        {
            TeamRosters = teamRosters,
            TeamAbilities = teamAbilitySets,
        };
    }

    private static TeamRosterRecord MapTeamRoster(IdentityTeamDto team)
    {
        TeamId teamId = ParseTeamId(team.TeamListLabel);

        return new TeamRosterRecord
        {
            TeamId = teamId,
            TeamListLabel = team.TeamListLabel,
            PlayersInCanonicalSlotOrder = team.Players
                .Select(player => new PlayerIdentityRecord
                {
                    TeamId = teamId,
                    RosterSlot = ParseRosterSlot(player.Slot),
                    SourceLabel = player.SourceLabel,
                    JerseyNumber = checked((byte)player.JerseyNumber),
                    SourceNamePayload = player.SourceNamePayload,
                })
                .ToArray(),
        };
    }

    private static TeamAbilitySet MapTeamAbilitySet(AbilityTeamDto team)
    {
        TeamId teamId = ParseTeamId(team.TeamListLabel);
        Dictionary<RosterSlot, BaseAbilityRecord> abilities = new();

        foreach (AbilitySlotDto slot in team.Slots)
        {
            abilities[ParseRosterSlot(slot.Slot)] = MapAbilityRecord(slot);
        }

        return new TeamAbilitySet
        {
            TeamId = teamId,
            SourceLabel = team.AbilityLabel,
            AbilitiesBySlot = abilities,
        };
    }

    private static BaseAbilityRecord MapAbilityRecord(AbilitySlotDto slot)
    {
        return slot.Role switch
        {
            "QB" => new QuarterbackAbilityRecord
            {
                RushingPower = ParseAttributeGrade(slot.RushingPower),
                RunningSpeed = ParseAttributeGrade(slot.RunningSpeed),
                MaximumSpeed = ParseAttributeGrade(slot.MaximumSpeed),
                HittingPower = ParseAttributeGrade(slot.HittingPower),
                FaceIdentifier = checked((byte)slot.FaceIdentifier.Value),
                PassingSpeed = ParseAttributeGrade(slot.PassingSpeed!),
                PassControl = ParseAttributeGrade(slot.PassControl!),
                AccuracyOfPassing = ParseAttributeGrade(slot.AccuracyOfPassing!),
                AvoidPassBlock = ParseAttributeGrade(slot.AvoidPassBlock!),
            },
            "SKILL" => new SkillPositionAbilityRecord
            {
                RushingPower = ParseAttributeGrade(slot.RushingPower),
                RunningSpeed = ParseAttributeGrade(slot.RunningSpeed),
                MaximumSpeed = ParseAttributeGrade(slot.MaximumSpeed),
                HittingPower = ParseAttributeGrade(slot.HittingPower),
                FaceIdentifier = checked((byte)slot.FaceIdentifier.Value),
                BallControl = ParseAttributeGrade(slot.BallControl!),
                Receptions = ParseAttributeGrade(slot.Receptions!),
            },
            "OL" => new OffensiveLineAbilityRecord
            {
                RushingPower = ParseAttributeGrade(slot.RushingPower),
                RunningSpeed = ParseAttributeGrade(slot.RunningSpeed),
                MaximumSpeed = ParseAttributeGrade(slot.MaximumSpeed),
                HittingPower = ParseAttributeGrade(slot.HittingPower),
                FaceIdentifier = checked((byte)slot.FaceIdentifier.Value),
            },
            "DEF" => new DefenderAbilityRecord
            {
                RushingPower = ParseAttributeGrade(slot.RushingPower),
                RunningSpeed = ParseAttributeGrade(slot.RunningSpeed),
                MaximumSpeed = ParseAttributeGrade(slot.MaximumSpeed),
                HittingPower = ParseAttributeGrade(slot.HittingPower),
                FaceIdentifier = checked((byte)slot.FaceIdentifier.Value),
                PassInterceptions = ParseAttributeGrade(slot.PassInterceptions!),
                Quickness = ParseAttributeGrade(slot.Quickness!),
            },
            "KP" => new KickerPunterAbilityRecord
            {
                RushingPower = ParseAttributeGrade(slot.RushingPower),
                RunningSpeed = ParseAttributeGrade(slot.RunningSpeed),
                MaximumSpeed = ParseAttributeGrade(slot.MaximumSpeed),
                HittingPower = ParseAttributeGrade(slot.HittingPower),
                FaceIdentifier = checked((byte)slot.FaceIdentifier.Value),
                KickOrPuntAbility = ParseAttributeGrade(slot.KickOrPuntAbility!),
                AvoidKickBlock = ParseAttributeGrade(slot.AvoidKickBlock!),
            },
            _ => throw new InvalidOperationException($"Unsupported Bank1_2 ability role '{slot.Role}'."),
        };
    }

    private static T DeserializeFile<T>(string path)
    {
        string json = File.ReadAllText(path);
        T? value = JsonSerializer.Deserialize<T>(json, JsonOptions);
        return value ?? throw new InvalidOperationException($"Failed to deserialize {path} into {typeof(T).Name}.");
    }

    private static TeamId ParseTeamId(string teamListLabel)
    {
        string key = teamListLabel.Replace("_LIST", string.Empty, StringComparison.Ordinal);
        return key switch
        {
            "BUFFALO" => TeamId.Buffalo,
            "INDIANAPOLIS" => TeamId.Indianapolis,
            "MIAMI" => TeamId.Miami,
            "NEW_ENGLAND" => TeamId.NewEngland,
            "NEW_YORK_JETS" => TeamId.NewYorkJets,
            "CINCINNATI" => TeamId.Cincinnati,
            "CLEVELAND" => TeamId.Cleveland,
            "HOUSTON" => TeamId.Houston,
            "PITTSBURGH" => TeamId.Pittsburgh,
            "DENVER" => TeamId.Denver,
            "KANSAS_CITY" => TeamId.KansasCity,
            "LOS_ANGELES_RAIDERS" => TeamId.LosAngelesRaiders,
            "SAN_DIEGO" => TeamId.SanDiego,
            "SEATTLE" => TeamId.Seattle,
            "WASHINGTON" => TeamId.Washington,
            "NEW_YORK_GIANTS" => TeamId.NewYorkGiants,
            "PHILADELPHIA" => TeamId.Philadelphia,
            "PHOENIX" => TeamId.Phoenix,
            "DALLAS" => TeamId.Dallas,
            "CHICAGO" => TeamId.Chicago,
            "DETROIT" => TeamId.Detroit,
            "GREEN_BAY" => TeamId.GreenBay,
            "MINNESOTA" => TeamId.Minnesota,
            "TAMPA_BAY" => TeamId.TampaBay,
            "SAN_FRANCISCO" => TeamId.SanFrancisco,
            "LOS_ANGELES_RAMS" => TeamId.LosAngelesRams,
            "NEW_ORLEANS" => TeamId.NewOrleans,
            "ATLANTA" => TeamId.Atlanta,
            _ => throw new InvalidOperationException($"Unsupported team list label '{teamListLabel}'."),
        };
    }

    private static RosterSlot ParseRosterSlot(string slot)
    {
        return slot switch
        {
            "QB1" => RosterSlot.Qb1,
            "QB2" => RosterSlot.Qb2,
            "RB1" => RosterSlot.Rb1,
            "RB2" => RosterSlot.Rb2,
            "RB3" => RosterSlot.Rb3,
            "RB4" => RosterSlot.Rb4,
            "WR1" => RosterSlot.Wr1,
            "WR2" => RosterSlot.Wr2,
            "WR3" => RosterSlot.Wr3,
            "WR4" => RosterSlot.Wr4,
            "TE1" => RosterSlot.Te1,
            "TE2" => RosterSlot.Te2,
            "C" => RosterSlot.C,
            "LG" => RosterSlot.Lg,
            "RG" => RosterSlot.Rg,
            "LT" => RosterSlot.Lt,
            "RT" => RosterSlot.Rt,
            "RE" => RosterSlot.Re,
            "NT" => RosterSlot.Nt,
            "LE" => RosterSlot.Le,
            "ROLB" => RosterSlot.Rolb,
            "RILB" => RosterSlot.Rilb,
            "LILB" => RosterSlot.Lilb,
            "LOLB" => RosterSlot.Lolb,
            "RCB" => RosterSlot.Rcb,
            "LCB" => RosterSlot.Lcb,
            "FS" => RosterSlot.Fs,
            "SS" => RosterSlot.Ss,
            "K" => RosterSlot.K,
            "P" => RosterSlot.P,
            _ => throw new InvalidOperationException($"Unsupported roster slot '{slot}'."),
        };
    }

    private static AttributeGrade ParseAttributeGrade(AttributeValueDto dto)
    {
        return dto.SourceLabel switch
        {
            "ATTRIBUTE_6" => AttributeGrade.Grade6,
            "ATTRIBUTE_13" => AttributeGrade.Grade13,
            "ATTRIBUTE_19" => AttributeGrade.Grade19,
            "ATTRIBUTE_25" => AttributeGrade.Grade25,
            "ATTRIBUTE_31" => AttributeGrade.Grade31,
            "ATTRIBUTE_38" => AttributeGrade.Grade38,
            "ATTRIBUTE_44" => AttributeGrade.Grade44,
            "ATTRIBUTE_50" => AttributeGrade.Grade50,
            "ATTRIBUTE_56" => AttributeGrade.Grade56,
            "ATTRIBUTE_63" => AttributeGrade.Grade63,
            "ATTRIBUTE_69" => AttributeGrade.Grade69,
            "ATTRIBUTE_75" => AttributeGrade.Grade75,
            "ATTRIBUTE_81" => AttributeGrade.Grade81,
            "ATTRIBUTE_88" => AttributeGrade.Grade88,
            "ATTRIBUTE_94" => AttributeGrade.Grade94,
            "ATTRIBUTE_100" => AttributeGrade.Grade100,
            _ => throw new InvalidOperationException($"Unsupported attribute grade label '{dto.SourceLabel}'."),
        };
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    private sealed record IdentityRoot
    {
        public required List<IdentityTeamDto> Teams { get; init; }
    }

    private sealed record IdentityTeamDto
    {
        public required int Order { get; init; }

        public required string TeamListLabel { get; init; }

        public required List<IdentityPlayerDto> Players { get; init; }
    }

    private sealed record IdentityPlayerDto
    {
        public required string Slot { get; init; }

        public required string SourceLabel { get; init; }

        public required int JerseyNumber { get; init; }

        public required string SourceNamePayload { get; init; }
    }

    private sealed record AbilityRoot
    {
        public required List<AbilityTeamDto> Teams { get; init; }
    }

    private sealed record AbilityTeamDto
    {
        public required int Order { get; init; }

        public required string TeamListLabel { get; init; }

        public required string AbilityLabel { get; init; }

        public required List<AbilitySlotDto> Slots { get; init; }
    }

    private sealed record AbilitySlotDto
    {
        public required string Slot { get; init; }

        public required string Role { get; init; }

        public required AttributeValueDto RushingPower { get; init; }

        public required AttributeValueDto RunningSpeed { get; init; }

        public required AttributeValueDto MaximumSpeed { get; init; }

        public required AttributeValueDto HittingPower { get; init; }

        public required FaceIdentifierDto FaceIdentifier { get; init; }

        public AttributeValueDto? PassingSpeed { get; init; }

        public AttributeValueDto? PassControl { get; init; }

        public AttributeValueDto? AccuracyOfPassing { get; init; }

        public AttributeValueDto? AvoidPassBlock { get; init; }

        public AttributeValueDto? BallControl { get; init; }

        public AttributeValueDto? Receptions { get; init; }

        public AttributeValueDto? PassInterceptions { get; init; }

        public AttributeValueDto? Quickness { get; init; }

        public AttributeValueDto? KickOrPuntAbility { get; init; }

        public AttributeValueDto? AvoidKickBlock { get; init; }
    }

    private sealed record AttributeValueDto
    {
        public required string SourceLabel { get; init; }

        public required int Value { get; init; }
    }

    private sealed record FaceIdentifierDto
    {
        public required string Hex { get; init; }

        public required int Value { get; init; }
    }
}
