using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Jump ability secondary data
/// </summary>
public class AbilityJumpTable : TableBase<AbilityJump>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Jump ability secondary data. <see href="https://ffhacktics.com/wiki/Ability_Secondary_Data:_Jump_Abilities"/>
/// </summary>
public class AbilityJump : DiffableModelBase<AbilityJump>, IDiffableModel<AbilityJump>, IIdentifiableModel
{
    public int Id { get; set; }

    public byte? Range { get; set; }
    public byte? Vertical { get; set; }

    public static Dictionary<string, DiffablePropertyItem<AbilityJump>> PropertyMap { get; } = new()
    {
        [nameof(Range)] = new DiffablePropertyItem<AbilityJump, byte?>(nameof(Range), i => i.Range, (i, v) => i.Range = v),
        [nameof(Vertical)] = new DiffablePropertyItem<AbilityJump, byte?>(nameof(Vertical), i => i.Vertical, (i, v) => i.Vertical = v),
    };

    public static AbilityJump FromStructure(int id, ref ABILITY_JUMP_DATA @struct)
    {
        var data = new AbilityJump()
        {
            Id = id,
            Range = @struct.Range,
            Vertical = @struct.Vertical,
        };

        return data;
    }

    /// <summary>
    /// Clones the ability data.
    /// </summary>
    public AbilityJump Clone()
    {
        return new AbilityJump()
        {
            Id = Id,
            Range = Range,
            Vertical = Vertical,
        };
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
