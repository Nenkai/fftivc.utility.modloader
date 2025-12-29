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

public class ItemIdRangeToCategoryTable : TableBase<ItemIdRangeToCategory>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public class ItemIdRangeToCategory : DiffableModelBase<ItemIdRangeToCategory>, IDiffableModel<ItemIdRangeToCategory>, IIdentifiableModel
{
    public int Id { get; set; }
    public ushort? StartItemId { get; set; }

    public static Dictionary<string, DiffablePropertyItem<ItemIdRangeToCategory>> PropertyMap { get; } = new()
    {
        [nameof(StartItemId)] =          new DiffablePropertyItem<ItemIdRangeToCategory, ushort?>(nameof(StartItemId), i => i.StartItemId, (i, v) => i.StartItemId = v),
    };

    public static ItemIdRangeToCategory FromStructure(int id, ref ITEM_ID_RANGE_TO_CATEGORY_DATA @struct)
    {
        var item = new ItemIdRangeToCategory()
        {
            Id = id,
            StartItemId = @struct.StartItemId,
        };

        return item;
    }

    /// <summary>
    /// Clones the item category range.
    /// </summary>
    /// <returns></returns>
    public ItemIdRangeToCategory Clone()
    {
        return new ItemIdRangeToCategory()
        {
            Id = Id,
            StartItemId = StartItemId,
        };
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
