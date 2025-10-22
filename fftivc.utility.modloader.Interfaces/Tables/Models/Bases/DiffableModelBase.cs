using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces.Tables.Models.Bases;

/// <summary>
/// Base diffable model.
/// </summary>
/// <typeparam name="TModel"></typeparam>
public abstract class DiffableModelBase<TModel>
    where TModel : DiffableModelBase<TModel>, IDiffableModel<TModel>
{
    /// <summary>
    /// Compares this model to the specified one and returns changes.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public IList<ModelDiff> DiffModel(TModel other)
    {
        var list = new List<ModelDiff>();
        foreach (var propItem in TModel.PropertyMap)
        {
            if (propItem.Value.HasChanged((TModel)this, other))
                list.Add(propItem.Value.CreateDiff((TModel)this, other));
        }

        return list;
    }

    /// <summary>
    /// Applies a list of changes.
    /// </summary>
    /// <param name="diffs"></param>
    public void ApplyChanges(IEnumerable<ModelDiff> diffs)
    {
        foreach (var diff in diffs)
            ApplyChange(diff);
    }

    /// <summary>
    /// Applies a change.
    /// </summary>
    /// <param name="diff"></param>
    public void ApplyChange(ModelDiff diff)
    {
        if (TModel.PropertyMap.TryGetValue(diff.Name, out DiffablePropertyItem<TModel>? prop))
            prop?.Apply((TModel)this, diff);
    }
}
