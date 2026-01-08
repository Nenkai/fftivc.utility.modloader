using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Ability type/animation sequence table. <see href="https://ffhacktics.com/wiki/Animations_(Tab)"/> <br/>
/// </summary>
public class AbilityTypeTable : TableBase<AbilityType>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Ability animation sequence.
/// </summary>
public class AbilityType : DiffableModelBase<AbilityType>, IDiffableModel<AbilityType>, IIdentifiableModel
{
    /// <summary>
    /// Id. No more than 453 in vanilla.
    /// </summary>
    public int Id { get; set; }

    public byte? ChargeEffectType { get; set; }
    public byte? AnimationId { get; set; }
    public byte? BattleTextId { get; set; }

    public static Dictionary<string, DiffablePropertyItem<AbilityType>> PropertyMap { get; } = new()
    {
        [nameof(ChargeEffectType)] = new DiffablePropertyItem<AbilityType, byte?>(nameof(ChargeEffectType), i => i.ChargeEffectType, (i, v) => i.ChargeEffectType = v),
        [nameof(AnimationId)] = new DiffablePropertyItem<AbilityType, byte?>(nameof(AnimationId), i => i.AnimationId, (i, v) => i.AnimationId = v),
        [nameof(BattleTextId)] = new DiffablePropertyItem<AbilityType, byte?>(nameof(BattleTextId), i => i.BattleTextId, (i, v) => i.BattleTextId = v),
    };

    public static AbilityType FromStructure(int id, ref ABILITY_TYPE_DATA @struct)
    {
        var animationSequence = new AbilityType()
        {
            Id = id,
            ChargeEffectType = @struct.ChargeEffectType,
            AnimationId = @struct.AnimationId,
            BattleTextId = @struct.BattleTextId,
        };

        return animationSequence;
    }

    /// <summary>
    /// Clones the ability animation sequence.
    /// </summary>
    /// <returns></returns>
    public AbilityType Clone()
    {
        return new AbilityType()
        {
            Id = Id,
            ChargeEffectType = ChargeEffectType,
            AnimationId = AnimationId,
            BattleTextId = BattleTextId
        };
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member