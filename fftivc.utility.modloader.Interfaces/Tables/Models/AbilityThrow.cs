using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Throw ability secondary data
/// </summary>
public class AbilityThrowTable : TableBase<AbilityThrow>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Throw ability secondary data. <see href="https://ffhacktics.com/wiki/Ability_Secondary_Data:_Throw_Abilities"/>
/// </summary>
public class AbilityThrow : DiffableModelBase<AbilityThrow>, IDiffableModel<AbilityThrow>, IIdentifiableModel
{
    public int Id { get; set; }

    public ThrowItemType? ItemType { get; set; }

    public static Dictionary<string, DiffablePropertyItem<AbilityThrow>> PropertyMap { get; } = new()
    {
        [nameof(ItemType)] = new DiffablePropertyItem<AbilityThrow, ThrowItemType?>(nameof(ItemType), i => i.ItemType, (i, v) => i.ItemType = v),
    };

    public static AbilityThrow FromStructure(int id, ref ABILITY_THROW_DATA @struct)
    {
        var data = new AbilityThrow()
        {
            Id = id,
            ItemType = @struct.ItemType,
        };

        return data;
    }

    /// <summary>
    /// Clones the ability data.
    /// </summary>
    public AbilityThrow Clone()
    {
        return new AbilityThrow()
        {
            Id = Id,
            ItemType = ItemType,
        };
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
