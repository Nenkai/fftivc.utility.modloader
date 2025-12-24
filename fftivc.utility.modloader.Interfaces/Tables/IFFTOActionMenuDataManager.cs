using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// ActionMenu data manager.
/// </summary>
public interface IFFTOActionMenuDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies an action menu directly to the game.
    /// </summary>
    /// <param name="modId">Mod that made the change.</param>
    /// <param name="actionMenu"></param>
    public void ApplyTablePatch(string modId, ActionMenu actionMenu);

    /// <summary>
    /// Gets an action menu, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ActionMenu GetOriginalActionMenu(int index);

    /// <summary>
    /// Gets an action menu with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ActionMenu GetActionMenu(int index);
}
