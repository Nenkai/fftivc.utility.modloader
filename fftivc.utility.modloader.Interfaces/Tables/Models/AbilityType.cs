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

    public byte? Animation1 { get; set; }
    public byte? Animation2 { get; set; }
    public byte? Animation3 { get; set; }

    public static Dictionary<string, DiffablePropertyItem<AbilityType>> PropertyMap { get; } = new()
    {
        [nameof(Animation1)] = new DiffablePropertyItem<AbilityType, byte?>(nameof(Animation1), i => i.Animation1, (i, v) => i.Animation1 = v),
        [nameof(Animation2)] = new DiffablePropertyItem<AbilityType, byte?>(nameof(Animation2), i => i.Animation2, (i, v) => i.Animation2 = v),
        [nameof(Animation3)] = new DiffablePropertyItem<AbilityType, byte?>(nameof(Animation3), i => i.Animation3, (i, v) => i.Animation3 = v),
    };

    public static AbilityType FromStructure(int id, ref ABILITY_TYPE_DATA @struct)
    {
        var animationSequence = new AbilityType()
        {
            Id = id,
            Animation1 = @struct.Animation1,
            Animation2 = @struct.Animation2,
            Animation3 = @struct.Animation3,
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
            Animation1 = Animation1,
            Animation2 = Animation2,
            Animation3 = Animation3
        };
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member