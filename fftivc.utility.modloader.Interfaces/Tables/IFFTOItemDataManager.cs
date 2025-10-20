using fftivc.utility.modloader.Interfaces.Tables.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Item data manager.
/// </summary>
public interface IFFTOItemDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies an item directly to the game.
    /// </summary>
    /// <param name="modId">Mod id that is changing the item.</param>
    /// <param name="item"></param>
    public void ApplyTablePatch(string modId, Item item);

    /// <summary>
    /// Gets an item, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Item GetOriginalItem(int index);

    /// <summary>
    /// Gets an item with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Item GetItem(int index);
}
