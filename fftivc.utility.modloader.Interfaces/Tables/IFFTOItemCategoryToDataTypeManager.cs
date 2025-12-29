using fftivc.utility.modloader.Interfaces.Tables.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Item category to additional data type manager.
/// </summary>
public interface IFFTOItemCategoryToDataTypeManager : IFFTOTableManager
{
    /// <summary>
    /// Applies an item category to additional data type directly to the game.
    /// </summary>
    /// <param name="modId">Mod id that is changing the item category to additional data type.</param>
    /// <param name="item"></param>
    public void ApplyTablePatch(string modId, ItemCategoryToDataType item);

    /// <summary>
    /// Gets an item category to additional data type, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ItemCategoryToDataType GetOriginalItemCategoryToDataType(int index);

    /// <summary>
    /// Gets an item category to additional data type with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ItemCategoryToDataType GetItemCategoryToDataType(int index);
}
