using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces;

/// <summary>
/// Running game mode for FFTO.
/// </summary>
public enum FFTOGameMode
{
    /// <summary>
    /// Classic mode.
    /// </summary>
    Classic,

    /// <summary>
    /// Enhanced mode.
    /// </summary>
    Enhanced,

    /// <summary>
    /// Both.
    /// </summary>
    Combined,
}
