using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Accessory item secondary data
/// </summary>
public class ItemAccessoryTable : TableBase<ItemAccessory>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Accessory item secondary data. <see href="https://ffhacktics.com/wiki/Accessory_Secondary_Data"/>
/// </summary>
public class ItemAccessory : DiffableModelBase<ItemAccessory>, IDiffableModel<ItemAccessory>, IIdentifiableModel
{
    public int Id { get; set; }

    public byte? PhysicalEvasion { get; set; }
    public byte? MagicalEvasion { get; set; }

    public static Dictionary<string, DiffablePropertyItem<ItemAccessory>> PropertyMap { get; } = new()
    {
        [nameof(PhysicalEvasion)] = new DiffablePropertyItem<ItemAccessory, byte?>(nameof(PhysicalEvasion), i => i.PhysicalEvasion, (i, v) => i.PhysicalEvasion = v),
        [nameof(MagicalEvasion)] = new DiffablePropertyItem<ItemAccessory, byte?>(nameof(MagicalEvasion), i => i.MagicalEvasion, (i, v) => i.MagicalEvasion = v),
    };

    public static ItemAccessory FromStructure(int id, ref ITEM_ACCESSORY_DATA @struct)
    {
        var data = new ItemAccessory()
        {
            Id = id,
            PhysicalEvasion = @struct.PhysicalEvasion,
            MagicalEvasion = @struct.MagicalEvasion,
        };

        return data;
    }

    /// <summary>
    /// Clones the item.
    /// </summary>
    /// <returns></returns>
    public ItemAccessory Clone()
    {
        return new ItemAccessory()
        {
            Id = Id,
            PhysicalEvasion = PhysicalEvasion,
            MagicalEvasion = MagicalEvasion
        };
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
