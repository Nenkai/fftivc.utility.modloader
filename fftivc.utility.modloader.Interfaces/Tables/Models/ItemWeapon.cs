using fftivc.utility.modloader.Interfaces.Tables.Models.Bases;
using fftivc.utility.modloader.Interfaces.Tables.Structures;

namespace fftivc.utility.modloader.Interfaces.Tables.Models;

/// <summary>
/// Weapon item secondary data
/// </summary>
public class ItemWeaponTable : TableBase<ItemWeapon>, IVersionableModel
{
    /// <inheritdoc/>
    public uint Version { get; set; } = 1;
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Weapon item secondary data. <see href="https://ffhacktics.com/wiki/Weapon_Secondary_Data"/>
/// </summary>
public class ItemWeapon : DiffableModelBase<ItemWeapon>, IDiffableModel<ItemWeapon>, IIdentifiableModel
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

    public static Dictionary<string, DiffablePropertyItem<ItemWeapon>> PropertyMap { get; } = new()
    {
        [nameof(Range)] = new DiffablePropertyItem<ItemWeapon, byte?>(nameof(Range), i => i.Range, (i, v) => i.Range = v),
        [nameof(AttackFlags)] = new DiffablePropertyItem<ItemWeapon, WeaponAttackFlags?>(nameof(AttackFlags), i => i.AttackFlags, (i, v) => i.AttackFlags = v),
        [nameof(Formula)] = new DiffablePropertyItem<ItemWeapon, byte?>(nameof(Formula), i => i.Formula, (i, v) => i.Formula = v),
        [nameof(Unused_0x03)] = new DiffablePropertyItem<ItemWeapon, byte?>(nameof(Unused_0x03), i => i.Unused_0x03, (i, v) => i.Unused_0x03 = v),
        [nameof(Power)] = new DiffablePropertyItem<ItemWeapon, byte?>(nameof(Power), i => i.Power, (i, v) => i.Power = v),
        [nameof(Evasion)] = new DiffablePropertyItem<ItemWeapon, byte?>(nameof(Evasion), i => i.Evasion, (i, v) => i.Evasion = v),
        [nameof(Elements)] = new DiffablePropertyItem<ItemWeapon, WeaponElementFlags?>(nameof(Elements), i => i.Elements, (i, v) => i.Elements = v),
        [nameof(OptionsAbilityId)] = new DiffablePropertyItem<ItemWeapon, byte?>(nameof(OptionsAbilityId), i => i.OptionsAbilityId, (i, v) => i.OptionsAbilityId = v),
    };

    public static ItemWeapon FromStructure(int id, ref ITEM_WEAPON_DATA @struct)
    {
        var data = new ItemWeapon()
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
    public ItemWeapon Clone()
    {
        return new ItemWeapon()
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

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
