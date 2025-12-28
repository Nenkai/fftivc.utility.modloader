using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Consunable item secondary data
/// </summary>
public class ItemConsumableTable : TableBase<ItemConsumable>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Consunable item secondary data. <see href="https://ffhacktics.com/wiki/Item_Secondary_Data"/>
/// </summary>
public class ItemConsumable : DiffableModelBase<ItemConsumable>, IDiffableModel<ItemConsumable>, IIdentifiableModel
{
    public int Id { get; set; }

    public byte? Formula { get; set; }
    public byte? Z { get; set; }
    public byte? StatusEffectId { get; set; }

    public static Dictionary<string, DiffablePropertyItem<ItemConsumable>> PropertyMap { get; } = new()
    {
        [nameof(Formula)] = new DiffablePropertyItem<ItemConsumable, byte?>(nameof(Formula), i => i.Formula, (i, v) => i.Formula = v),
        [nameof(Z)] = new DiffablePropertyItem<ItemConsumable, byte?>(nameof(Z), i => i.Z, (i, v) => i.Z = v),
        [nameof(StatusEffectId)] = new DiffablePropertyItem<ItemConsumable, byte?>(nameof(StatusEffectId), i => i.StatusEffectId, (i, v) => i.StatusEffectId = v)
    };

    public static ItemConsumable FromStructure(int id, ref ITEM_CONSUMABLE_DATA @struct)
    {
        var data = new ItemConsumable()
        {
            Id = id,
            Formula = @struct.Formula,
            Z = @struct.Z,
            StatusEffectId = @struct.StatusEffectId,
        };

        return data;
    }

    /// <summary>
    /// Clones the item.
    /// </summary>
    /// <returns></returns>
    public ItemConsumable Clone()
    {
        return new ItemConsumable()
        {
            Id = Id,
            Formula = Formula,
            Z = Z,
            StatusEffectId = StatusEffectId
        };
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
