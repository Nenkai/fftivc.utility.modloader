using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Weapon Secondary 
/// </summary>
public class WeaponSecondaryTable : TableBase<WeaponSecondary>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

/// <summary>
/// Shield Secondary 
/// </summary>
public class ShieldSecondaryTable : TableBase<ShieldSecondary>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

/// <summary>
/// Head/Body Secondary 
/// </summary>
public class HeadBodySecondaryTable : TableBase<HeadBodySecondary>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

/// <summary>
/// Accessory Secondary 
/// </summary>
public class AccessorySecondaryTable : TableBase<AccessorySecondary>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

/// <summary>
/// Chemist Item Secondary 
/// </summary>
public class ChemistItemSecondaryTable : TableBase<ChemistItemSecondary>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Weapon Secondary Data. <see href="https://ffhacktics.com/wiki/Weapon_Secondary_Data"/>
/// </summary>
public class WeaponSecondary : DiffableModelBase<WeaponSecondary>, IDiffableModel<WeaponSecondary>, IIdentifiableModel
{
    public int Id { get; set; }

    public byte? Range { get; set; }
    public WeaponAttackFlags? AttackFlags { get; set; }
    public byte? Formula { get; set; } // See https://ffhacktics.com/wiki/Formulas
    public byte? Unused_0x03 { get; set; }
    public byte? Power { get; set; }
    public byte? Evasion { get; set; }
    public WeaponElementFlags? Elements { get; set; }

    /// <summary>
    /// ItemOptionsId or AbilityId if Formula is something like 0x02
    /// </summary>
    public byte? OptionsAbilityId { get; set; }

    public static Dictionary<string, DiffablePropertyItem<WeaponSecondary>> PropertyMap { get; } = new()
    {
        [nameof(Range)] = new DiffablePropertyItem<WeaponSecondary, byte?>(nameof(Range), i => i.Range, (i, v) => i.Range = v),
        [nameof(AttackFlags)] = new DiffablePropertyItem<WeaponSecondary, WeaponAttackFlags?>(nameof(AttackFlags), i => i.AttackFlags, (i, v) => i.AttackFlags = v),
        [nameof(Formula)] = new DiffablePropertyItem<WeaponSecondary, byte?>(nameof(Formula), i => i.Formula, (i, v) => i.Formula = v),
        [nameof(Unused_0x03)] = new DiffablePropertyItem<WeaponSecondary, byte?>(nameof(Unused_0x03), i => i.Unused_0x03, (i, v) => i.Unused_0x03 = v),
        [nameof(Power)] = new DiffablePropertyItem<WeaponSecondary, byte?>(nameof(Power), i => i.Power, (i, v) => i.Power = v),
        [nameof(Evasion)] = new DiffablePropertyItem<WeaponSecondary, byte?>(nameof(Evasion), i => i.Evasion, (i, v) => i.Evasion = v),
        [nameof(Elements)] = new DiffablePropertyItem<WeaponSecondary, WeaponElementFlags?>(nameof(Elements), i => i.Elements, (i, v) => i.Elements = v),
        [nameof(OptionsAbilityId)] = new DiffablePropertyItem<WeaponSecondary, byte?>(nameof(OptionsAbilityId), i => i.OptionsAbilityId, (i, v) => i.OptionsAbilityId = v),
    };

    public static WeaponSecondary FromStructure(int id, ref WEAPON_SECONDARY_DATA @struct)
    {
        var data = new WeaponSecondary()
        {
            Id = id,
            Range = @struct.Range,
            AttackFlags = @struct.AttackFlags,
            Formula = @struct.Formula,
            Unused_0x03 = @struct.Unused_0x03,
            Power = @struct.Power,
            Evasion = @struct.Evasion,
            Elements = @struct.Elements,
            OptionsAbilityId = @struct.StatusEffectIdOrAbilityId
        };

        return data;
    }

    /// <summary>
    /// Clones the item.
    /// </summary>
    /// <returns></returns>
    public WeaponSecondary Clone()
    {
        return new WeaponSecondary()
        {
            Id = Id,
            Range = Range,
            AttackFlags = AttackFlags,
            Formula = Formula,
            Unused_0x03 = Unused_0x03,
            Power = Power,
            Evasion = Evasion,
            Elements = Elements,
            OptionsAbilityId = OptionsAbilityId
        };
    }
}

/// <summary>
/// Shield Secondary Data. <see href="https://ffhacktics.com/wiki/Shield_Secondary_Data"/>
/// </summary>
public class ShieldSecondary : DiffableModelBase<ShieldSecondary>, IDiffableModel<ShieldSecondary>, IIdentifiableModel
{
    public int Id { get; set; }

    public byte? PhysicalEvasion { get; set; }
    public byte? MagicalEvasion { get; set; }

    public static Dictionary<string, DiffablePropertyItem<ShieldSecondary>> PropertyMap { get; } = new()
    {
        [nameof(PhysicalEvasion)] = new DiffablePropertyItem<ShieldSecondary, byte?>(nameof(PhysicalEvasion), i => i.PhysicalEvasion, (i, v) => i.PhysicalEvasion = v),
        [nameof(MagicalEvasion)] = new DiffablePropertyItem<ShieldSecondary, byte?>(nameof(MagicalEvasion), i => i.MagicalEvasion, (i, v) => i.MagicalEvasion = v),
    };

    public static ShieldSecondary FromStructure(int id, ref SHIELD_SECONDARY_DATA @struct)
    {
        var data = new ShieldSecondary()
        {
            Id = id,
            PhysicalEvasion = @struct.PhysicalEvasion,
            MagicalEvasion = @struct.MagicalEvasion,
        };

        return data;
    }

