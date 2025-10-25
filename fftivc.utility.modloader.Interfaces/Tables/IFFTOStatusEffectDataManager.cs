using fftivc.utility.modloader.Interfaces.Tables.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Status data manager.
/// </summary>
public interface IFFTOStatusEffectDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies a status directly to the game.
    /// </summary>
    /// <param name="modId">Mod that made the change.</param>
    /// <param name="status"></param>
    public void ApplyTablePatch(string modId, StatusEffect status);

    /// <summary>
    /// Gets an ability, before any modded changes were applied.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public StatusEffect GetOriginalStatusEffect(int index);

    /// <summary>
    /// Gets an ability with currently modded changes.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public StatusEffect GetStatusEffect(int index);
}
