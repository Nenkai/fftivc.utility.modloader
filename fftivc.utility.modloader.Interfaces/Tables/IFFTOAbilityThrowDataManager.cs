using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Throw ability secondary data manager.
/// </summary>
public interface IFFTOAbilityThrowDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies a throw ability's secondary data entry directly to the game.
    /// </summary>
    /// <param name="modId">Mod id that is changing the item.</param>
    /// <param name="item"></param>
    public void ApplyTablePatch(string modId, AbilityThrow item);

    /// <summary>
    /// Gets a throw ability's secondary data entry, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    public AbilityThrow GetOriginalThrowAbility(int index);

    /// <summary>
    /// Gets a throw ability's secondary data entry with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    public AbilityThrow GetThrowAbility(int index);
}
