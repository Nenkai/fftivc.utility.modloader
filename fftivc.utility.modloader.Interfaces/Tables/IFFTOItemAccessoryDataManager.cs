using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Accessory item secondary data manager.
/// </summary>
public interface IFFTOItemAccessoryDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies an accessory item's secondary data entry directly to the game.
    /// </summary>
    /// <param name="modId">Mod id that is changing the item.</param>
    /// <param name="item"></param>
    public void ApplyTablePatch(string modId, ItemAccessory item);

    /// <summary>
    /// Gets an accessory item's secondary data entry, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ItemAccessory GetOriginalAccessoryItem(int index);

    /// <summary>
    /// Gets an accessory item's secondary data entry with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ItemAccessory GetAccessoryItem(int index);
}
