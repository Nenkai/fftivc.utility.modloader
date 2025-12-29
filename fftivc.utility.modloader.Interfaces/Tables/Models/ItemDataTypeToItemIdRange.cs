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

public class ItemDataTypeToItemIdRangeTable : TableBase<ItemDataTypeToItemIdRange>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public class ItemDataTypeToItemIdRange : DiffableModelBase<ItemDataTypeToItemIdRange>, IDiffableModel<ItemDataTypeToItemIdRange>, IIdentifiableModel
{
    public int Id { get; set; }
    public uint? ItemIdRangeId { get; set; }

    public static Dictionary<string, DiffablePropertyItem<ItemDataTypeToItemIdRange>> PropertyMap { get; } = new()
    {
        [nameof(ItemIdRangeId)] = new DiffablePropertyItem<ItemDataTypeToItemIdRange, uint?>(nameof(ItemIdRangeId), i => i.ItemIdRangeId, (i, v) => i.ItemIdRangeId = v),
    };

    public static ItemDataTypeToItemIdRange FromStructure(int id, ref ITEM_DATA_TYPE_TO_ITEM_ID_RANGE @struct)
    {
        var model = new ItemDataTypeToItemIdRange()
        {
            Id = id,
            ItemIdRangeId = @struct.ItemIdRangeId,
        };

        return model;
    }

    /// <summary>
    /// Clones the item data type to item id range.
    /// </summary>
    /// <returns></returns>
    public ItemDataTypeToItemIdRange Clone()
    {
        return new ItemDataTypeToItemIdRange()
        {
            Id = Id,
            ItemIdRangeId = ItemIdRangeId,
        };
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
