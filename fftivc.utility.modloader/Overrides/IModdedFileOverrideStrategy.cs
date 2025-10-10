using fftivc.utility.modloader.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Overrides;

/// <summary>
/// Represents an override strategy for a modded file, such that it is overriden on the file system level through other means.
/// </summary>
public interface IModdedFileOverrideStrategy
{
    /// <summary>
    /// Initializes the overriding strategy.
    /// </summary>
    /// <param name="gameMode"></param>
    public void Initialize(FFTOGameMode gameMode);

    /// <summary>
    /// Returns whether the specified filename should is subject to this overriding strategy.
    /// </summary>
    /// <param name="gamePath"></param>
    /// <returns></returns>
    public bool Matches(string gamePath);

    /// <summary>
    /// Whether this strategy also allows the file to be inserted on the file system.
    /// </summary>
    /// <returns></returns>
    public bool ReplacesFileSystemFile();

    /// <summary>
    /// Applies an override strategy for the specified file.
    /// </summary>
    /// <param name="gameType">Game type for which this file applies to.</param>
    /// <param name="modId">Mod that owns this file.</param>
    /// <param name="gamePath">Game path.</param>
    /// <param name="localPath">Local path on disk.</param>
    public void Apply(FFTOGameMode gameType, string modId, string gamePath, string localPath);
}
