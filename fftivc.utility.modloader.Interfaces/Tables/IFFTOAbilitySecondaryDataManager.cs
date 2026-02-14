using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Ability secondary data manager.
/// </summary>
public interface IFFTOAbilitySecondaryDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies an ability secondary directly to the game.
    /// </summary>
    /// <param name="modId">Mod that made the change.</param>
    /// <param name="abilitySecondary"></param>
    public void ApplyTablePatch(string modId, AbilitySecondary abilitySecondary);

    /// <summary>
    /// Gets an ability secondary, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public AbilitySecondary GetOriginalAbilitySecondary(int index);

    /// <summary>
    /// Gets an ability secondary with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public AbilitySecondary GetAbilitySecondary(int index);
}
