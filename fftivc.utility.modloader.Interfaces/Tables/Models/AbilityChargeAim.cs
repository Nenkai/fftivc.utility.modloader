using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Aim ability secondary data
/// </summary>
public class AbilityChargeAimTable : TableBase<AbilityChargeAim>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Aim ability secondary data. <see href="https://ffhacktics.com/wiki/Ability_Secondary_Data:_Charge_Abilities"/>
/// </summary>
public class AbilityChargeAim : DiffableModelBase<AbilityChargeAim>, IDiffableModel<AbilityChargeAim>, IIdentifiableModel
{
    public int Id { get; set; }

    public byte? Ticks { get; set; }
    public byte? Power { get; set; }

    public static Dictionary<string, DiffablePropertyItem<AbilityChargeAim>> PropertyMap { get; } = new()
    {
        [nameof(Ticks)] = new DiffablePropertyItem<AbilityChargeAim, byte?>(nameof(Ticks), i => i.Ticks, (i, v) => i.Ticks = v),
        [nameof(Power)] = new DiffablePropertyItem<AbilityChargeAim, byte?>(nameof(Power), i => i.Power, (i, v) => i.Power = v),
    };

    public static AbilityChargeAim FromStructure(int id, ref ABILITY_CHARGE_AIM_DATA @struct)
    {
        var data = new AbilityChargeAim()
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
    public AbilityChargeAim Clone()
    {
        return new AbilityChargeAim()
        {
            Id = Id,
            Ticks = Ticks,
            Power = Power,
        };
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
