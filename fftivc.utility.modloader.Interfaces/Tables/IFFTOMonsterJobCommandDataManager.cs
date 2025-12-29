using fftivc.utility.modloader.Interfaces.Tables.Models;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Monster Job command/skill set data manager.
/// </summary>
public interface IFFTOMonsterJobCommandDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies a monster job command/skill set directly to the game.
    /// </summary>
    /// <param name="modId">Mod that made the change.</param>
    /// <param name="monsterJobCommand"></param>
    public void ApplyTablePatch(string modId, MonsterJobCommand monsterJobCommand);

    /// <summary>
    /// Gets a monster job command/skill set, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public MonsterJobCommand GetOriginalMonsterJobCommand(int index);

    /// <summary>
    /// Gets a monster job command/skill set with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public MonsterJobCommand GetMonsterJobCommand(int index);
}
