using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Default ability secondary data
/// </summary>
public class AbilitySecondaryTable : TableBase<AbilitySecondary>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

/// <summary>
/// Default ability secondary data. <see href="https://ffhacktics.com/wiki/Ability_Secondary_Data:_Default_Abilities"/>
/// </summary>
/// 

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class AbilitySecondary : DiffableModelBase<AbilitySecondary>, IDiffableModel<AbilitySecondary>, IIdentifiableModel
{
    public int Id { get; set; }
    public byte? Range { get; set; }
    public byte? EffectArea { get; set; }
    public byte? Vertical { get; set; }
    public AbilitySecondaryFlags? Flags { get; set; }
    public AbilityElement? Element { get; set; }
    public byte? Formula { get; set; }
    public byte? X { get; set; }
    public byte? Y { get; set; }
    public byte? InflictStatus { get; set; }
    public byte? CT { get; set; }
    public byte? MPCost { get; set; }

    public static Dictionary<string, DiffablePropertyItem<AbilitySecondary>> PropertyMap { get; } = new()
    {
        [nameof(Range)] = new DiffablePropertyItem<AbilitySecondary, byte?>(nameof(Range), i => i.Range, (i, v) => i.Range = v),
        [nameof(EffectArea)] = new DiffablePropertyItem<AbilitySecondary, byte?>(nameof(EffectArea), i => i.EffectArea, (i, v) => i.EffectArea = v),
        [nameof(Vertical)] = new DiffablePropertyItem<AbilitySecondary, byte?>(nameof(Vertical), i => i.Vertical, (i, v) => i.Vertical = v),
        [nameof(Flags)] = new DiffablePropertyItem<AbilitySecondary, AbilitySecondaryFlags?>(nameof(Flags), i => i.Flags, (i, v) => i.Flags = v),
        [nameof(Element)] = new DiffablePropertyItem<AbilitySecondary, AbilityElement?>(nameof(Element), i => i.Element, (i, v) => i.Element = v),
        [nameof(Formula)] = new DiffablePropertyItem<AbilitySecondary, byte?>(nameof(Formula), i => i.Formula, (i, v) => i.Formula = v),
        [nameof(X)] = new DiffablePropertyItem<AbilitySecondary, byte?>(nameof(X), i => i.X, (i, v) => i.X = v),
        [nameof(Y)] = new DiffablePropertyItem<AbilitySecondary, byte?>(nameof(Y), i => i.Y, (i, v) => i.Y = v),
        [nameof(InflictStatus)] = new DiffablePropertyItem<AbilitySecondary, byte?>(nameof(InflictStatus), i => i.InflictStatus, (i, v) => i.InflictStatus = v),
        [nameof(CT)] = new DiffablePropertyItem<AbilitySecondary, byte?>(nameof(CT), i => i.CT, (i, v) => i.CT = v),
        [nameof(MPCost)] = new DiffablePropertyItem<AbilitySecondary, byte?>(nameof(MPCost), i => i.MPCost, (i, v) => i.MPCost = v),
    };

    public static AbilitySecondary FromStructure(int id, ref ABILITY_SECONDARY_DATA @struct)
    {
        var data = new AbilitySecondary()
        {
            Id = id,
            Range = @struct.Range,
            EffectArea = @struct.EffectArea,
            Vertical = @struct.Vertical,
            Flags = @struct.Flags,
            Element = @struct.Element,
            Formula = @struct.Formula,
            X = @struct.X,
            Y = @struct.Y,
            InflictStatus = @struct.InflictStatus,
            CT = @struct.CT,
            MPCost = @struct.MPCost,
        };

        return data;
    }

    /// <summary>
    /// Clones the ability secondary data.
    /// </summary>
    public AbilitySecondary Clone()
    {
        return new AbilitySecondary()
        {
            Id = Id,
            Range = Range,
            Vertical = Vertical,
            Flags = Flags,
            Element = Element,
            Formula = Formula,
            X = X, 
            Y = Y,
            InflictStatus = InflictStatus,
            CT = CT,
            MPCost = MPCost,
        };
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member