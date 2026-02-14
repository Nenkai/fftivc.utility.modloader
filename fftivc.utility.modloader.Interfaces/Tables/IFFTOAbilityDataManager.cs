using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Ability data manager.
/// </summary>
public interface IFFTOAbilityDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies an ability directly to the game.
    /// </summary>
    /// <param name="modId">Mod that made the change.</param>
    /// <param name="ability"></param>
    public void ApplyTablePatch(string modId, Ability ability);

    /// <summary>
    /// Gets an ability, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Ability GetOriginalAbility(int index);

    /// <summary>
    /// Gets an ability with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Ability GetAbility(int index);
}
