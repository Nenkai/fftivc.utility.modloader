using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Job command table (skill sets). <see href="https://ffhacktics.com/wiki/Skillsets"/>
/// Linked to JobCommand nex table.
/// </summary>
public class JobCommandTable : IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;

    /// <summary>
    /// Command sets.
    /// </summary>
    public List<JobCommand> JobCommands { get; set; } = [];
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Job command set (skill set). <see href="https://ffhacktics.com/wiki/Skillsets"/> <br/>
/// NOTE: Extend id flags are handled automatically when setting an ability id.
/// </summary>
public class JobCommand : DiffableModelBase<JobCommand>, IDiffableModel<JobCommand>
{
    /// <summary>
    /// Id. No more than 512 in vanilla.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Determines whether to use an extended id when fetching for an ability id. If toggled for a specific ability, id is beyond 256.
    /// </summary>
    public ExtendAbilityIdFlags ExtendAbilityIdFlagBits { get; set; }

    /// <summary>
    /// Determines whether to use an extended id when fetching for an RSM id. If toggled for a specific RSM, id is beyond 256.
    /// </summary>
    public ExtendReactionSupportMovementIdFlags ExtendReactionSupportMovementIdFlagBits { get; set; }

    // Should or should not be made an array? Flag handling is really weird.
    public ushort AbilityId1 
    { 
        get;
        set
        {
            ExtendAbilityIdFlagBits = SetFlagU16(ExtendAbilityIdFlagBits, ExtendAbilityIdFlags.ExtendedAbility1, value >= 256);
            field = value;
        }
    }

    public ushort AbilityId2
    {
        get;
        set
        {
            ExtendAbilityIdFlagBits = SetFlagU16(ExtendAbilityIdFlagBits, ExtendAbilityIdFlags.ExtendedAbility2, value >= 256);
            field = value;
        }
    }
    public ushort AbilityId3
    {
        get;
        set
        {
            ExtendAbilityIdFlagBits = SetFlagU16(ExtendAbilityIdFlagBits, ExtendAbilityIdFlags.ExtendedAbility3, value >= 256);
            field = value;
        }
    }
    public ushort AbilityId4
    {
        get;
        set
        {
            ExtendAbilityIdFlagBits = SetFlagU16(ExtendAbilityIdFlagBits, ExtendAbilityIdFlags.ExtendedAbility4, value >= 256);
            field = value;
        }
    }
    public ushort AbilityId5
    {
        get;
        set
        {
            ExtendAbilityIdFlagBits = SetFlagU16(ExtendAbilityIdFlagBits, ExtendAbilityIdFlags.ExtendedAbility5, value >= 256);
            field = value;
        }
    }
    public ushort AbilityId6
    {
        get;
        set
        {
            ExtendAbilityIdFlagBits = SetFlagU16(ExtendAbilityIdFlagBits, ExtendAbilityIdFlags.ExtendedAbility6, value >= 256);
            field = value;
        }
    }
    public ushort AbilityId7
    {
        get;
        set
        {
            ExtendAbilityIdFlagBits = SetFlagU16(ExtendAbilityIdFlagBits, ExtendAbilityIdFlags.ExtendedAbility7, value >= 256);
            field = value;
        }
    }

