using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Shop inventory data
/// </summary>
public class ItemShopsTable : TableBase<ItemShops>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Shop inventory data (See https://ffhacktics.com/wiki/Shop_Selling_Lists)
/// </summary>
public class ItemShops : DiffableModelBase<ItemShops>, IDiffableModel<ItemShops>, IIdentifiableModel
{
    /// <summary>
    /// Id. No more than 255 in vanilla.
    /// </summary>
    public int Id { get; set; }

    public ShopFlags? Shops { get; set; }

    public static Dictionary<string, DiffablePropertyItem<ItemShops>> PropertyMap { get; } = new()
    {
        [nameof(Shops)] = new DiffablePropertyItem<ItemShops, ShopFlags?>(nameof(Shops), i => i.Shops, (i, v) => i.Shops = v),
    };

    public static ItemShops FromStructure(int id, ref ITEM_SHOPS_DATA @struct)
    {
        var model = new ItemShops()
        {
            Id = id,
            Shops = @struct.Shops,
        };

        return model;
    }

    /// <summary>
    /// Clones the spawn data.
    /// </summary>
    public ItemShops Clone()
    {
        return new ItemShops()
        {
            Id = Id,
            Shops = Shops,
        };
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
