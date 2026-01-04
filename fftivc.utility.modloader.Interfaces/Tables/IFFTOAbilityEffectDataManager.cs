using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Ability effect data manager.
/// </summary>
public interface IFFTOAbilityEffectDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies an ability effect directly to the game.
    /// </summary>
    /// <param name="modId">Mod that made the change.</param>
    /// <param name="abilityEffect"></param>
    public void ApplyTablePatch(string modId, AbilityEffect abilityEffect);

    /// <summary>
    /// Gets an ability effect, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public AbilityEffect GetOriginalAbilityEffect(int index);

    /// <summary>
    /// Gets an ability effect with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public AbilityEffect GetAbilityEffect(int index);
}