    public ushort AbilityId8
    {
        get;
        set
        {
            ExtendAbilityIdFlagBits = SetFlagU16(ExtendAbilityIdFlagBits, ExtendAbilityIdFlags.ExtendedAbility8, value >= 256);
            field = value;
        }
    }
    public ushort AbilityId9
    {
        get;
        set
        {
            ExtendAbilityIdFlagBits = SetFlagU16(ExtendAbilityIdFlagBits, ExtendAbilityIdFlags.ExtendedAbility9, value >= 256);
            field = value;
        }
    }
    public ushort AbilityId10
    {
        get;
        set
        {
            ExtendAbilityIdFlagBits = SetFlagU16(ExtendAbilityIdFlagBits, ExtendAbilityIdFlags.ExtendedAbility10, value >= 256);
            field = value;
        }
    }
    public ushort AbilityId11
    {
        get;
        set
        {
            ExtendAbilityIdFlagBits = SetFlagU16(ExtendAbilityIdFlagBits, ExtendAbilityIdFlags.ExtendedAbility11, value >= 256);
            field = value;
        }
    }
    public ushort AbilityId12
    {
        get;
        set
        {
            ExtendAbilityIdFlagBits = SetFlagU16(ExtendAbilityIdFlagBits, ExtendAbilityIdFlags.ExtendedAbility12, value >= 256);
            field = value;
        }
    }
    public ushort AbilityId13
    {
        get;
        set
        {
            ExtendAbilityIdFlagBits = SetFlagU16(ExtendAbilityIdFlagBits, ExtendAbilityIdFlags.ExtendedAbility13, value >= 256);
            field = value;
        }
    }
    public ushort AbilityId14
    {
        get;
        set
        {
            ExtendAbilityIdFlagBits = SetFlagU16(ExtendAbilityIdFlagBits, ExtendAbilityIdFlags.ExtendedAbility14, value >= 256);
            field = value;
        }
    }
    public ushort AbilityId15
    {
        get;
        set
        {
            ExtendAbilityIdFlagBits = SetFlagU16(ExtendAbilityIdFlagBits, ExtendAbilityIdFlags.ExtendedAbility15, value >= 256);
            field = value;
        }
    }
    public ushort AbilityId16
    {
        get;
        set
        {
            ExtendAbilityIdFlagBits = SetFlagU16(ExtendAbilityIdFlagBits, ExtendAbilityIdFlags.ExtendedAbility16, value >= 256);
            field = value;
        }
    }
    public ushort ReactionSupportMovementId1
    {
        get;
        set
        {
            ExtendReactionSupportMovementIdFlagBits = SetFlagU8(ExtendReactionSupportMovementIdFlagBits, ExtendReactionSupportMovementIdFlags.ExtendRSMId1, value >= 256);
            field = value;
        }
    }
    public ushort ReactionSupportMovementId2
    {
        get;
        set
        {
            ExtendReactionSupportMovementIdFlagBits = SetFlagU8(ExtendReactionSupportMovementIdFlagBits, ExtendReactionSupportMovementIdFlags.ExtendRSMId2, value >= 256);
            field = value;
        }
    }
    public ushort ReactionSupportMovementId3
    {
        get;
        set
        {
            ExtendReactionSupportMovementIdFlagBits = SetFlagU8(ExtendReactionSupportMovementIdFlagBits, ExtendReactionSupportMovementIdFlags.ExtendRSMId3, value >= 256);
            field = value;
        }
    }
    public ushort ReactionSupportMovementId4
    {
        get;
        set
        {
            ExtendReactionSupportMovementIdFlagBits = SetFlagU8(ExtendReactionSupportMovementIdFlagBits, ExtendReactionSupportMovementIdFlags.ExtendRSMId4, value >= 256);
            field = value;
        }
    }
    public ushort ReactionSupportMovementId5
    {
        get;
        set
        {
            ExtendReactionSupportMovementIdFlagBits = SetFlagU8(ExtendReactionSupportMovementIdFlagBits, ExtendReactionSupportMovementIdFlags.ExtendRSMId4, value >= 256);
            field = value;
        }
    }
    public ushort ReactionSupportMovementId6
    {
        get;
        set
        {
            ExtendReactionSupportMovementIdFlagBits = SetFlagU8(ExtendReactionSupportMovementIdFlagBits, ExtendReactionSupportMovementIdFlags.ExtendRSMId6, value >= 256);
            field = value;
        }
    }

