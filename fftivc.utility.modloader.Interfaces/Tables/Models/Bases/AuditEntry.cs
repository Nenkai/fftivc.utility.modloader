using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces.Tables.Models.Bases;

/// <summary>
/// Represents a model change made by a mod.
/// </summary>
public class AuditEntry
{
    /// <summary>
    /// Mod id that made this change.
    /// </summary>
    public required string ModIdOwner { get; set; }

    /// <summary>
    /// Change made.
    /// </summary>
    public required ModelDiff Difference { get; set; }
}
