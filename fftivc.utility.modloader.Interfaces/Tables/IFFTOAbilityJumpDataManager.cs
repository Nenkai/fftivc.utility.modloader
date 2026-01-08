using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Jump ability secondary data manager.
/// </summary>
public interface IFFTOAbilityJumpDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies a jump ability's secondary data entry directly to the game.
    /// </summary>
    /// <param name="modId">Mod id that is changing the item.</param>
    /// <param name="item"></param>
    public void ApplyTablePatch(string modId, AbilityJump item);

    /// <summary>
    /// Gets a jump ability's secondary data entry, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    public AbilityJump GetOriginalJumpAbility(int index);

    /// <summary>
    /// Gets a jump ability's secondary data entry with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    public AbilityJump GetJumpAbility(int index);
}
