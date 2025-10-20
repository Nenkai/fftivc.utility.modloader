using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces.Tables.Models.Bases;

/// <summary>
/// Model that can be diffed against others.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IDiffableModel<T>
{
    /// <summary>
    /// List of properties defined for the model. <b>Property names should never be renamed, or removed!</b>
    /// </summary>
    static abstract Dictionary<string, DiffablePropertyItem<T>> PropertyMap { get; }

    /// <summary>
    /// Compares this model to the specified one and returns changes.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    IList<ModelDiff> DiffModel(T other);

    /// <summary>
    /// Applies a list of changes.
    /// </summary>
    /// <param name="diffs"></param>
    void ApplyChanges(IEnumerable<ModelDiff> diffs);

    /// <summary>
    /// Applies a change.
    /// </summary>
    /// <param name="diff"></param>
    void ApplyChange(ModelDiff diff);
}
