using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Ability effect table. <see href="https://ffhacktics.com/wiki/Effects"/>
/// </summary>
public class AbilityEffectTable : TableBase<AbilityEffect>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Ability effect.
/// </summary>
public class AbilityEffect : DiffableModelBase<AbilityEffect>, IDiffableModel<AbilityEffect>, IIdentifiableModel
{
    /// <summary>
    /// Id. No more than 453 in vanilla.
    /// </summary>
    public int Id { get; set; }

    public ushort? EffectId { get; set; }

    public static Dictionary<string, DiffablePropertyItem<AbilityEffect>> PropertyMap { get; } = new()
    {
        [nameof(EffectId)] = new DiffablePropertyItem<AbilityEffect, ushort?>(nameof(EffectId), i => i.EffectId, (i, v) => i.EffectId = v),
    };

    public static AbilityEffect FromStructure(int id, ref ABILITY_EFFECT_DATA @struct)
    {
        var abilityEffect = new AbilityEffect()
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
    public AbilityEffect Clone()
    {
        return new AbilityEffect()
        {
            Id = Id,
            EffectId = EffectId,
        };
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member