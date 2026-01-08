using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Treasure hunting item data manager.
/// </summary>
public interface IFFTOMapItemDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies a map item entry directly to the game.
    /// </summary>
    /// <param name="modId">Mod id that is changing the item.</param>
    /// <param name="item"></param>
    public void ApplyTablePatch(string modId, MapTrapFormation item);

    /// <summary>
    /// Gets a map item entry, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public MapTrapFormation GetOriginalMapTrapFormation(int index);

    /// <summary>
    /// Gets a map item entry with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public MapTrapFormation GetMapTrapFormation(int index);
}
