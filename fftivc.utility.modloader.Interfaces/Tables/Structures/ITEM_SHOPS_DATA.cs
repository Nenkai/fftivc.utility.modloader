using System.Runtime.InteropServices;

namespace fftivc.utility.modloader.Interfaces.Tables.Structures;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// ITEM_SHOPS
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ITEM_SHOPS_DATA
{
    public ShopFlags Shops { get; set; }
}

/// <summary>
/// Determines whether or not the category of items is equippable by the job (Set 1)
/// </summary>
[Flags]
public enum ShopFlags : ushort
{
    None = 0,

    Gollund = 1 << 15,
    Dorter = 1 << 14,
    Zaland = 1 << 13,
    Goug = 1 << 12,
    Warjilis = 1 << 11,
    Bervenia = 1 << 10,
    SalGhidos = 1 << 9,
    Unused = 1 << 8,

    Lesalia = 1 << 7,
    Riovanes = 1 << 6,
    Eagrose = 1 << 5,
    Lionel = 1 << 4,
    Limberry = 1 << 3,
    Zeltennia = 1 << 2,
    Gariland = 1 << 1,
    Yardrow = 1 << 0,
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member