using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces.Tables.Models.Bases;

/// <summary>
/// Represents a property difference for a model.
/// </summary>
public class ModelDiff
{
    /// <summary>
    /// Property name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// New value.
    /// </summary>
    public object NewValue { get; set; }

    /// <summary>
    /// Constructor for ModelDiff
    /// </summary>
    /// <param name="name"></param>
    /// <param name="newValue"></param>
    public ModelDiff(string name, object newValue)
    {
        Name = name;
        NewValue = newValue;
    }
}
