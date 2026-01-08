using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Ability type data manager.
/// </summary>
public interface IFFTOAbilityTypeDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies an ability type directly to the game.
    /// </summary>
    /// <param name="modId">Mod that made the change.</param>
    /// <param name="abilityAnimation"></param>
    public void ApplyTablePatch(string modId, AbilityType abilityAnimation);

    /// <summary>
    /// Gets an ability type, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public AbilityType GetOriginalAbilityType(int index);

    /// <summary>
    /// Gets an ability type with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public AbilityType GetAbilityType(int index);
}
