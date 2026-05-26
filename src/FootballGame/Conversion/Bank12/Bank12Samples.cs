using FootballGame.Conversion.Bank12.Models;

namespace FootballGame.Conversion.Bank12;

/// <summary>
/// Small hand-authored sample artifacts for Bank1_2 review.
/// These are not a full parser; they prove the intended decoded model shape.
/// </summary>
public static class Bank12Samples
{
    public static TeamRosterRecord BuffaloRoster => new()
    {
        TeamId = TeamId.Buffalo,
        TeamListLabel = "BUFFALO_LIST",
        PlayersInCanonicalSlotOrder = new PlayerIdentityRecord[]
        {
            new() { TeamId = TeamId.Buffalo, RosterSlot = RosterSlot.Qb1, SourceLabel = "BUFFALO_QB1", JerseyNumber = 0x00, SourceNamePayload = "qbBILLS" },
            new() { TeamId = TeamId.Buffalo, RosterSlot = RosterSlot.Qb2, SourceLabel = "BUFFALO_QB2", JerseyNumber = 0x14, SourceNamePayload = "frankREICH" },
            new() { TeamId = TeamId.Buffalo, RosterSlot = RosterSlot.Rb1, SourceLabel = "BUFFALO_RB1", JerseyNumber = 0x34, SourceNamePayload = "thurmanTHOMAS" },
            new() { TeamId = TeamId.Buffalo, RosterSlot = RosterSlot.Rb2, SourceLabel = "BUFFALO_RB2", JerseyNumber = 0x41, SourceNamePayload = "jamieMUELLER" },
            new() { TeamId = TeamId.Buffalo, RosterSlot = RosterSlot.Rb3, SourceLabel = "BUFFALO_RB3", JerseyNumber = 0x23, SourceNamePayload = "kennethDAVIS" },
            new() { TeamId = TeamId.Buffalo, RosterSlot = RosterSlot.Rb4, SourceLabel = "BUFFALO_RB4", JerseyNumber = 0x30, SourceNamePayload = "donSMITH" },
            new() { TeamId = TeamId.Buffalo, RosterSlot = RosterSlot.Wr1, SourceLabel = "BUFFALO_WR1", JerseyNumber = 0x80, SourceNamePayload = "jamesLOFTON" },
            new() { TeamId = TeamId.Buffalo, RosterSlot = RosterSlot.Wr2, SourceLabel = "BUFFALO_WR2", JerseyNumber = 0x83, SourceNamePayload = "andreREED" },
            new() { TeamId = TeamId.Buffalo, RosterSlot = RosterSlot.Wr3, SourceLabel = "BUFFALO_WR3", JerseyNumber = 0x82, SourceNamePayload = "donBEEBE" },
            new() { TeamId = TeamId.Buffalo, RosterSlot = RosterSlot.Wr4, SourceLabel = "BUFFALO_WR4", JerseyNumber = 0x85, SourceNamePayload = "alEDWARDS" },
            new() { TeamId = TeamId.Buffalo, RosterSlot = RosterSlot.Te1, SourceLabel = "BUFFALO_TE1", JerseyNumber = 0x84, SourceNamePayload = "keithMCKELLER" },
            new() { TeamId = TeamId.Buffalo, RosterSlot = RosterSlot.Te2, SourceLabel = "BUFFALO_TE2", JerseyNumber = 0x88, SourceNamePayload = "peteMETZELAARS" },
            new() { TeamId = TeamId.Buffalo, RosterSlot = RosterSlot.C, SourceLabel = "BUFFALO_C", JerseyNumber = 0x67, SourceNamePayload = "kentHULL" },
            new() { TeamId = TeamId.Buffalo, RosterSlot = RosterSlot.Lg, SourceLabel = "BUFFALO_LG", JerseyNumber = 0x51, SourceNamePayload = "jimRITCHER" },
            new() { TeamId = TeamId.Buffalo, RosterSlot = RosterSlot.Rg, SourceLabel = "BUFFALO_RG", JerseyNumber = 0x65, SourceNamePayload = "johnDAVIS" },
            new() { TeamId = TeamId.Buffalo, RosterSlot = RosterSlot.Lt, SourceLabel = "BUFFALO_LT", JerseyNumber = 0x69, SourceNamePayload = "willWOLFORD" },
            new() { TeamId = TeamId.Buffalo, RosterSlot = RosterSlot.Rt, SourceLabel = "BUFFALO_RT", JerseyNumber = 0x75, SourceNamePayload = "howardBALLARD" },
            new() { TeamId = TeamId.Buffalo, RosterSlot = RosterSlot.Re, SourceLabel = "BUFFALO_RE", JerseyNumber = 0x78, SourceNamePayload = "bruceSMITH" },
            new() { TeamId = TeamId.Buffalo, RosterSlot = RosterSlot.Nt, SourceLabel = "BUFFALO_NT", JerseyNumber = 0x91, SourceNamePayload = "jeffWRIGHT" },
            new() { TeamId = TeamId.Buffalo, RosterSlot = RosterSlot.Le, SourceLabel = "BUFFALO_LE", JerseyNumber = 0x96, SourceNamePayload = "leonSEALS" },
            new() { TeamId = TeamId.Buffalo, RosterSlot = RosterSlot.Rolb, SourceLabel = "BUFFALO_ROLB", JerseyNumber = 0x56, SourceNamePayload = "darrylTALLEY" },
            new() { TeamId = TeamId.Buffalo, RosterSlot = RosterSlot.Rilb, SourceLabel = "BUFFALO_RILB", JerseyNumber = 0x50, SourceNamePayload = "rayBENTLEY" },
            new() { TeamId = TeamId.Buffalo, RosterSlot = RosterSlot.Lilb, SourceLabel = "BUFFALO_LILB", JerseyNumber = 0x58, SourceNamePayload = "shaneCONLAN" },
            new() { TeamId = TeamId.Buffalo, RosterSlot = RosterSlot.Lolb, SourceLabel = "BUFFALO_LOLB", JerseyNumber = 0x97, SourceNamePayload = "c.BENNETT" },
            new() { TeamId = TeamId.Buffalo, RosterSlot = RosterSlot.Rcb, SourceLabel = "BUFFALO_RCB", JerseyNumber = 0x37, SourceNamePayload = "nateODOMES" },
            new() { TeamId = TeamId.Buffalo, RosterSlot = RosterSlot.Lcb, SourceLabel = "BUFFALO_LCB", JerseyNumber = 0x47, SourceNamePayload = "kirbyJACKSON" },
            new() { TeamId = TeamId.Buffalo, RosterSlot = RosterSlot.Fs, SourceLabel = "BUFFALO_FS", JerseyNumber = 0x38, SourceNamePayload = "markKELSO" },
            new() { TeamId = TeamId.Buffalo, RosterSlot = RosterSlot.Ss, SourceLabel = "BUFFALO_SS", JerseyNumber = 0x46, SourceNamePayload = "leonardSMITH" },
            new() { TeamId = TeamId.Buffalo, RosterSlot = RosterSlot.K, SourceLabel = "BUFFALO_K", JerseyNumber = 0x11, SourceNamePayload = "scottNORWOOD" },
            new() { TeamId = TeamId.Buffalo, RosterSlot = RosterSlot.P, SourceLabel = "BUFFALO_P", JerseyNumber = 0x10, SourceNamePayload = "rickTUTEN" },
        },
    };

