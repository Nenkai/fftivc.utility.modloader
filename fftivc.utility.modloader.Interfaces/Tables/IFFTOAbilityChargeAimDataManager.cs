using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Aim ability secondary data manager.
/// </summary>
public interface IFFTOAbilityChargeAimDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies an aim ability's secondary data entry directly to the game.
    /// </summary>
    /// <param name="modId">Mod id that is changing the item.</param>
    /// <param name="item"></param>
    public void ApplyTablePatch(string modId, AbilityChargeAim item);

    /// <summary>
    /// Gets an aim ability's secondary data entry, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    public AbilityChargeAim GetOriginalChargeAimAbility(int index);

    /// <summary>
    /// Gets an aim ability's secondary data entry with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    public AbilityChargeAim GetChargeAimAbility(int index);
}
