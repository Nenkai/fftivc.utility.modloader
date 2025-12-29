using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Armor item secondary data manager.
/// </summary>
public interface IFFTOItemArmorDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies an armor item's secondary data entry directly to the game.
    /// </summary>
    /// <param name="modId">Mod id that is changing the item.</param>
    /// <param name="item"></param>
    public void ApplyTablePatch(string modId, ItemArmor item);

    /// <summary>
    /// Gets an armor item's secondary data entry, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ItemArmor GetOriginalArmorItem(int index);

    /// <summary>
    /// Gets an armor item's secondary data entry with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ItemArmor GetArmorItem(int index);
}
