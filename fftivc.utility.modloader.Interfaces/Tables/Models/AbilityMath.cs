using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Math ability secondary data
/// </summary>
public class AbilityMathTable : TableBase<AbilityMath>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Math ability secondary data. <see href="https://ffhacktics.com/wiki/Ability_Secondary_Data:_Math_Abilities"/>
/// </summary>
public class AbilityMath : DiffableModelBase<AbilityMath>, IDiffableModel<AbilityMath>, IIdentifiableModel
{
    public int Id { get; set; }

    public MathAbilityType? AbilityType { get; set; }

    public static Dictionary<string, DiffablePropertyItem<AbilityMath>> PropertyMap { get; } = new()
    {
        [nameof(AbilityType)] = new DiffablePropertyItem<AbilityMath, MathAbilityType?>(nameof(AbilityType), i => i.AbilityType, (i, v) => i.AbilityType = v),
    };

    public static AbilityMath FromStructure(int id, ref ABILITY_MATH_DATA @struct)
    {
        var data = new AbilityMath()
        {
            Id = id,
            AbilityType = @struct.AbilityType,
        };

        return data;
    }

    /// <summary>
    /// Clones the ability data.
    /// </summary>
    public AbilityMath Clone()
    {
        return new AbilityMath()
        {
            Id = Id,
            AbilityType = AbilityType,
        };
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
