using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Status data manager.
/// </summary>
public interface IFFTOInflictStatusDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies an "inflict status" entry directly to the game.
    /// </summary>
    /// <param name="modId">Mod that made the change.</param>
    /// <param name="status"></param>
    public void ApplyTablePatch(string modId, InflictStatus status);

    /// <summary>
    /// Gets an ability, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public InflictStatus GetOriginalInflictStatus(int index);

    /// <summary>
    /// Gets an ability with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public InflictStatus GetInflictStatus(int index);
}
