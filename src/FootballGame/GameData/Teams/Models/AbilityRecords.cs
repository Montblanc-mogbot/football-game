namespace FootballGame.GameData.Teams.Models;

public abstract record BaseAbilityRecord
{
    public required AttributeGrade RushingPower { get; init; }

    public required AttributeGrade RunningSpeed { get; init; }

    public required AttributeGrade MaximumSpeed { get; init; }

    public required AttributeGrade HittingPower { get; init; }

    public required byte FaceIdentifier { get; init; }
}

public sealed record QuarterbackAbilityRecord : BaseAbilityRecord
{
    public required AttributeGrade PassingSpeed { get; init; }

    public required AttributeGrade PassControl { get; init; }

    public required AttributeGrade AccuracyOfPassing { get; init; }

    public required AttributeGrade AvoidPassBlock { get; init; }
}

public sealed record SkillPositionAbilityRecord : BaseAbilityRecord
{
    public required AttributeGrade BallControl { get; init; }

    public required AttributeGrade Receptions { get; init; }
}

public sealed record OffensiveLineAbilityRecord : BaseAbilityRecord;

public sealed record DefenderAbilityRecord : BaseAbilityRecord
{
    public required AttributeGrade PassInterceptions { get; init; }

    public required AttributeGrade Quickness { get; init; }
}

public sealed record KickerPunterAbilityRecord : BaseAbilityRecord
{
    public required AttributeGrade KickOrPuntAbility { get; init; }

    public required AttributeGrade AvoidKickBlock { get; init; }
}
