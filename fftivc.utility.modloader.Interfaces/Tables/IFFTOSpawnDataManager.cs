using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Spawn data manager.
/// </summary>
public interface IFFTOSpawnDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies spawn data directly to the game.
    /// </summary>
    /// <param name="modId">Mod that made the change.</param>
    /// <param name="spawn"></param>
    public void ApplyTablePatch(string modId, Spawn spawn);

    /// <summary>
    /// Gets spawn data, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Spawn GetOriginalSpawn(int index);

    /// <summary>
    /// Gets spawn data with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Spawn GetSpawn(int index);
}
