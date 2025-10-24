using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces.Tables.Models.Bases;

/// <summary>
/// Represents a model that can be identified.
/// </summary>
public interface IIdentifiableModel
{
    /// <summary>
    /// Id for this model.
    /// </summary>
    int Id { get; set; }
}
