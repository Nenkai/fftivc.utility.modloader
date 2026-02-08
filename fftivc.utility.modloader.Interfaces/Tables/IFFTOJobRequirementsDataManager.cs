using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Job requirements data manager.
/// </summary>
public interface IFFTOJobRequirementsDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies a job's requirements directly to the game.
    /// </summary>
    /// <param name="modId">Mod that made the change.</param>
    /// <param name="jobRequirement"></param>
    public void ApplyTablePatch(string modId, JobRequirements jobRequirement);

    /// <summary>
    /// Gets a job's requirements before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public JobRequirements GetOriginalJobRequirements(int index);

    /// <summary>
    /// Gets a job's requirements with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public JobRequirements GetJobRequirements(int index);
}
