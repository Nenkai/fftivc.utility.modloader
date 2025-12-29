using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Job command/skill set data manager.
/// </summary>
public interface IFFTOJobDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies a job directly to the game.
    /// </summary>
    /// <param name="modId">Mod that made the change.</param>
    /// <param name="job"></param>
    public void ApplyTablePatch(string modId, Job job);

    /// <summary>
    /// Gets a job before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Job GetOriginalJob(int index);

    /// <summary>
    /// Gets a job with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Job GetJob(int index);
}
