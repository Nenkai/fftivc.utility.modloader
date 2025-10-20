using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces.Tables.Models.Bases;

/// <summary>
/// Represents a model that can be versioned.
/// </summary>
public interface IVersionableModel
{
    /// <summary>
    /// Version for this model.
    /// </summary>
    uint Version { get; set; }
}
