using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces;

/// <summary>
/// Represents a modded file.
/// </summary>
public interface IFFTOModFile
{
    /// <summary>
    /// Game type for which this file belongs to.
    /// </summary>
    public FFTOGameMode GameMode { get; set; }

    /// <summary>
    /// Mod id that owns this modded file.
    /// </summary>
    public string ModIdOwner { get; }

    /// <summary>
    /// Game path.
    /// </summary>
    public string GamePath { get; }

    /// <summary>
    /// Local path.
    /// </summary>
    public string LocalPath { get; }
}