    /// <summary>
    /// Clones the item.
    /// </summary>
    /// <returns></returns>
    public ShieldSecondary Clone()
    {
        return new ShieldSecondary()
        {
            Id = Id,
            PhysicalEvasion = PhysicalEvasion,
            MagicalEvasion = MagicalEvasion
        };
    }
}

/// <summary>
/// Head/Body Secondary Data. <see href="https://ffhacktics.com/wiki/Helm/Armor_Secondary_Data"/>
/// </summary>
public class HeadBodySecondary : DiffableModelBase<HeadBodySecondary>, IDiffableModel<HeadBodySecondary>, IIdentifiableModel
{
    public int Id { get; set; }

    public byte? HPBonus { get; set; }
    public byte? MPBonus { get; set; }

    public static Dictionary<string, DiffablePropertyItem<HeadBodySecondary>> PropertyMap { get; } = new()
    {
        [nameof(HPBonus)] = new DiffablePropertyItem<HeadBodySecondary, byte?>(nameof(HPBonus), i => i.HPBonus, (i, v) => i.HPBonus = v),
        [nameof(MPBonus)] = new DiffablePropertyItem<HeadBodySecondary, byte?>(nameof(MPBonus), i => i.MPBonus, (i, v) => i.MPBonus = v),
    };

    public static HeadBodySecondary FromStructure(int id, ref HEAD_BODY_SECONDARY_DATA @struct)
    {
        var data = new HeadBodySecondary()
        {
            Id = id,
            HPBonus = @struct.HPBonus,
            MPBonus = @struct.MPBonus,
        };

        return data;
    }

    /// <summary>
    /// Clones the item.
    /// </summary>
    /// <returns></returns>
    public HeadBodySecondary Clone()
    {
        return new HeadBodySecondary()
        {
            Id = Id,
            HPBonus = HPBonus,
            MPBonus = MPBonus
        };
    }
}

/// <summary>
/// Accessory Secondary Data. <see href="https://ffhacktics.com/wiki/Accessory_Secondary_Data"/>
/// </summary>
public class AccessorySecondary : DiffableModelBase<AccessorySecondary>, IDiffableModel<AccessorySecondary>, IIdentifiableModel
{
    public int Id { get; set; }

    public byte? PhysicalEvasion { get; set; }
    public byte? MagicalEvasion { get; set; }

    public static Dictionary<string, DiffablePropertyItem<AccessorySecondary>> PropertyMap { get; } = new()
    {
        [nameof(PhysicalEvasion)] = new DiffablePropertyItem<AccessorySecondary, byte?>(nameof(PhysicalEvasion), i => i.PhysicalEvasion, (i, v) => i.PhysicalEvasion = v),
        [nameof(MagicalEvasion)] = new DiffablePropertyItem<AccessorySecondary, byte?>(nameof(MagicalEvasion), i => i.MagicalEvasion, (i, v) => i.MagicalEvasion = v),
    };

    public static AccessorySecondary FromStructure(int id, ref ACCESSORY_SECONDARY_DATA @struct)
    {
        var data = new AccessorySecondary()
        {
            Id = id,
            PhysicalEvasion = @struct.PhysicalEvasion,
            MagicalEvasion = @struct.MagicalEvasion,
        };

        return data;
    }

    /// <summary>
    /// Clones the item.
    /// </summary>
    /// <returns></returns>
    public AccessorySecondary Clone()
    {
        return new AccessorySecondary()
        {
            Id = Id,
            PhysicalEvasion = PhysicalEvasion,
            MagicalEvasion = MagicalEvasion
        };
    }
}

/// <summary>
/// Chemist Item Secondary Data. <see href="https://ffhacktics.com/wiki/Item_Secondary_Data"/>
/// </summary>
public class ChemistItemSecondary : DiffableModelBase<ChemistItemSecondary>, IDiffableModel<ChemistItemSecondary>, IIdentifiableModel
{
    public int Id { get; set; }

    public byte? Formula { get; set; }
    public byte? Z { get; set; }
    public byte? StatusEffectId { get; set; }

    public static Dictionary<string, DiffablePropertyItem<ChemistItemSecondary>> PropertyMap { get; } = new()
    {
        [nameof(Formula)] = new DiffablePropertyItem<ChemistItemSecondary, byte?>(nameof(Formula), i => i.Formula, (i, v) => i.Formula = v),
        [nameof(Z)] = new DiffablePropertyItem<ChemistItemSecondary, byte?>(nameof(Z), i => i.Z, (i, v) => i.Z = v),
        [nameof(StatusEffectId)] = new DiffablePropertyItem<ChemistItemSecondary, byte?>(nameof(StatusEffectId), i => i.StatusEffectId, (i, v) => i.StatusEffectId = v)
    };

    public static ChemistItemSecondary FromStructure(int id, ref CHEMIST_ITEM_SECONDARY_DATA @struct)
    {
        var data = new ChemistItemSecondary()
        {
            Id = id,
            Formula = @struct.Formula,
            Z = @struct.Z,
            StatusEffectId = @struct.StatusEffectId,
        };

        return data;
    }

    /// <summary>
    /// Clones the item.
    /// </summary>
    /// <returns></returns>
    public ChemistItemSecondary Clone()
    {
        return new ChemistItemSecondary()
        {
            Id = Id,
            Formula = Formula,
            Z = Z,
            StatusEffectId = StatusEffectId
        };
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
