using fftivc.utility.modloader.Interfaces.Tables.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Ability data manager.
/// </summary>
public interface IFFTOAbilityDataManager : IFFTOTableManager
{
    /// <summary>
    /// Applies an ability directly to the game.
    /// </summary>
    /// <param name="ability"></param>
    public void ApplyChange(Ability ability);
}
