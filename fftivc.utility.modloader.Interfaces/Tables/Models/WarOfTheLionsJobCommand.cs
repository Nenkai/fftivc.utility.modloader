using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// War of the Lions-exclusive job command table (Darkness/Piracy/Huntcraft, ids 224-226).
/// These three skillsets exist in vanilla Job/JobData (Dark Knight's JobCommandId is 224,
/// confirmed in JobData.xml), but sit outside the range of the existing JobCommandData (0-175)
/// and MonsterJobCommandData (176-223) hardcoded tables.
///
/// Reuses the exact same JobCommand model/JOB_COMMAND_DATA struct as JobCommandData, since ids
/// 224-226 use the identical 16-action + 6-RSM layout. See FFTOWarOfTheLionsJobCommandDataManager
/// for how the table's address in memory is located.
/// </summary>
public class WarOfTheLionsJobCommandTable : TableBase<JobCommand>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}
