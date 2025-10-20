using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Item table. <see href="https://ffhacktics.com/wiki/Item_Data"/>
/// </summary>
public class ItemTable : IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;

    /// <summary>
    /// Abilities.
    /// </summary>
    public List<Item> Items { get; set; } = [];
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Item. <see href="https://ffhacktics.com/wiki/Item_Data"/>
/// </summary>
public class Item : DiffableModelBase<Item>, IDiffableModel<Item>
{
    /// <summary>
    /// Id. No more than 512 in vanilla. <br/>
    /// 254 is random. 255 should not be used/empty.
    /// </summary>
    public int Id { get; set; }

    public byte Palette { get; set; }
    public byte SpriteID { get; set; }
    public byte RequiredLevel { get; set; }
    public ItemTypeFlags TypeFlags { get; set; }

    /// <summary>
    /// or previously 'Second Table ID' as per hacktics
    /// </summary>
    public byte AdditionalDataId { get; set; }

    public ItemCategory ItemCategory { get; set; } // eGetItemSmallCategory
    public byte Unused_0x06 { get; set; }

    /// <summary>
    /// or previously 'Item Attributes' as per hacktics
    /// </summary>
    public byte EquipBonusId { get; set; } // pspItemGetEquipBonus
    public ushort Price { get; set; } // GetItemPrice
    public ItemShopAvailability ShopAvailability { get; set; }
    public byte Unused_0x0B { get; set; }

    public static Dictionary<string, DiffablePropertyItem<Item>> PropertyMap { get; } = new()
    {
        [nameof(Palette)] =          new DiffablePropertyItem<Item, byte>(nameof(Palette), i => i.Palette, (i, v) => i.Palette = v),
        [nameof(SpriteID)] =         new DiffablePropertyItem<Item, byte>(nameof(SpriteID), i => i.SpriteID, (i, v) => i.SpriteID = v),
        [nameof(RequiredLevel)] =    new DiffablePropertyItem<Item, byte>(nameof(RequiredLevel), i => i.RequiredLevel, (i, v) => i.RequiredLevel =v),
        [nameof(TypeFlags)] =        new DiffablePropertyItem<Item, ItemTypeFlags>(nameof(TypeFlags), i => i.TypeFlags, (i, v) => i.TypeFlags = v),
        [nameof(AdditionalDataId)] = new DiffablePropertyItem<Item, byte>(nameof(AdditionalDataId), i => i.AdditionalDataId, (i, v) => i.AdditionalDataId = v),
        [nameof(ItemCategory)] =     new DiffablePropertyItem<Item, ItemCategory>(nameof(ItemCategory), i => i.ItemCategory, (i, v) => i.ItemCategory = v),
        [nameof(Unused_0x06)] =      new DiffablePropertyItem<Item, byte>(nameof(Unused_0x06), i => i.Unused_0x06, (i, v) => i.Unused_0x06 = v),
        [nameof(EquipBonusId)] =     new DiffablePropertyItem<Item, byte>(nameof(EquipBonusId), i => i.EquipBonusId, (i, v) => i.EquipBonusId = v),
        [nameof(Price)] =            new DiffablePropertyItem<Item, ushort>(nameof(Price), i => i.Price, (i, v) => i.Price = v),
        [nameof(ShopAvailability)] = new DiffablePropertyItem<Item, ItemShopAvailability>(nameof(ShopAvailability), i => i.ShopAvailability, (i, v) => i.ShopAvailability = v),
        [nameof(Unused_0x0B)] =      new DiffablePropertyItem<Item, byte>(nameof(Unused_0x0B), i => i.Unused_0x0B, (i, v) => i.Unused_0x0B = v),
    };

    /// <summary>
    /// Clones the item.
    /// </summary>
    /// <returns></returns>
    public Item Clone()
    {
        return new Item()
        {
            Id = Id,
            Palette = Palette,
            SpriteID = SpriteID,
            RequiredLevel = RequiredLevel,
            TypeFlags = TypeFlags,
            AdditionalDataId = AdditionalDataId,
            ItemCategory = ItemCategory,
            Unused_0x06 = Unused_0x06,
            EquipBonusId = EquipBonusId,
            Price = Price,
            ShopAvailability = ShopAvailability,
            Unused_0x0B = Unused_0x0B,
        };
    }

    public IList<ModelDiff> DiffModel(Item other)
    {
        List<ModelDiff> changes = [];
        foreach (var propItem in PropertyMap)
        {
            if (propItem.Value.HasChanged(this, other))
                changes.Add(propItem.Value.CreateDiff(this, other));
        }

        return changes;
    }

    public void ApplyChanges(IEnumerable<ModelDiff> diffs)
    {
        foreach (var diff in diffs)
        {
            if (PropertyMap.TryGetValue(diff.Name, out DiffablePropertyItem<Item>? prop))
                prop.Apply(this, diff);
        }
    }

    public void ApplyChange(ModelDiff diff)
    {
        if (PropertyMap.TryGetValue(diff.Name, out DiffablePropertyItem<Item>? prop))
            prop.Apply(this, diff);
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