    public static TeamAbilitySet BuffaloAbilities => new()
    {
        TeamId = TeamId.Buffalo,
        SourceLabel = "BUFFALO_BILLS_ABILITIES",
        AbilitiesBySlot = new Dictionary<RosterSlot, BaseAbilityRecord>
        {
            [RosterSlot.Qb1] = new QuarterbackAbilityRecord
            {
                RushingPower = AttributeGrade.Grade69,
                RunningSpeed = AttributeGrade.Grade25,
                MaximumSpeed = AttributeGrade.Grade13,
                HittingPower = AttributeGrade.Grade13,
                FaceIdentifier = 0x52,
                PassingSpeed = AttributeGrade.Grade56,
                PassControl = AttributeGrade.Grade81,
                AccuracyOfPassing = AttributeGrade.Grade81,
                AvoidPassBlock = AttributeGrade.Grade81,
            },
            [RosterSlot.Rb1] = new SkillPositionAbilityRecord
            {
                RushingPower = AttributeGrade.Grade69,
                RunningSpeed = AttributeGrade.Grade38,
                MaximumSpeed = AttributeGrade.Grade63,
                HittingPower = AttributeGrade.Grade25,
                FaceIdentifier = 0x83,
                BallControl = AttributeGrade.Grade75,
                Receptions = AttributeGrade.Grade50,
            },
            [RosterSlot.Rb2] = new SkillPositionAbilityRecord
            {
                RushingPower = AttributeGrade.Grade69,
                RunningSpeed = AttributeGrade.Grade44,
                MaximumSpeed = AttributeGrade.Grade25,
                HittingPower = AttributeGrade.Grade88,
                FaceIdentifier = 0x51,
                BallControl = AttributeGrade.Grade50,
                Receptions = AttributeGrade.Grade25,
            },
            [RosterSlot.C] = new OffensiveLineAbilityRecord
            {
                RushingPower = AttributeGrade.Grade69,
                RunningSpeed = AttributeGrade.Grade25,
                MaximumSpeed = AttributeGrade.Grade38,
                HittingPower = AttributeGrade.Grade69,
                FaceIdentifier = 0x1E,
            },
            [RosterSlot.Re] = new DefenderAbilityRecord
            {
                RushingPower = AttributeGrade.Grade94,
                RunningSpeed = AttributeGrade.Grade25,
                MaximumSpeed = AttributeGrade.Grade69,
                HittingPower = AttributeGrade.Grade88,
                FaceIdentifier = 0x22,
                PassInterceptions = AttributeGrade.Grade44,
                Quickness = AttributeGrade.Grade63,
            },
            [RosterSlot.K] = new KickerPunterAbilityRecord
            {
                RushingPower = AttributeGrade.Grade69,
                RunningSpeed = AttributeGrade.Grade25,
                MaximumSpeed = AttributeGrade.Grade19,
                HittingPower = AttributeGrade.Grade13,
                FaceIdentifier = 0x55,
                KickOrPuntAbility = AttributeGrade.Grade81,
                AvoidKickBlock = AttributeGrade.Grade44,
            },
        },
    };
}
