using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Chemist item/consumables secondary data manager.
/// </summary>
public interface IFFTOItemConsumableDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies a consumable item's secondary data entry directly to the game.
    /// </summary>
    /// <param name="modId">Mod id that is changing the item.</param>
    /// <param name="item"></param>
    public void ApplyTablePatch(string modId, ItemConsumable item);

    /// <summary>
    /// Gets a consumable item's secondary data entry, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ItemConsumable GetOriginalConsumableItem(int index);

    /// <summary>
    /// Gets a consumable item's secondary data entry with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ItemConsumable GetConsumableItem(int index);
}