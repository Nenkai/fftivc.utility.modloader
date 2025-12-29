using fftivc.utility.modloader.Interfaces.Tables.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Item id range to category manager.
/// </summary>
public interface IFFTODataTypeToItemIdRangeManager : IFFTOTableManager
{
    /// <summary>
    /// Applies an item data type to item id range directly to the game.
    /// </summary>
    /// <param name="modId">Mod id that is changing the item category range.</param>
    /// <param name="item"></param>
    public void ApplyTablePatch(string modId, ItemDataTypeToItemIdRange item);

    /// <summary>
    /// Gets an item data type to item id range, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ItemDataTypeToItemIdRange GetOriginalItemDataTypeToItemIdRange(int index);

    /// <summary>
    /// Gets an item data type to item id range with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ItemDataTypeToItemIdRange GetItemDataTypeToItemIdRange(int index);
}
