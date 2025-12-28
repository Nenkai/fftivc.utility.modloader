using System.Runtime.InteropServices;

namespace fftivc.utility.modloader.Interfaces.Tables.Structures;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ITEM_WEAPON_DATA
{
    public byte Range { get; set; }
    public WeaponAttackFlags AttackFlags { get; set; }
    public byte Formula { get; set; } // See https://ffhacktics.com/wiki/Formulas
    public byte Unused_0x03 { get; set; }
    public byte Power { get; set; }
    public byte Evasion { get; set; }
    public WeaponElementFlags Elements { get; set; }
    public byte StatusEffectIdOrAbilityId { get; set; }
}

[Flags]
public enum WeaponAttackFlags : byte
{
    Striking = 1 << 7,
    Lunging = 1 << 6,
    Direct = 1 << 5,
    Arc = 1 << 4,
    TwoSwords = 1 << 3,
    TwoHands = 1 << 2,
    Throwable = 1 << 1,
    ForcedTwoHands = 1 << 0,
}

[Flags]
public enum WeaponElementFlags : byte
{
    Fire = 1 << 7,
    Lightning = 1 << 6,
    Ice = 1 << 5,
    Wind = 1 << 4,
    Earth = 1 << 3,
    Water = 1 << 2,
    Holy = 1 << 1,
    Dark = 1 << 0
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member