    public static Dictionary<string, DiffablePropertyItem<JobCommand>> PropertyMap { get; } = new()
    {
        [nameof(ExtendAbilityIdFlagBits)]      = new DiffablePropertyItem<JobCommand, ExtendAbilityIdFlags>(nameof(ExtendAbilityIdFlagBits), i => i.ExtendAbilityIdFlagBits, (i, v) => i.ExtendAbilityIdFlagBits = v),
        [nameof(ExtendReactionSupportMovementIdFlagBits)] = new DiffablePropertyItem<JobCommand, ExtendReactionSupportMovementIdFlags>(nameof(ExtendReactionSupportMovementIdFlagBits), i => i.ExtendReactionSupportMovementIdFlagBits, (i, v) => i.ExtendReactionSupportMovementIdFlagBits = v),
        [nameof(AbilityId1)]                   = new DiffablePropertyItem<JobCommand, ushort>(nameof(AbilityId1), i => i.AbilityId1, (i, v) => i.AbilityId1 = v),
        [nameof(AbilityId2)]                   = new DiffablePropertyItem<JobCommand, ushort>(nameof(AbilityId2), i => i.AbilityId2, (i, v) => i.AbilityId2 = v),
        [nameof(AbilityId3)]                   = new DiffablePropertyItem<JobCommand, ushort>(nameof(AbilityId3), i => i.AbilityId3, (i, v) => i.AbilityId3 = v),
        [nameof(AbilityId4)]                   = new DiffablePropertyItem<JobCommand, ushort>(nameof(AbilityId4), i => i.AbilityId4, (i, v) => i.AbilityId4 = v),
        [nameof(AbilityId5)]                   = new DiffablePropertyItem<JobCommand, ushort>(nameof(AbilityId5), i => i.AbilityId5, (i, v) => i.AbilityId5 = v),
        [nameof(AbilityId6)]                   = new DiffablePropertyItem<JobCommand, ushort>(nameof(AbilityId6), i => i.AbilityId6, (i, v) => i.AbilityId6 = v),
        [nameof(AbilityId7)]                   = new DiffablePropertyItem<JobCommand, ushort>(nameof(AbilityId7), i => i.AbilityId7, (i, v) => i.AbilityId7 = v),
        [nameof(AbilityId8)]                   = new DiffablePropertyItem<JobCommand, ushort>(nameof(AbilityId8), i => i.AbilityId8, (i, v) => i.AbilityId8 = v),
        [nameof(AbilityId9)]                   = new DiffablePropertyItem<JobCommand, ushort>(nameof(AbilityId9), i => i.AbilityId9, (i, v) => i.AbilityId9 = v),
        [nameof(AbilityId10)]                  = new DiffablePropertyItem<JobCommand, ushort>(nameof(AbilityId10), i => i.AbilityId10, (i, v) => i.AbilityId10 = v),
        [nameof(AbilityId11)]                  = new DiffablePropertyItem<JobCommand, ushort>(nameof(AbilityId11), i => i.AbilityId11, (i, v) => i.AbilityId11 = v),
        [nameof(AbilityId12)]                  = new DiffablePropertyItem<JobCommand, ushort>(nameof(AbilityId12), i => i.AbilityId12, (i, v) => i.AbilityId12 = v),
        [nameof(AbilityId13)]                  = new DiffablePropertyItem<JobCommand, ushort>(nameof(AbilityId13), i => i.AbilityId13, (i, v) => i.AbilityId13 = v),
        [nameof(AbilityId14)]                  = new DiffablePropertyItem<JobCommand, ushort>(nameof(AbilityId14), i => i.AbilityId14, (i, v) => i.AbilityId14 = v),
        [nameof(AbilityId15)]                  = new DiffablePropertyItem<JobCommand, ushort>(nameof(AbilityId15), i => i.AbilityId15, (i, v) => i.AbilityId15 = v),
        [nameof(AbilityId16)]                  = new DiffablePropertyItem<JobCommand, ushort>(nameof(AbilityId16), i => i.AbilityId16, (i, v) => i.AbilityId16 = v),
        [nameof(ReactionSupportMovementId1)]   = new DiffablePropertyItem<JobCommand, ushort>(nameof(ReactionSupportMovementId1), i => i.ReactionSupportMovementId1, (i, v) => i.ReactionSupportMovementId1 = v),
        [nameof(ReactionSupportMovementId2)]   = new DiffablePropertyItem<JobCommand, ushort>(nameof(ReactionSupportMovementId2), i => i.ReactionSupportMovementId2, (i, v) => i.ReactionSupportMovementId2 = v),
        [nameof(ReactionSupportMovementId3)]   = new DiffablePropertyItem<JobCommand, ushort>(nameof(ReactionSupportMovementId3), i => i.ReactionSupportMovementId3, (i, v) => i.ReactionSupportMovementId3 = v),
        [nameof(ReactionSupportMovementId4)]   = new DiffablePropertyItem<JobCommand, ushort>(nameof(ReactionSupportMovementId4), i => i.ReactionSupportMovementId4, (i, v) => i.ReactionSupportMovementId4 = v),
        [nameof(ReactionSupportMovementId5)]   = new DiffablePropertyItem<JobCommand, ushort>(nameof(ReactionSupportMovementId5), i => i.ReactionSupportMovementId5, (i, v) => i.ReactionSupportMovementId5 = v),
        [nameof(ReactionSupportMovementId6)]   = new DiffablePropertyItem<JobCommand, ushort>(nameof(ReactionSupportMovementId6), i => i.ReactionSupportMovementId6, (i, v) => i.ReactionSupportMovementId6 = v),
    };

    /// <summary>
    /// Clones the ability.
    /// </summary>
    /// <returns></returns>
    public JobCommand Clone()
    {
        return new JobCommand()
        {
            ExtendAbilityIdFlagBits = ExtendAbilityIdFlagBits,
            ExtendReactionSupportMovementIdFlagBits = ExtendReactionSupportMovementIdFlagBits,
            AbilityId1 = AbilityId1,
            AbilityId2 = AbilityId2,
            AbilityId3 = AbilityId3,
            AbilityId4 = AbilityId4,
            AbilityId5 = AbilityId5,
            AbilityId6 = AbilityId6,
            AbilityId7 = AbilityId7,
            AbilityId8 = AbilityId8,
            AbilityId9 = AbilityId9,
            AbilityId10 = AbilityId10,
            AbilityId11 = AbilityId11,
            AbilityId12 = AbilityId12,
            AbilityId13 = AbilityId13,
            AbilityId14 = AbilityId14,
            AbilityId15 = AbilityId15,
            AbilityId16 = AbilityId16,
            ReactionSupportMovementId1 = ReactionSupportMovementId1,
            ReactionSupportMovementId2 = ReactionSupportMovementId2,
            ReactionSupportMovementId3 = ReactionSupportMovementId3,
            ReactionSupportMovementId4 = ReactionSupportMovementId4,
            ReactionSupportMovementId5 = ReactionSupportMovementId5,
            ReactionSupportMovementId6 = ReactionSupportMovementId6,
        };
    }

    private static T SetFlagU16<T>(T value, T flag, bool condition) where T : Enum
    {
        ushort v = Convert.ToUInt16(value);
        ushort f = Convert.ToUInt16(flag);
        return (T)(object)(ushort)(condition ? (v | f) : (v & ~f));
    }

    private static T SetFlagU8<T>(T value, T flag, bool condition) where T : Enum
    {
        byte v = Convert.ToByte(value);
        byte f = Convert.ToByte(flag);
        return (T)(object)(byte)(condition ? (v | f) : (v & ~f));
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member