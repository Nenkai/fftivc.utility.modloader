using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Monster Job command table (skill sets).
/// </summary>
public class MonsterJobCommandTable : TableBase<MonsterJobCommand>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Monster Job command set (skill set).
/// NOTE: Extend id flags are handled automatically when setting an ability id.
/// </summary>
public class MonsterJobCommand : DiffableModelBase<MonsterJobCommand>, IDiffableModel<MonsterJobCommand>, IIdentifiableModel
{
    /// <summary>
    /// Id. 176 (Chocobo) - 223 (Tiamat)
    /// </summary>
    public int Id { get; set; }

    public ushort? AbilityId1 { get; set; }
    public ushort? AbilityId2 { get; set; }
    public ushort? AbilityId3 { get; set; }
    public ushort? AbilityId4 { get; set; }

    public static Dictionary<string, DiffablePropertyItem<MonsterJobCommand>> PropertyMap { get; } = new()
    {
        [nameof(AbilityId1)] = new DiffablePropertyItem<MonsterJobCommand, ushort?>(nameof(AbilityId1), i => i.AbilityId1, (i, v) => i.AbilityId1 = v),
        [nameof(AbilityId2)] = new DiffablePropertyItem<MonsterJobCommand, ushort?>(nameof(AbilityId2), i => i.AbilityId2, (i, v) => i.AbilityId2 = v),
        [nameof(AbilityId3)] = new DiffablePropertyItem<MonsterJobCommand, ushort?>(nameof(AbilityId3), i => i.AbilityId3, (i, v) => i.AbilityId3 = v),
        [nameof(AbilityId4)] = new DiffablePropertyItem<MonsterJobCommand, ushort?>(nameof(AbilityId4), i => i.AbilityId4, (i, v) => i.AbilityId4 = v),
    };

    public static MonsterJobCommand FromStructure(int id, ref MONSTER_JOB_COMMAND_DATA @struct)
    {
        var jobCommand = new MonsterJobCommand()
        {
            Id = id,
            AbilityId1 = @struct.ExtendMonsterAbilityIdFlagBits.HasFlag(ExtendMonsterAbilityIdFlags.ExtendedAbility1) ? (ushort)(@struct.AbilityId1 + 256) : @struct.AbilityId1,
            AbilityId2 = @struct.ExtendMonsterAbilityIdFlagBits.HasFlag(ExtendMonsterAbilityIdFlags.ExtendedAbility2) ? (ushort)(@struct.AbilityId2 + 256) : @struct.AbilityId2,
            AbilityId3 = @struct.ExtendMonsterAbilityIdFlagBits.HasFlag(ExtendMonsterAbilityIdFlags.ExtendedAbility3) ? (ushort)(@struct.AbilityId3 + 256) : @struct.AbilityId3,
            AbilityId4 = @struct.ExtendMonsterAbilityIdFlagBits.HasFlag(ExtendMonsterAbilityIdFlags.ExtendedAbility4) ? (ushort)(@struct.AbilityId4 + 256) : @struct.AbilityId4,
        };

        return jobCommand;
    }

    /// <summary>
    /// Clones the monster job commands.
    /// </summary>
    /// <returns></returns>
    public MonsterJobCommand Clone()
    {
        return new MonsterJobCommand()
        {
            Id = Id,
            AbilityId1 = AbilityId1,
            AbilityId2 = AbilityId2,
            AbilityId3 = AbilityId3,
            AbilityId4 = AbilityId4,
        };
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member