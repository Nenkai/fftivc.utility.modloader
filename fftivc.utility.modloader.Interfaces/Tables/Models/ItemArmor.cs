using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Armor item secondary data
/// </summary>
public class ItemArmorTable : TableBase<ItemArmor>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Armor secondary data. <see href="https://ffhacktics.com/wiki/Helm/Armor_Secondary_Data"/>
/// </summary>
public class ItemArmor : DiffableModelBase<ItemArmor>, IDiffableModel<ItemArmor>, IIdentifiableModel
{
    public int Id { get; set; }

    public byte? HPBonus { get; set; }
    public byte? MPBonus { get; set; }

    public static Dictionary<string, DiffablePropertyItem<ItemArmor>> PropertyMap { get; } = new()
    {
        [nameof(HPBonus)] = new DiffablePropertyItem<ItemArmor, byte?>(nameof(HPBonus), i => i.HPBonus, (i, v) => i.HPBonus = v),
        [nameof(MPBonus)] = new DiffablePropertyItem<ItemArmor, byte?>(nameof(MPBonus), i => i.MPBonus, (i, v) => i.MPBonus = v),
    };

    public static ItemArmor FromStructure(int id, ref ITEM_ARMOR_DATA @struct)
    {
        var data = new ItemArmor()
        {
            Id = id,
            HPBonus = @struct.HPBonus,
            MPBonus = @struct.MPBonus,
        };

        return data;
    }

    /// <summary>
    /// Clones the item.
    /// </summary>
    /// <returns></returns>
    public ItemArmor Clone()
    {
        return new ItemArmor()
        {
            Id = Id,
            HPBonus = HPBonus,
            MPBonus = MPBonus
        };
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
