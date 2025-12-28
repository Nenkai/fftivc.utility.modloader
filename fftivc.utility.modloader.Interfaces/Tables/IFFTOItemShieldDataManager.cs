using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Shield item secondary data manager.
/// </summary>
public interface IFFTOItemShieldDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies a shield item's secondary data entry directly to the game.
    /// </summary>
    /// <param name="modId">Mod id that is changing the item.</param>
    /// <param name="item"></param>
    public void ApplyTablePatch(string modId, ItemShield item);

    /// <summary>
    /// Gets a shield item's secondary data entry, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ItemShield GetOriginalShieldItem(int index);

    /// <summary>
    /// Gets a shield item's secondary data entry with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ItemShield GetShieldItem(int index);
}
