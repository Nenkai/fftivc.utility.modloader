using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Weapon item secondary data manager.
/// </summary>
public interface IFFTOItemWeaponDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies a weapon item's secondary data entry directly to the game.
    /// </summary>
    /// <param name="modId">Mod id that is changing the item.</param>
    /// <param name="item"></param>
    public void ApplyTablePatch(string modId, ItemWeapon item);

    /// <summary>
    /// Gets a weapon item's secondary data entry, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ItemWeapon GetOriginalWeaponItem(int index);

    /// <summary>
    /// Gets a weapon item's secondary data entry with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ItemWeapon GetWeaponItem(int index);
}
