using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Aim ability secondary data
/// </summary>
public class AbilityAimTable : TableBase<AbilityAim>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Aim ability secondary data. <see href="https://ffhacktics.com/wiki/Ability_Secondary_Data:_Charge_Abilities"/>
/// </summary>
public class AbilityAim : DiffableModelBase<AbilityAim>, IDiffableModel<AbilityAim>, IIdentifiableModel
{
    public int Id { get; set; }

    public byte? Ticks { get; set; }
    public byte? Power { get; set; }

    public static Dictionary<string, DiffablePropertyItem<AbilityAim>> PropertyMap { get; } = new()
    {
        [nameof(Ticks)] = new DiffablePropertyItem<AbilityAim, byte?>(nameof(Ticks), i => i.Ticks, (i, v) => i.Ticks = v),
        [nameof(Power)] = new DiffablePropertyItem<AbilityAim, byte?>(nameof(Power), i => i.Power, (i, v) => i.Power = v),
    };

    public static AbilityAim FromStructure(int id, ref ABILITY_AIM_DATA @struct)
    {
        var data = new AbilityAim()
        {
            Id = id,
            Ticks = @struct.Ticks,
            Power = @struct.Power,
        };

        return data;
    }

    /// <summary>
    /// Clones the ability data.
    /// </summary>
    public AbilityAim Clone()
    {
        return new AbilityAim()
        {
            Id = Id,
            Ticks = Ticks,
            Power = Power,
        };
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
