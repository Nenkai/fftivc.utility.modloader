using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Job need level data manager.
/// </summary>
public interface IFFTOJobNeedLevelDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies a job's need levels directly to the game.
    /// </summary>
    /// <param name="modId">Mod that made the change.</param>
    /// <param name="jobNeedLevels"></param>
    public void ApplyTablePatch(string modId, JobNeedLevel jobNeedLevels);

    /// <summary>
    /// Gets a job's need levels before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public JobNeedLevel GetOriginalJobNeedLevel(int index);

    /// <summary>
    /// Gets a job's need levels with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public JobNeedLevel GetJobNeedLevel(int index);
}
