using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Math ability secondary data manager.
/// </summary>
public interface IFFTOAbilityMathDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies a math ability's secondary data entry directly to the game.
    /// </summary>
    /// <param name="modId">Mod id that is changing the item.</param>
    /// <param name="item"></param>
    public void ApplyTablePatch(string modId, AbilityMath item);

    /// <summary>
    /// Gets a math ability's secondary data entry, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    public AbilityMath GetOriginalMathAbility(int index);

    /// <summary>
    /// Gets a math ability's secondary data entry with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    public AbilityMath GetMathAbility(int index);
}
