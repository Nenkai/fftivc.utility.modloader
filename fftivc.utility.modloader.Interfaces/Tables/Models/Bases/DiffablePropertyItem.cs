using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces.Tables.Models.Bases;

/// <summary>
/// Represents a property for a model that can be diffed.
/// </summary>
/// <typeparam name="TModel"></typeparam>
public abstract record DiffablePropertyItem<TModel>
{
    protected DiffablePropertyItem(string name) => Name = name;

    /// <summary>
    /// Property name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Whether the model has changed.
    /// </summary>
    /// <param name="original"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public abstract bool HasChanged(TModel original, TModel other);

    /// <summary>
    /// Creates a model difference.
    /// </summary>
    /// <param name="original"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public abstract ModelDiff CreateDiff(TModel original, TModel other);

    /// <summary>
    /// Applies a model property difference.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="diff"></param>
    public abstract void Apply(TModel model, ModelDiff diff);
}

/// <summary>
/// Represents a property item with a value that can be diffed.
/// </summary>
/// <typeparam name="TModel">Model type.</typeparam>
/// <typeparam name="TValue">Property type.</typeparam>
/// <param name="Name">Name of the property.</param>
/// <param name="Getter">Getter, for getting the property value.</param>
/// <param name="Setter">Setter, for setting the property value.</param>
public sealed record DiffablePropertyItem<TModel, TValue>(string Name, Func<TModel, TValue?> Getter, Action<TModel, TValue> Setter) 
    : DiffablePropertyItem<TModel>(Name)
{
    /// <inheritdoc/>
    public override bool HasChanged(TModel original, TModel other)
    {
        TValue? originalPropValue = Getter(original);
        TValue? otherPropValue = Getter(other);

        // If it's null, it hasn't changed, it simply hasn't been set.
        if (otherPropValue is null)
            return false;

        return !EqualityComparer<TValue>.Default.Equals(originalPropValue, otherPropValue);
    }

    /// <inheritdoc/>
    public override ModelDiff CreateDiff(TModel original, TModel other)
        => new ModelDiff(Name, Getter(other));

    /// <inheritdoc/>
    public override void Apply(TModel model, ModelDiff diff) =>
        Setter(model, (TValue)diff.NewValue);
}
