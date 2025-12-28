using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Shield item secondary data
/// </summary>
public class ItemShieldTable : TableBase<ItemShield>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Shield item secondary Data. <see href="https://ffhacktics.com/wiki/Shield_Secondary_Data"/>
/// </summary>
public class ItemShield : DiffableModelBase<ItemShield>, IDiffableModel<ItemShield>, IIdentifiableModel
{
    public int Id { get; set; }

    public byte? PhysicalEvasion { get; set; }
    public byte? MagicalEvasion { get; set; }

    public static Dictionary<string, DiffablePropertyItem<ItemShield>> PropertyMap { get; } = new()
    {
        [nameof(PhysicalEvasion)] = new DiffablePropertyItem<ItemShield, byte?>(nameof(PhysicalEvasion), i => i.PhysicalEvasion, (i, v) => i.PhysicalEvasion = v),
        [nameof(MagicalEvasion)] = new DiffablePropertyItem<ItemShield, byte?>(nameof(MagicalEvasion), i => i.MagicalEvasion, (i, v) => i.MagicalEvasion = v),
    };

    public static ItemShield FromStructure(int id, ref ITEM_SHIELD_DATA @struct)
    {
        var data = new ItemShield()
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
    public ItemShield Clone()
    {
        return new ItemShield()
        {
            Id = Id,
            PhysicalEvasion = PhysicalEvasion,
            MagicalEvasion = MagicalEvasion
        };
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
