using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// CommandType data manager.
/// </summary>
public interface IFFTOCommandTypeDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies a command type directly to the game.
    /// </summary>
    /// <param name="modId">Mod that made the change.</param>
    /// <param name="commandType"></param>
    public void ApplyTablePatch(string modId, CommandType commandType);

    /// <summary>
    /// Gets a command type (action menu in FFTPatcher), before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public CommandType GetOriginalCommandType(int index);

    /// <summary>
    /// Get a command type (action menu in FFTPatcher) with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public CommandType GetCommandType(int index);
}
