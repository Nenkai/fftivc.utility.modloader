using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// ItemEquipBonus data manager.
/// </summary>
public interface IFFTOItemEquipBonusDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies an item equip bonus entry directly to the game.
    /// </summary>
    /// <param name="modId">Mod that made the change.</param>
    /// <param name="itemEquipBonus"></param>
    public void ApplyTablePatch(string modId, ItemEquipBonus itemEquipBonus);

    /// <summary>
    /// Gets an item equip bonus entry before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ItemEquipBonus GetOriginalItemEquipBonus(int index);

    /// <summary>
    /// Gets an item equip bonus entry with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ItemEquipBonus GetItemEquipBonus(int index);
}
