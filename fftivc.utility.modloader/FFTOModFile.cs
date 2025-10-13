using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using fftivc.utility.modloader.Interfaces;

namespace fftivc.utility.modloader;

public class FFTOModFile : IFFTOModFile
{
    /// <summary>
    /// Game type for which this file belongs to.
    /// </summary>
    public required FFTOGameMode GameMode { get; set; }

    /// <summary>
    /// Mod id that owns this modded file.
    /// </summary>
    public required string ModIdOwner { get; set; }

    /// <summary>
    /// Game path.
    /// </summary>
    public required string GamePath { get; set; }

    /// <summary>
    /// Local path.
    /// </summary>
    public required string LocalPath { get; set; }
}
