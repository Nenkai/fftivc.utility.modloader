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

public class ItemCategoryToDataTypeTable : TableBase<ItemCategoryToDataType>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public class ItemCategoryToDataType : DiffableModelBase<ItemCategoryToDataType>, IDiffableModel<ItemCategoryToDataType>, IIdentifiableModel
{
    public int Id { get; set; }
    public uint? DataTypeId { get; set; }

    public static Dictionary<string, DiffablePropertyItem<ItemCategoryToDataType>> PropertyMap { get; } = new()
    {
        [nameof(DataTypeId)] = new DiffablePropertyItem<ItemCategoryToDataType, uint?>(nameof(DataTypeId), i => i.DataTypeId, (i, v) => i.DataTypeId = v),
    };

    public static ItemCategoryToDataType FromStructure(int id, ref ITEM_CATEGORY_TO_DATA_TYPE_DATA @struct)
    {
        var model = new ItemCategoryToDataType()
        {
            Id = id,
            DataTypeId = @struct.DataTypeId,
        };

        return model;
    }

    /// <summary>
    /// Clones the item category range.
    /// </summary>
    /// <returns></returns>
    public ItemCategoryToDataType Clone()
    {
        return new ItemCategoryToDataType()
        {
            Id = Id,
            DataTypeId = DataTypeId,
        };
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
