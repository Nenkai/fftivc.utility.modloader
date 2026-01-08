using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Ability effect table. <see href="https://ffhacktics.com/wiki/Effects"/>
/// </summary>
public class AbilityEffectNumberFilterTable : TableBase<AbilityEffectNumberFilter>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Ability effect.
/// </summary>
public class AbilityEffectNumberFilter : DiffableModelBase<AbilityEffectNumberFilter>, IDiffableModel<AbilityEffectNumberFilter>, IIdentifiableModel
{
    /// <summary>
    /// Id. No more than 453 in vanilla.
    /// </summary>
    public int Id { get; set; }

    public short? EffectId { get; set; }

    public static Dictionary<string, DiffablePropertyItem<AbilityEffectNumberFilter>> PropertyMap { get; } = new()
    {
        [nameof(EffectId)] = new DiffablePropertyItem<AbilityEffectNumberFilter, short?>(nameof(EffectId), i => i.EffectId, (i, v) => i.EffectId = v),
    };

    public static AbilityEffectNumberFilter FromStructure(int id, ref ABILITY_EFFECT_NUMBER_FILTER_DATA @struct)
    {
        var abilityEffect = new AbilityEffectNumberFilter()
        {
            Id = id,
            EffectId = @struct.EffectId,
        };

        return abilityEffect;
    }

    /// <summary>
    /// Clones the ability effect.
    /// </summary>
    /// <returns></returns>
    public AbilityEffectNumberFilter Clone()
    {
        return new AbilityEffectNumberFilter()
        {
            Id = Id,
            EffectId = EffectId,
        };
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member