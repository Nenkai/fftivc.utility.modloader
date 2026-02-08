using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Item Shops data manager.
/// </summary>
public interface IFFTOItemShopsDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies an item's shops directly to the game.
    /// </summary>
    /// <param name="modId">Mod id that is changing the item's shops.</param>
    /// <param name="shops"></param>
    public void ApplyTablePatch(string modId, ItemShops shops);

    /// <summary>
    /// Gets an item's shops, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ItemShops GetOriginalItemShops(int index);

    /// <summary>
    /// Gets an item's shops with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ItemShops GetItemShops(int index);
}
