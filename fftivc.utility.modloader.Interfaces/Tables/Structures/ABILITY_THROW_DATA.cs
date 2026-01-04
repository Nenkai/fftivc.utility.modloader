using System.Runtime.InteropServices;

namespace fftivc.utility.modloader.Interfaces.Tables.Structures;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// ABILITY_THROW_DATA
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ABILITY_THROW_DATA
{
    public ThrowItemType ItemType { get; set; }
}

public enum ThrowItemType : byte
{
    None = 0,
    Knife,
    NinjaBlade,
    Sword,
    KnightSword,
    Katana,
    Axe,
    Rod,
    Staff,
    Flail,
    Gun,
    Crossbow,
    Bow,
    Instrument,
    Book,
    Polearm,
    Pole,
    Bag,
    Cloth,
    Shield,
    Helmet,
    Hat,
    HairAdornment,
    Armor,
    Clothing,
    Robe,
    Shoes,
    Armguard,
    Ring,
    Armlet,
    Cloak,
    Perfume,
    Throwing,
    Bomb,
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member