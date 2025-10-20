using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;

namespace fftivc.utility.modloader.Interfaces.Tables;

/// <summary>
/// Table manager for data hardcoded in the executable.
/// </summary>
public interface IFFTOTableManager
{
    /// <summary>
    /// Gets the list of all changed entries in the table.
    /// </summary>
    IReadOnlyDictionary<(int Id, string PropertyName), AuditEntry> ChangedProperties { get; }

    /// <summary>
    /// Initializes the table manager.
    /// </summary>
    public void Init();

    /// <summary>
    /// Registers any potential files to be applied by the table manager, from the specified folder.
    /// </summary>
    /// <param name="modId">Mod id that owns the folder.</param>
    /// <param name="folder">Source folder.</param>
    public void RegisterFolder(string modId, string folder);

    /// <summary>
    /// (For mod loader use only) Applies pending file changes, when all mods have been loaded.
    /// </summary>
    public void ApplyPendingFileChanges();
}
